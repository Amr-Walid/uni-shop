### Task 5: Implement Public Checkout and Contact Endpoints
**Files:**
- Create: `FTD.Api/Controllers/OrdersController.cs`
- Create: `FTD.Api/Controllers/ContactController.cs`

**Interfaces:**
- Consumes: `IOrderService`, `IMessageService`, `ICartService` (to calculate fees if needed, or directly compute in services)
- Produces: API endpoints `POST /api/orders/checkout`, `POST /api/contact`.

- [ ] **Step 1: Create OrdersController**
  Create `FTD.Api/Controllers/OrdersController.cs`:
  ```csharp
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
  ```

- [ ] **Step 2: Create ContactController**
  Create `FTD.Api/Controllers/ContactController.cs`:
  ```csharp
  using FTD.Application.DTOs;
  using FTD.Application.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      public class ContactController : ControllerBase
      {
          private readonly IMessageService _messageService;
          public ContactController(IMessageService messageService) => _messageService = messageService;

          [HttpPost]
          public async Task<IActionResult> SendMessage([FromBody] ContactMessageDto dto)
          {
              if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Message))
                  return BadRequest("جميع الحقول الإلزامية مطلوبة (الاسم، البريد الإلكتروني، الرسالة)");

              await _messageService.SaveMessageAsync(dto);
              return Ok(new { success = true, message = "تم إرسال الرسالة بنجاح" });
          }
      }
  }
  ```

- [ ] **Step 3: Verify Build**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 4: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/OrdersController.cs FTD.Api/Controllers/ContactController.cs
  git commit -m "feat: implement public orders checkout and contact messages api endpoints"
  ```
