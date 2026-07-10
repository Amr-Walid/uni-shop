using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IContentService _contentService;

        public OrdersController(IOrderService orderService, IProductService productService, IContentService contentService)
        {
            _orderService = orderService;
            _productService = productService;
            _contentService = contentService;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout([FromBody] ApiCheckoutRequest request)
        {
            if (request.Items == null || !request.Items.Any())
                return BadRequest("سلة التسوق فارغة");

            if (string.IsNullOrEmpty(request.CustomerName) || string.IsNullOrEmpty(request.CustomerPhone) || string.IsNullOrEmpty(request.Address))
                return BadRequest("البيانات الشخصية الأساسية مطلوبة (الاسم، الهاتف، العنوان)");

            // Build CartDto programmatically based on sent items
            var cartItems = new List<CartItemDto>();
            foreach (var item in request.Items)
            {
                var product = await _productService.GetByIdAsync(item.ProductId);
                if (product != null && product.IsActive)
                {
                    cartItems.Add(new CartItemDto
                    {
                        ProductId = product.Id,
                        ProductName = product.NameAr,
                        Emoji = product.Emoji,
                        ImagePath = product.ImagePath,
                        BrandName = product.BrandName,
                        UnitPrice = product.Price,
                        Quantity = item.Quantity
                    });
                }
            }

            if (!cartItems.Any())
                return BadRequest("لا توجد منتجات صالحة في سلة التسوق");

            var shippingFeeStr = await _contentService.GetSettingAsync("shipping.fee", "150");
            var shippingFee = decimal.TryParse(shippingFeeStr, out var fee) ? fee : 150m;

            var freeAboveStr = await _contentService.GetSettingAsync("shipping.free.above", "5000");
            var freeAbove = decimal.TryParse(freeAboveStr, out var fa) ? fa : 5000m;

            var cartDto = new CartDto { Items = cartItems, ShippingFee = shippingFee };
            if (cartDto.SubTotal >= freeAbove) cartDto.ShippingFee = 0;

            var checkoutDto = new CheckoutDto
            {
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                CustomerEmail = request.CustomerEmail,
                Address = request.Address,
                City = request.City,
                Governorate = request.Governorate,
                Notes = request.Notes
            };

            var order = await _orderService.CreateOrderAsync(checkoutDto, cartDto);
            return Ok(new { success = true, orderNumber = order.OrderNumber });
        }

        public class ApiCheckoutRequest
        {
            public string CustomerName { get; set; } = "";
            public string CustomerPhone { get; set; } = "";
            public string? CustomerEmail { get; set; }
            public string Address { get; set; } = "";
            public string City { get; set; } = "";
            public string Governorate { get; set; } = "";
            public string? Notes { get; set; }
            public List<ApiCartItem> Items { get; set; } = new();
        }

        public class ApiCartItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }
}
