# Task 2 Brief: Create DashboardService in FTD.Application

## Files to create/modify
- Modify: `FTD.Application/DTOs/DTOs.cs`
- Create: `FTD.Application/Services/DashboardService.cs`

## Requirements

1. **Modify `DTOs.cs`**:
   - Add the following DTO classes inside `FTD.Application.DTOs` namespace (just before the closing brace):
     ```csharp
     public class DashboardDto
     {
         public int TotalProducts { get; set; }
         public int TotalOrders { get; set; }
         public int NewOrders { get; set; }
         public int PendingOrders { get; set; }
         public decimal TodayRevenue { get; set; }
         public decimal MonthRevenue { get; set; }
         public List<SalesOrderDto> RecentOrders { get; set; } = new();
         public List<OrderStatusCountDto> OrdersByStatus { get; set; } = new();
     }

     public class OrderStatusCountDto
     {
         public string StatusName { get; set; } = "";
         public string ColorHex { get; set; } = "";
         public int Count { get; set; }
     }
     ```

2. **Create `DashboardService.cs`**:
   - Save in: `FTD.Application/Services/DashboardService.cs`
   - Implement `DashboardService` class:
     - It should take `IAppDbContext` in constructor.
     - Implement: `public async Task<DashboardDto> GetDashboardDataAsync()`
     - Code logic:
       - `var today = DateTime.UtcNow.Date;`
       - `var monthStart = new DateTime(today.Year, today.Month, 1);`
       - Query 10 most recent orders including Status:
         ```csharp
         var recentOrders = await _db.SalesOrders
             .Include(o => o.Status)
             .OrderByDescending(o => o.CreatedAt)
             .Take(10).ToListAsync();
         ```
       - Return a new `DashboardDto` filled with:
         - `TotalProducts`: count of active products (`p.IsActive`).
         - `TotalOrders`: count of all sales orders.
         - `NewOrders`: count of orders with `StatusId == 1`.
         - `PendingOrders`: count of orders with `StatusId == 3 || StatusId == 4`.
         - `TodayRevenue`: sum of `TotalAmount` for orders created today (`o.CreatedAt.Date == today`) and status not cancelled/returned (`o.StatusId != 7`). (Use nullable decimal cast `(decimal?)o.TotalAmount` to handle empty sums).
         - `MonthRevenue`: sum of `TotalAmount` for orders created since `monthStart` (`o.CreatedAt >= monthStart`) and status not cancelled/returned (`o.StatusId != 7`).
         - `RecentOrders`: mapped `recentOrders` using `ToDto()`.
         - `OrdersByStatus`: count of orders grouped by status from `_db.OrderStatuses`:
           ```csharp
           OrdersByStatus = await _db.OrderStatuses
               .Select(s => new OrderStatusCountDto
               {
                   StatusName = s.NameAr,
                   ColorHex = s.ColorHex,
                   Count = s.Orders.Count
               }).ToListAsync()
           ```

## Verification
- Run `dotnet build` from the solution root or `FTD.Application` directory to ensure compilation succeeds.
