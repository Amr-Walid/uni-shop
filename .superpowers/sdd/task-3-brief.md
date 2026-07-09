# Task 3: Create FTD.Infrastructure Layer

**Goal:** Create the Infrastructure Layer holding EF Core AppDbContext implementation, Migrations, and SMTP EmailService.

**Files:**
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Infrastructure/FTD.Infrastructure.csproj`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Infrastructure/Data/AppDbContext.cs`
- Create: `c:/Users/dell/Documents/unigroup/New folder/FTD.Infrastructure/Services/EmailService.cs`
- Move: `c:/Users/dell/Documents/unigroup/New folder/FTD.Infrastructure/Migrations/`

- [ ] **Step 1: Scaffold FTD.Infrastructure project**
  Run: `dotnet new classlib -o FTD.Infrastructure` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
- [ ] **Step 2: Add references and EF Core packages**
  Run: `dotnet sln FTD.Web/FTD.Web.sln add FTD.Infrastructure/FTD.Infrastructure.csproj` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
  Run: `dotnet add FTD.Infrastructure/FTD.Infrastructure.csproj reference FTD.Application/FTD.Application.csproj` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
  Run: `dotnet add FTD.Infrastructure/FTD.Infrastructure.csproj reference FTD.Domain/FTD.Domain.csproj` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
  Run: `dotnet add FTD.Infrastructure/FTD.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 9.0.0` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
  Run: `dotnet add FTD.Infrastructure/FTD.Infrastructure.csproj package Microsoft.AspNetCore.Identity.EntityFrameworkCore --version 9.0.0` (CWD: `c:\Users\dell\Documents\unigroup\New folder`)
- [ ] **Step 3: Refactor and Move AppDbContext**
  Move the DB context file from [AppDbContext.cs](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Web/Data/AppDbContext.cs) to `FTD.Infrastructure/Data/AppDbContext.cs`.
  Implement `IAppDbContext` interface on `AppDbContext`. Update model binding namespaces to `FTD.Domain.Entities`. Ensure the namespace of `AppDbContext` is `FTD.Infrastructure.Data`.
- [ ] **Step 4: Move Migrations folder**
  Move all migrations from [Migrations](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Web/Migrations) to `FTD.Infrastructure/Migrations/`. Update namespaces inside all migration files to match `FTD.Infrastructure.Migrations`.
- [ ] **Step 5: Move EmailService**
  Move [EmailService.cs](file:///c:/Users/dell/Documents/unigroup/New%20folder/FTD.Web/Services/EmailService.cs) to `FTD.Infrastructure/Services/EmailService.cs`. Implement `IEmailService` interface. Update namespace to `FTD.Infrastructure.Services`.
- [ ] **Step 6: Verify Infrastructure builds**
  Run: `dotnet build FTD.Infrastructure/FTD.Infrastructure.csproj`
  Expected: BUILD SUCCESS.
