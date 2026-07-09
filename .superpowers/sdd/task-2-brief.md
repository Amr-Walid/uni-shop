# Task 2: Create FTD.Application Layer

**Goal:** Create the Application Layer holding interfaces, DTOs, mappers, and services returning DTOs.

**Files:**
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/FTD.Application.csproj`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/Interfaces/IAppDbContext.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/Interfaces/IEmailService.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/DTOs/DTOs.cs` (ProductDto, CategoryDto, BrandDto, SalesOrderDto, SalesOrderDetailDto, NavigationItemDto, ContactInfoDto, ContentPageDto, PageSectionDto, SiteSettingDto, ContactMessageDto, etc.)
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/Mappers/MappingExtensions.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/Services/ProductService.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/Services/ContentService.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Application/Services/OrderService.cs`

- [ ] **Step 1: Scaffold FTD.Application project**
  Run: `dotnet new classlib -o FTD.Application` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
- [ ] **Step 2: Add references and solution association**
  Run: `dotnet sln FTD.Web/FTD.Web.sln add FTD.Application/FTD.Application.csproj` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
  Run: `dotnet add FTD.Application/FTD.Application.csproj reference FTD.Domain/FTD.Domain.csproj` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
- [ ] **Step 3: Define Interfaces**
  Create `IAppDbContext.cs` with EF Core sets mapped to interfaces, plus `SaveChangesAsync()`.
  Create `IEmailService.cs` with `Task SendContactNotificationAsync(string name, string email, string phone, string message)`.
- [ ] **Step 4: Create DTOs and Mappers**
  Create `DTOs.cs` defining:
  - `ProductDto`, `CategoryDto`, `BrandDto`, `SalesOrderDto`, `SalesOrderDetailDto`, `NavigationItemDto`, `ContactInfoDto`, `ContentPageDto`, `PageSectionDto`, `SiteSettingDto`, `ContactMessageDto`.
  Create `MappingExtensions.cs` containing manual mapper extension methods for these DTOs.
- [ ] **Step 5: Move and Refactor Services**
  Move the services from [Services.cs](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Web/Services/Services.cs) into separate class files inside `FTD.Application/Services/` (e.g. `ProductService.cs`, `ContentService.cs`, `OrderService.cs`).
  Refactor them to use `IAppDbContext` instead of direct `AppDbContext` dependencies, and return DTO classes. Make sure namespaces are `FTD.Application.Services`.
- [ ] **Step 6: Verify Application builds**
  Run: `dotnet build FTD.Application/FTD.Application.csproj`
  Expected: BUILD SUCCESS.
