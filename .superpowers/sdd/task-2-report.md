# Task 2 Report: Configure AppSettings, JWT, CORS, and Services in Program.cs

## What was implemented
1. **FTD.Api/appsettings.json**:
   - Added configuration settings for Database Connection (`DefaultConnection`).
   - Added `JwtSettings` with a secure 256-bit secret key, issuer, audience, and expiration period (120 minutes).
   - Added SMTP settings under `EmailSettings`.
2. **FTD.Api/Properties/launchSettings.json**:
   - Created the launch settings profile configured to run the web API locally on port `5100` (`http://localhost:5100`).
3. **FTD.Api/Program.cs**:
   - Registered `AppDbContext` using SQL Server.
   - Configured Scoped registration for `IAppDbContext` resolved via `AppDbContext`.
   - Configured ASP.NET Core Identity with password complexity rules.
   - Set up JWT Bearer authentication with validation parameters (Symmetric key, issuer/audience validation, zero clock skew).
   - Configured a CORS policy named `"AllowAll"` to permit any origin, method, and headers.
   - Registered all Core business services (`IProductService`, `IContentService`, `IOrderService`, `ICartService`, `IMessageService`, `IDashboardService`) with Scoped lifetimes.
   - Registered `EmailSettings` as a Singleton and `IEmailService` (mapped to `EmailService`) as Scoped.
   - Arranged the middleware pipeline to use CORS, authentication, authorization, and controller routing.

## Test Results & Verification
- Ran local build for the `FTD.Api` project.
- **Build Output Evidence**:
  ```text
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

## Files Changed
- `FTD.Api/appsettings.json` (created)
- `FTD.Api/Properties/launchSettings.json` (created)
- `FTD.Api/Program.cs` (created/updated)

## Self-Review Findings
- Verified that all required namespaces (like `FTD.Application.Interfaces`, `FTD.Infrastructure.Services`, `FTD.Infrastructure.Data`) exist and match the actual implementation namespaces in the solution.
- Checked that Identity password requirements matches exactly with Task Brief requirements:
  - `RequireDigit = true`
  - `RequiredLength = 8`
  - `RequireUppercase = false`
  - `RequireNonAlphanumeric = false`
  - `SignIn.RequireConfirmedAccount = false`
- JWT validation logic utilizes `ClockSkew = TimeSpan.Zero` for exact token expiration enforcement.
- CORS policy matches `"AllowAll"`.

## Issues or Concerns
- None.
