# Task 4: Refactor FTD.Web Presentation Layer

**Goal:** Refactor the FTD.Web project to consume services/DTOs and strictly update MVC views to use DTOs.

**Files:**
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/FTD.Web.csproj`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Program.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Services/CartService.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/HomeController.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/ProductsController.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/CartOrderController.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/Admin/AdminControllers.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/Admin/AdminAttributesAndSectionsControllers.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/Admin/AdminBrandsController.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/Admin/AdminMessagesController.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Controllers/Admin/AdminNavigationController.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Views/Shared/_ProductCard.cshtml`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Views/Products/Detail.cshtml`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Views/Products/Index.cshtml`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Views/Cart/Index.cshtml`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/Views/Order/Checkout.cshtml`

- [ ] **Step 1: Update FTD.Web.csproj references**
  Add project references to `FTD.Application` and `FTD.Infrastructure`. Remove direct EF Core package references.
- [ ] **Step 2: Update Program.cs configuration**
  Register `IAppDbContext` mapped to `AppDbContext`, and `IEmailService` mapped to `EmailService`. Register Application Services (`ProductService`, `ContentService`, `OrderService`).
- [ ] **Step 3: Refactor CartService**
  Refactor `FTD.Web/Services/CartService.cs` (or rather clean up old duplicate service files if any, and make sure the remaining `CartService` in FTD.Web queries through the Application layers and uses DTOs).
- [ ] **Step 4: Refactor Controllers**
  Refactor all Web controllers (public and admin) to inject the application layer services and interact strictly with DTOs and ViewModels. Remove the old services folder from FTD.Web.
- [ ] **Step 5: Refactor Views and Partials**
  Update `@model` statements of all `.cshtml` files under `Views` (such as `_ProductCard.cshtml`, `Detail.cshtml`, `Index.cshtml`, `Cart/Index.cshtml`, etc.) to consume DTOs instead of domain entities directly.
- [ ] **Step 6: Verify solution builds**
  Run: `dotnet build FTD.Web/FTD.Web.sln`
  Expected: BUILD SUCCESS.
