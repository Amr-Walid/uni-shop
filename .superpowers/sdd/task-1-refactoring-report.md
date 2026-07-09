# Task 1 Refactoring Report: Add Order Query Methods to OrderService

## Implementation Summary
The three database querying methods specified in the Task 1 brief have been successfully implemented inside the `OrderService` class in the `FTD.Application` project. These methods encapsulate queries against the EF Core DbContext, avoiding direct database querying in the Controllers/Web layer.

### Methods Added to `OrderService` in `FTD.Application/Services/OrderService.cs`:
1. **`GetOrdersAsync(int? statusId)`**
   - Retrieves sales orders from `_db.SalesOrders` including their related `Status`.
   - Optionally filters by `StatusId` if a value is supplied.
   - Orders descending by `CreatedAt`.
   - Maps entities to `SalesOrderDto` using the `ToDto()` extension method, filtering out any nulls.

2. **`GetOrderByIdAsync(int id)`**
   - Fetches a single sales order by ID.
   - Includes its `Status` and related `Details`, as well as the nested `Product` entity for each detail.
   - Returns the mapped `SalesOrderDto` or `null` if not found.

3. **`GetAllStatusesAsync()`**
   - Retrieves all order statuses ordered by `SortOrder`.
   - Maps them to `OrderStatusDto` using `ToDto()`.

---

## Code Modification Details
File: [OrderService.cs](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Application/Services/OrderService.cs)

```csharp
        public async Task<List<SalesOrderDto>> GetOrdersAsync(int? statusId)
        {
            var query = _db.SalesOrders.Include(o => o.Status).AsQueryable();

            if (statusId.HasValue)
            {
                query = query.Where(o => o.StatusId == statusId.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders
                .Select(o => o.ToDto())
                .OfType<SalesOrderDto>()
                .ToList();
        }

        public async Task<SalesOrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _db.SalesOrders
                .Include(o => o.Status)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order.ToDto();
        }

        public async Task<List<OrderStatusDto>> GetAllStatusesAsync()
        {
            var statuses = await _db.OrderStatuses
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return statuses
                .Select(s => s.ToDto())
                .OfType<OrderStatusDto>()
                .ToList();
        }
```

---

## Verification Results

A full compilation check was executed from the solution projects to ensure no compilation errors or broken references exist:

1. **`FTD.Application` Build:**
   - Command: `dotnet build` from `FTD.Application` directory
   - Result: **Build Succeeded**
   - Warnings: 0 new warnings (1 pre-existing nullable reference type assignment warning in `MappingExtensions.cs`).

2. **`FTD.Web` Build:**
   - Command: `dotnet build` from `FTD.Web` directory
   - Result: **Build Succeeded** (0 Warnings, 0 Errors).
