using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FTD.Api.Controllers.V1
{
    /// <summary>
    /// Checkout and order access: place an order, list your own orders, view
    /// detail, track as a guest, and cancel.
    /// </summary>
    [ApiController]
    [Route("api/v1/orders")]
    [Tags("Orders")]
    public class OrdersController : ApiControllerBase
    {
        private const string IdempotencyHeader = "Idempotency-Key";
        private const string CheckoutEndpointKey = "POST /api/v1/orders/checkout";

        /// <summary>
        /// Serializer options for the stored idempotency response.
        ///
        /// MUST match what MVC emits. Raw <c>JsonSerializer.Serialize</c> uses
        /// PascalCase, while MVC camel-cases property names — so a replayed
        /// response came back as <c>{"OrderNumber":...}</c> where the original
        /// was <c>{"orderNumber":...}</c>, and every client silently parsed the
        /// replay as nulls. The retry then looked like a failed order even
        /// though the order existed.
        /// </summary>
        private static readonly JsonSerializerOptions IdempotencyJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly IOrderService _orders;
        private readonly IProductService _products;
        private readonly IContentService _content;
        private readonly IIdempotencyService _idempotency;

        public OrdersController(
            IOrderService orders,
            IProductService products,
            IContentService content,
            IIdempotencyService idempotency)
        {
            _orders = orders;
            _products = products;
            _content = content;
            _idempotency = idempotency;
        }

        /// <summary>
        /// Places an order. Works for guests and for signed-in customers (in the
        /// latter case the order is linked to the account so it appears under
        /// "my orders").
        ///
        /// IDEMPOTENCY: send a unique <c>Idempotency-Key</c> header (one UUID per
        /// checkout attempt). Mobile networks drop responses, and the client
        /// cannot distinguish "not created" from "created but the reply was
        /// lost", so it retries — without a key that produces a duplicate order
        /// and deducts stock twice. Retrying with the same key replays the
        /// original result instead.
        /// </summary>
        /// <response code="201">Order created.</response>
        /// <response code="400">Cart invalid, product inactive or out of stock.</response>
        /// <response code="409">Idempotency key reused with a different payload.</response>
        [HttpPost("checkout")]
        [EnableRateLimiting("checkout-policy")]
        [ProducesResponseType(typeof(CheckoutResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Checkout([FromBody] MobileCheckoutRequestDto request)
        {
            // ── Idempotency pre-check ─────────────────────────────────────────
            var idempotencyKey = Request.Headers[IdempotencyHeader].ToString();
            var requestHash = _idempotency.ComputeRequestHash(JsonSerializer.Serialize(request));

            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var (found, conflict, responseJson, statusCode) =
                    await _idempotency.TryGetAsync(idempotencyKey, CheckoutEndpointKey, requestHash);

                if (conflict)
                {
                    return ConflictProblem(
                        "تم استخدام مفتاح العملية (Idempotency-Key) مع بيانات مختلفة. استخدم مفتاحاً جديداً.",
                        ApiErrorCodes.IdempotencyKeyConflict);
                }

                if (found && !string.IsNullOrEmpty(responseJson))
                {
                    // Replay the original response verbatim — the order already
                    // exists and must not be created a second time.
                    return new ContentResult
                    {
                        Content = responseJson,
                        ContentType = "application/json",
                        StatusCode = statusCode
                    };
                }
            }

            // ── Rebuild the cart from live catalog data ───────────────────────
            // Client-supplied prices are NEVER trusted: every unit price and
            // availability flag is re-read from the database here.
            var cartItems = new List<CartItemDto>();

            foreach (var line in request.Items)
            {
                var product = await _products.GetByIdAsync(line.ProductId);

                if (product == null)
                    return BadRequestProblem($"المنتج رقم {line.ProductId} غير موجود", ApiErrorCodes.ProductNotFound);

                if (!product.IsActive)
                    return BadRequestProblem($"المنتج ({product.NameAr}) غير متاح حالياً للشراء", ApiErrorCodes.ProductInactive);

                cartItems.Add(new CartItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.NameAr,
                    ProductNameEn = product.NameEn,
                    Emoji = product.Emoji,
                    ImagePath = product.ImagePath,
                    BrandName = product.BrandName,
                    UnitPrice = product.Price,
                    Quantity = line.Quantity
                });
            }

            if (cartItems.Count == 0)
                return BadRequestProblem("سلة التسوق فارغة", ApiErrorCodes.CartEmpty);

            var feeRaw = await _content.GetSettingAsync("shipping.fee", "150");
            var shippingFee = decimal.TryParse(feeRaw, out var parsedFee) ? parsedFee : 150m;

            var freeRaw = await _content.GetSettingAsync("shipping.free.above", "5000");
            var freeAbove = decimal.TryParse(freeRaw, out var parsedFree) ? parsedFree : 5000m;

            var cart = new CartDto
            {
                Items = cartItems,
                ShippingFee = shippingFee,
                FreeShippingAbove = freeAbove
            };
            if (cart.FreeShipping) cart.ShippingFee = 0;

            var checkout = new CheckoutDto
            {
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                CustomerEmail = request.CustomerEmail,
                Address = request.Address,
                City = request.City,
                Governorate = request.Governorate,
                Notes = request.Notes
            };

            // Stock verification and deduction happen inside the service, in the
            // same transaction as the insert. An InvalidOperationException
            // (e.g. insufficient stock) is translated by GlobalExceptionHandler
            // into a 400 carrying the Arabic message.
            var order = await _orders.CreateOrderAsync(checkout, cart, CurrentUserId);

            var statuses = await _orders.GetAllStatusesAsync();
            var status = statuses.FirstOrDefault(s => s.Id == order.StatusId);

            var response = new CheckoutResponseDto
            {
                Success = true,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                Total = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Status = status == null ? null : new OrderStatusSummaryDto
                {
                    Id = status.Id,
                    Name = LocalizationHelper.Pick(status.NameAr, status.NameEn, RequestLanguage),
                    ColorHex = status.ColorHex,
                    Icon = status.Icon,
                    SortOrder = status.SortOrder
                }
            };

            // Record the outcome so a retry replays it instead of re-ordering.
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                await _idempotency.SaveAsync(
                    idempotencyKey,
                    CheckoutEndpointKey,
                    requestHash,
                    JsonSerializer.Serialize(response, IdempotencyJsonOptions),
                    StatusCodes.Status201Created);
            }

            return StatusCode(StatusCodes.Status201Created, response);
        }

        /// <summary>One page of the signed-in customer's own orders.</summary>
        [HttpGet("my")]
        [Authorize]
        [ProducesResponseType(typeof(PagedResult<OrderListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders([FromQuery] int? page, [FromQuery] int? pageSize)
        {
            var (normalizedPage, normalizedSize) = PagedResult<OrderListItemDto>.Normalize(page, pageSize);

            var result = await _orders.GetMyOrdersAsync(
                CurrentUserId!, normalizedPage, normalizedSize, RequestLanguage, MediaBaseUrl);

            return Ok(result);
        }

        /// <summary>
        /// Detail of one of the caller's own orders.
        ///
        /// Ownership is enforced inside the query, and a foreign id returns 404
        /// rather than 403 so the endpoint never confirms that an order exists.
        /// </summary>
        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMyOrder(int id)
        {
            var order = await _orders.GetMyOrderDetailAsync(id, CurrentUserId!, RequestLanguage, MediaBaseUrl);

            return order == null
                ? NotFoundProblem("الطلب غير موجود", ApiErrorCodes.OrderNotFound)
                : Ok(order);
        }

        /// <summary>
        /// Tracks an order without signing in.
        /// </summary>
        /// <param name="orderNumber">The order number from the confirmation screen.</param>
        /// <param name="phone4">
        /// Last 4 digits of the phone number on the order. REQUIRED: order
        /// numbers follow a predictable pattern, so without this second factor
        /// anyone could enumerate numbers and read customers' names, phones and
        /// addresses.
        /// </param>
        [HttpGet("track/{orderNumber}")]
        [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TrackOrder(string orderNumber, [FromQuery] string? phone4)
        {
            if (string.IsNullOrWhiteSpace(phone4))
            {
                return BadRequestProblem(
                    "مطلوب آخر 4 أرقام من رقم الهاتف المستخدم في الطلب للتحقق",
                    ApiErrorCodes.ValidationFailed);
            }

            var order = await _orders.TrackOrderAsync(orderNumber, phone4, RequestLanguage, MediaBaseUrl);

            // Identical response for "no such order" and "wrong phone digits",
            // so the endpoint cannot be used to confirm which numbers are real.
            return order == null
                ? NotFoundProblem(
                    "لم نجد طلباً بهذا الرقم مع بيانات التحقق المدخلة",
                    ApiErrorCodes.OrderNotFound)
                : Ok(order);
        }

        /// <summary>
        /// Cancels one of the caller's orders and returns its stock to inventory.
        /// Allowed only while the order is still New or Confirmed.
        /// </summary>
        [HttpPost("{id:int}/cancel")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var (success, errorCode, message) = await _orders.CancelMyOrderAsync(id, CurrentUserId!);

            return success
                ? Ok(new { success = true, message })
                : ProblemFromCode(errorCode!, message!);
        }

        /// <summary>
        /// The full order-status ladder.
        ///
        /// Statuses are admin-editable data, so the client builds its progress
        /// tracker from this rather than hardcoding the workflow.
        /// </summary>
        [HttpGet("statuses")]
        [ProducesResponseType(typeof(List<OrderStatusSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _orders.GetAllStatusesAsync();
            var lang = RequestLanguage;

            return Ok(statuses.Select(s => new OrderStatusSummaryDto
            {
                Id = s.Id,
                Name = LocalizationHelper.Pick(s.NameAr, s.NameEn, lang),
                ColorHex = s.ColorHex,
                Icon = s.Icon,
                SortOrder = s.SortOrder
            }).ToList());
        }
    }
}
