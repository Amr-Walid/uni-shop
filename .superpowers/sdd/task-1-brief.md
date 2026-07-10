### Task 1: Scaffold FTD.Api Project
**Files:**
- Create: `FTD.Api/FTD.Api.csproj`
- Modify: `FTD.Web/FTD.Web.sln` (Register project)

**Interfaces:**
- Consumes: `FTD.Application/FTD.Application.csproj`, `FTD.Infrastructure/FTD.Infrastructure.csproj`
- Produces: Base project structure for FTD.Api

- [ ] **Step 1: Create FTD.Api project file**
  Create `FTD.Api/FTD.Api.csproj` with the following content:
  ```xml
  <Project Sdk="Microsoft.NET.Sdk.Web">

    <PropertyGroup>
      <TargetFramework>net9.0</TargetFramework>
      <Nullable>enable</Nullable>
      <ImplicitUsings>enable</ImplicitUsings>
      <RootNamespace>FTD.Api</RootNamespace>
    </PropertyGroup>

    <ItemGroup>
      <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
      <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
    </ItemGroup>

    <ItemGroup>
      <ProjectReference Include="..\FTD.Application\FTD.Application.csproj" />
      <ProjectReference Include="..\FTD.Infrastructure\FTD.Infrastructure.csproj" />
    </ItemGroup>

  </Project>
  ```

- [ ] **Step 2: Add FTD.Api project to the Solution**
  Run: `dotnet sln FTD.Web/FTD.Web.sln add FTD.Api/FTD.Api.csproj`
  Expected: Command returns successfully, adding FTD.Api to the solution configuration.

- [ ] **Step 3: Verify Solution Build**
  Run: `dotnet build FTD.Web/FTD.Web.sln`
  Expected: Build succeeds with 0 errors and 0 warnings.

- [ ] **Step 4: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/FTD.Api.csproj FTD.Web/FTD.Web.sln
  git commit -m "chore: scaffold FTD.Api project and link to solution"
  ```
