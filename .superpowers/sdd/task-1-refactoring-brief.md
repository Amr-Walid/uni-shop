# Task 1 Brief: Add Order Query Methods to OrderService

## Files to modify
- `FTD.Application/Services/OrderService.cs`

## Requirements
Implement the following database querying methods inside `OrderService` class to encapsulate query logic within the Application layer:

1. `public async Task<List<SalesOrderDto>> GetOrdersAsync(int? statusId)`
   - Fetch sales orders from `_db.SalesOrders` including the navigation property `Status`.
   - If `statusId` is provided (not null), filter by `StatusId == statusId.Value`.
   - Order the results descending by `CreatedAt`.
   - Map each `SalesOrder` entity to `SalesOrderDto` using the `ToDto()` extension method, and return the list of non-null DTOs.

2. `public async Task<SalesOrderDto?> GetOrderByIdAsync(int id)`
   - Fetch the single sales order matching `id` from `_db.SalesOrders`.
   - Include `Status` and `Details` (with `Product` nested inclusion).
   - Return the mapped `SalesOrderDto` or `null` if not found.

3. `public async Task<List<OrderStatusDto>> GetAllStatusesAsync()`
   - Fetch all order statuses from `_db.OrderStatuses`.
   - Order by `SortOrder`.
   - Map each `OrderStatus` entity to `OrderStatusDto` using `ToDto()` and return the list.

## Verification
- Run `dotnet build` from the solution root or `FTD.Application` directory to ensure compilation succeeds.
