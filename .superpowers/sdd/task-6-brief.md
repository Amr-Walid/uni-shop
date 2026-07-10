### Task 6: Implement Secured Admin Dashboard and Orders Management API
**Files:**
- Create: `FTD.Api/Controllers/AdminController.cs`

**Interfaces:**
- Consumes: `IDashboardService`, `IOrderService`, Identity `UserManager<IdentityUser>`
- Produces: API endpoints `GET /api/admin/dashboard`, `GET /api/admin/orders`, `GET /api/admin/orders/{id}`, `POST /api/admin/orders/{id}/status`. All secured with `[Authorize(Roles = "Admin")]`.

- [ ] **Step 1: Create AdminController**
  Create `FTD.Api/Controllers/AdminController.cs`:
  ```csharp
  using FTD.Application.Interfaces;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      [Authorize(Roles = "Admin")]
      public class AdminController : ControllerBase
      {
          private readonly IDashboardService _dashboardService;
          private readonly IOrderService _orderService;

          public AdminController(IDashboardService dashboardService, IOrderService orderService)
          {
              _dashboardService = dashboardService;
              _orderService = orderService;
          }

          [HttpGet("dashboard")]
          public async Task<IActionResult> GetDashboardStats()
          {
              var stats = await _dashboardService.GetStatsAsync();
              return Ok(stats);
          }

          [HttpGet("orders")]
          public async Task<IActionResult> GetOrders([FromQuery] int? statusId)
          {
              var orders = await _orderService.GetOrdersAsync(statusId);
              return Ok(orders);
          }

          [HttpGet("orders/{id}")]
          public async Task<IActionResult> GetOrderDetail(int id)
          {
              var order = await _orderService.GetOrderByIdAsync(id);
              if (order == null)
                  return NotFound("الطلب غير موجود");
              return Ok(order);
          }

          [HttpPost("orders/{id}/status")]
          public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
          {
              if (request == null || request.StatusId <= 0)
                  return BadRequest("معرف الحالة غير صالح");

              var order = await _orderService.GetOrderByIdAsync(id);
              if (order == null)
                  return NotFound("الطلب غير موجود");

              await _orderService.UpdateStatusAsync(id, request.StatusId);
              return Ok(new { success = true, message = "تم تحديث حالة الطلب بنجاح" });
          }

          public class UpdateStatusRequest
          {
              public int StatusId { get; set; }
          }
      }
  }
  ```

- [ ] **Step 2: Verify Build**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 3: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/AdminController.cs
  git commit -m "feat: implement secured AdminController api endpoints for dashboard stats and order management"
  ```
