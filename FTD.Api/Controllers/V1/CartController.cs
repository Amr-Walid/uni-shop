using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Api.Controllers.V1
{
    /// <summary>
    /// Server-side cart for signed-in customers, plus a public validation
    /// endpoint used by guests before checkout.
    /// </summary>
    [ApiController]
    [Route("api/v1/cart")]
    [Tags("Cart")]
    public class CartController : ApiControllerBase
    {
        private readonly IAppDbContext _db;
        private readonly ICartService _cartService;
        private readonly IProductService _products;
        private readonly IContentService _content;

        public CartController(
            IAppDbContext db,
            ICartService cartService,
            IProductService products,
            IContentService content)
        {
            _db = db;
            _cartService = cartService;
            _products = products;
            _content = content;
        }

        /// <summary>The signed-in customer's cart with server-computed totals.</summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCart()
        {
            var storage = await CreateStorageAsync();
            var cart = await _cartService.GetCartAsync(storage);
            return Ok(MapCart(cart));
        }

        /// <summary>Adds a product to the cart (quantities accumulate).</summary>
        /// <response code="404">Product missing or inactive (PRODUCT_NOT_FOUND).</response>
        [HttpPost("items")]
        [Authorize]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequestDto request)
        {
            // Validate against the catalog first so the stored cart can never
            // reference a product the customer is unable to buy.
            var product = await _products.GetByIdAsync(request.ProductId);
            if (product == null)
                return NotFoundProblem("المنتج غير موجود", ApiErrorCodes.ProductNotFound);

            if (!product.IsActive)
                return BadRequestProblem($"المنتج ({product.NameAr}) غير متاح حالياً", ApiErrorCodes.ProductInactive);

            var storage = await CreateStorageAsync();
            _cartService.AddItem(storage, request.ProductId, request.Quantity);
            await storage.SaveAsync();

            var cart = await _cartService.GetCartAsync(storage);
            return Ok(MapCart(cart));
        }

        /// <summary>Sets the quantity of a cart line. Quantity 0 removes it.</summary>
        [HttpPut("items/{productId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateItem(int productId, [FromBody] UpdateCartItemRequestDto request)
        {
            var storage = await CreateStorageAsync();
            _cartService.UpdateQty(storage, productId, request.Quantity);
            await storage.SaveAsync();

            var cart = await _cartService.GetCartAsync(storage);
            return Ok(MapCart(cart));
        }

        /// <summary>Removes a line from the cart.</summary>
        [HttpDelete("items/{productId:int}")]
        [Authorize]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var storage = await CreateStorageAsync();
            _cartService.RemoveItem(storage, productId);
            await storage.SaveAsync();

            var cart = await _cartService.GetCartAsync(storage);
            return Ok(MapCart(cart));
        }

        /// <summary>Empties the cart.</summary>
        [HttpDelete]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearCart()
        {
            var storage = await CreateStorageAsync();
            _cartService.ClearCart(storage);
            await storage.SaveAsync();

            return Ok(new { success = true, message = "تم إفراغ السلة" });
        }

        /// <summary>
        /// Merges a guest cart into the signed-in account's cart.
        ///
        /// Called right after login so items collected before signing in are not
        /// lost. Quantities are summed by default (never silently discarding the
        /// customer's selections); pass <c>replace: true</c> to overwrite.
        /// </summary>
        [HttpPost("merge")]
        [Authorize]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> MergeCart([FromBody] MergeCartRequestDto request)
        {
            var storage = await CreateStorageAsync();

            if (request.Replace)
                _cartService.ClearCart(storage);

            // Only merge products that still exist and are purchasable — a guest
            // cart can be days old.
            foreach (var item in request.Items)
            {
                var product = await _products.GetByIdAsync(item.ProductId);
                if (product == null || !product.IsActive) continue;

                _cartService.AddItem(storage, item.ProductId, item.Quantity);
            }

            await storage.SaveAsync();

            var cart = await _cartService.GetCartAsync(storage);
            return Ok(MapCart(cart));
        }

        /// <summary>
        /// Re-validates a client-held cart against live catalog data and returns
        /// authoritative totals.
        ///
        /// Public because guests check out too. This is what prevents the
        /// customer from filling in their whole address and only then being told
        /// an item is unavailable or its price changed.
        /// </summary>
        [HttpPost("validate")]
        [ProducesResponseType(typeof(CartValidationResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateCart([FromBody] ValidateCartRequestDto request)
        {
            var result = new CartValidationResultDto();
            var validItems = new List<CartItemDto>();

            foreach (var requested in request.Items)
            {
                var product = await _products.GetByIdAsync(requested.ProductId);

                if (product == null || !product.IsActive)
                {
                    result.IsValid = false;
                    result.Issues.Add(new CartItemIssueDto
                    {
                        ProductId = requested.ProductId,
                        ProductName = product?.NameAr ?? $"#{requested.ProductId}",
                        IssueCode = "REMOVED",
                        Message = "هذا المنتج لم يعد متاحاً وتم إزالته من السلة"
                    });
                    continue;
                }

                if (product.Stock <= 0)
                {
                    result.IsValid = false;
                    result.Issues.Add(new CartItemIssueDto
                    {
                        ProductId = product.Id,
                        ProductName = product.NameAr,
                        IssueCode = "OUT_OF_STOCK",
                        Message = "نفدت الكمية من هذا المنتج",
                        RequestedQuantity = requested.Quantity,
                        AvailableQuantity = 0
                    });
                    continue;
                }

                // Clamp instead of rejecting: keeping what IS available lets the
                // customer continue rather than starting over.
                var quantity = requested.Quantity;
                if (product.Stock < quantity)
                {
                    result.IsValid = false;
                    result.Issues.Add(new CartItemIssueDto
                    {
                        ProductId = product.Id,
                        ProductName = product.NameAr,
                        IssueCode = "QUANTITY_REDUCED",
                        Message = $"تم تعديل الكمية إلى {product.Stock} حسب المتاح في المخزن",
                        RequestedQuantity = requested.Quantity,
                        AvailableQuantity = product.Stock
                    });
                    quantity = product.Stock;
                }

                validItems.Add(new CartItemDto
                {
                    ProductId = product.Id,
                    ProductName = product.NameAr,
                    ProductNameEn = product.NameEn,
                    Emoji = product.Emoji,
                    // Absolute URL so the client can render the row directly.
                    ImagePath = MediaUrlHelper.ToAbsolute(product.ImagePath, MediaBaseUrl),
                    BrandName = product.BrandName,
                    // Authoritative CURRENT price — never the client's copy.
                    UnitPrice = product.Price,
                    Quantity = quantity
                });
            }

            var (shippingFee, freeAbove) = await GetShippingSettingsAsync();

            var cart = new CartDto
            {
                Items = validItems,
                ShippingFee = shippingFee,
                FreeShippingAbove = freeAbove
            };

            result.Items = validItems;
            result.SubTotal = cart.SubTotal;
            result.FreeShipping = cart.FreeShipping;
            result.ShippingFee = cart.FreeShipping ? 0 : shippingFee;
            result.Total = cart.Total;
            result.FreeShippingAbove = freeAbove;

            return Ok(result);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a database-backed cart store bound to the CURRENT token's
        /// user id. The id never comes from the request, so a caller cannot
        /// address another customer's cart.
        /// </summary>
        private async Task<DbCartStorage> CreateStorageAsync()
        {
            var storage = new DbCartStorage(_db, CurrentUserId!);
            // ICartStorage is synchronous by design (it was built for ISession),
            // so the row must be loaded before CartService touches it.
            await storage.LoadAsync();
            return storage;
        }

        private async Task<(decimal ShippingFee, decimal FreeAbove)> GetShippingSettingsAsync()
        {
            var feeRaw = await _content.GetSettingAsync("shipping.fee", "150");
            var fee = decimal.TryParse(feeRaw, out var parsedFee) ? parsedFee : 150m;

            var freeRaw = await _content.GetSettingAsync("shipping.free.above", "5000");
            var freeAbove = decimal.TryParse(freeRaw, out var parsedFree) ? parsedFree : 5000m;

            return (fee, freeAbove);
        }

        private CartResponseDto MapCart(CartDto cart)
        {
            // Rewrite stored relative image paths to absolute URLs for the client.
            foreach (var item in cart.Items)
                item.ImagePath = MediaUrlHelper.ToAbsolute(item.ImagePath, MediaBaseUrl);

            var remaining = cart.FreeShipping
                ? 0
                : Math.Max(0, cart.FreeShippingAbove - cart.SubTotal);

            return new CartResponseDto
            {
                Items = cart.Items,
                ItemsCount = cart.Items.Sum(i => i.Quantity),
                SubTotal = cart.SubTotal,
                ShippingFee = cart.FreeShipping ? 0 : cart.ShippingFee,
                Total = cart.Total,
                FreeShipping = cart.FreeShipping,
                FreeShippingAbove = cart.FreeShippingAbove,
                RemainingForFreeShipping = remaining
            };
        }
    }
}
