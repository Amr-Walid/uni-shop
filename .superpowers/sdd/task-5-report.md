# Task 5 Report: Implement Public Checkout and Contact Endpoints

## What was implemented

1. **OrdersController (`FTD.Api/Controllers/OrdersController.cs`)**:
   - Created the endpoint `POST /api/orders/checkout` to process client checkout requests.
   - Handled validation of personal information (`CustomerName`, `CustomerPhone`, `Address`).
   - Reconstructed the `CartDto` on the server-side:
     - Fetched each product from the database via `IProductService.GetByIdAsync` to check if it exists and is active.
     - Added valid items to the cart, retrieving the unit price, brand name, name, emoji, and image path securely from the database.
   - Programmatically determined shipping fees and free shipping eligibility using configuration options fetched from `IContentService` ("shipping.fee" defaulting to 150 and "shipping.free.above" defaulting to 5000).
   - Created the order using `IOrderService.CreateOrderAsync`.
   - Returned the generated `OrderNumber` upon success.

2. **ContactController (`FTD.Api/Controllers/ContactController.cs`)**:
   - Created the endpoint `POST /api/contact` to submit customer message forms.
   - Checked that required parameters (`Name`, `Email`, `Message`) are supplied.
   - Called `IMessageService.SaveMessageAsync` to persist the message.
   - Returned a success response status.

## What was tested and test results

### Build Verification
Ran `dotnet build FTD.Api/FTD.Api.csproj` to compile the API project and its dependencies:

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  FTD.Domain -> C:\Users\dell\Documents\unigroup\New folder\FTD.Domain\bin\Debug\net9.0\FTD.Domain.dll
  FTD.Application -> C:\Users\dell\Documents\unigroup\New folder\FTD.Application\bin\Debug\net9.0\FTD.Application.dll
  FTD.Infrastructure -> C:\Users\dell\Documents\unigroup\New folder\FTD.Infrastructure\bin\Debug\net9.0\FTD.Infrastructure.dll
  FTD.Api -> C:\Users\dell\Documents\unigroup\New folder\FTD.Api\bin\Debug\net9.0\FTD.Api.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)
```

The build succeeded with 0 warnings and 0 errors.

## Files changed

- **Created**: [OrdersController.cs](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Api/Controllers/OrdersController.cs)
- **Created**: [ContactController.cs](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Api/Controllers/ContactController.cs)

## Self-review findings

- **Validation Robustness**: Both controllers perform inputs check before any business logic is executed. Basic checkout requirements (`CustomerName`, `CustomerPhone`, `Address`) and empty cart conditions return helpful Arabic warning messages consistent with the client UI language.
- **Security Check**: Product prices are fetched directly from the database during reconstruction of the cart to prevent price manipulation from client requests.
- **Configuration Flexibility**: The shipping fee and free-shipping limit parameters are dynamically fetched from the system settings stored in the database, allowing admin customization.

## Issues or concerns

None. The implementation is complete and conforms to the task requirements.

## Fixes Implemented

We resolved potential crash hazards when empty request bodies or null JSON payloads are sent:
1. **OrdersController.cs null check**:
   In the `Checkout` action, we added a check to verify if the `request` parameter itself is null before checking its `Items` property.
2. **ContactController.cs null check**:
   In the `SendMessage` action, we added a check to verify if the `dto` parameter is null before attempting to access its properties.

### Compilation Verification

We ran `dotnet build FTD.Web/FTD.Web.sln` to compile the entire solution:

```
  Determining projects to restore...
  All projects are up-to-date for restore.
  FTD.Domain -> C:\Users\dell\Documents\unigroup\New folder\FTD.Domain\bin\Debug\net9.0\FTD.Domain.dll
  FTD.Application -> C:\Users\dell\Documents\unigroup\New folder\FTD.Application\bin\Debug\net9.0\FTD.Application.dll
  FTD.Infrastructure -> C:\Users\dell\Documents\unigroup\New folder\FTD.Infrastructure\bin\Debug\net9.0\FTD.Infrastructure.dll
  FTD.Api -> C:\Users\dell\Documents\unigroup\New folder\FTD.Api\bin\Debug\net9.0\FTD.Api.dll
  FTD.Web -> C:\Users\dell\Documents\unigroup\New folder\FTD.Web\bin\Debug\net9.0\FTD.Web.dll

Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:02.63
```
The project compiles with 0 errors.

