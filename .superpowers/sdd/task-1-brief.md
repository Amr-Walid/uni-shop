# Task 1: Create FTD.Domain Layer

**Goal:** Create a clean Domain project holding the entities without external web or DB context references.

**Files:**
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/FTD.Domain.csproj`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/Brand.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/Category.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/Product.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/ProductImage.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/ProductAttribute.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/AttributeValue.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/ProductAttributeValue.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/OrderStatus.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/SalesOrder.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/SalesOrderDetail.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/ContentBlock.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/ContentPage.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/PageSection.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/NavigationItem.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/ContactInfo.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/ContactMessage.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Domain/Entities/SiteSetting.cs`
- Modify: `c:/Users/dell/Documents/unigroup/New folder/FTD.Web/FTD.Web.sln`

- [ ] **Step 1: Scaffold FTD.Domain project**
  Run: `dotnet new classlib -o FTD.Domain` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
- [ ] **Step 2: Add FTD.Domain to the solution file**
  Run: `dotnet sln FTD.Web/FTD.Web.sln add FTD.Domain/FTD.Domain.csproj` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
- [ ] **Step 3: Extract Domain Entities**
  Extract all domain classes from [DomainModels.cs](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Web/Models/DomainModels.cs) into individual files inside `FTD.Domain/Entities/`. Use namespace `FTD.Domain.Entities`. Remove web-specific properties such as `IFormFile` from entities.
- [ ] **Step 4: Verify Domain builds**
  Run: `dotnet build FTD.Domain/FTD.Domain.csproj`
  Expected: BUILD SUCCESS.
- [ ] **Step 5: Commit Domain setup**
  Run: `git add FTD.Domain/` followed by a commit.
