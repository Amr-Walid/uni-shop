### Task 2: Configure AppSettings, JWT, CORS, and Services in Program.cs
**Files:**
- Create: `FTD.Api/appsettings.json`
- Create: `FTD.Api/Program.cs`
- Create: `FTD.Api/Properties/launchSettings.json`

**Interfaces:**
- Consumes: DbContext from FTD.Infrastructure, services from FTD.Application
- Produces: Configured runtime pipeline supporting REST API, JWT validation, and CORS.

- [ ] **Step 1: Create FTD.Api/appsettings.json**
  Write configuration file containing connection strings and JWT configuration:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Information",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "AllowedHosts": "*",
    "ConnectionStrings": {
      "DefaultConnection": "Server=localhost;Database=FTD_TechZone;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
    },
    "JwtSettings": {
      "Secret": "A_VERY_LONG_AND_SECURE_JWT_SECRET_KEY_FOR_FTD_TECHZONE_2026_JWT_MUST_BE_AT_LEAST_256_BITS",
      "Issuer": "FTD.Api",
      "Audience": "FTD.Client",
      "ExpiryMinutes": 120
    },
    "EmailSettings": {
      "SmtpHost": "smtp.gmail.com",
      "SmtpPort": 587,
      "SenderEmail": "",
      "SenderName": "FTD TechZone API",
      "Password": "",
      "NotifyEmail": ""
    }
  }
  ```

- [ ] **Step 2: Create FTD.Api/Properties/launchSettings.json**
  Define local development ports (e.g., http port 5100, https port 7100):
  ```json
  {
    "profiles": {
      "http": {
        "commandName": "Project",
        "dotnetRunMessages": true,
        "launchBrowser": false,
        "applicationUrl": "http://localhost:5100",
        "environmentVariables": {
          "ASPNETCORE_ENVIRONMENT": "Development"
        }
      }
    }
  }
  ```

- [ ] **Step 3: Create FTD.Api/Program.cs**
  Implement dependency injection, identity configuration, CORS policy, and JWT validation middleware:
  ```csharp
  using FTD.Application.Interfaces;
  using FTD.Application.Services;
  using FTD.Infrastructure.Data;
  using FTD.Infrastructure.Services;
  using Microsoft.AspNetCore.Authentication.JwtBearer;
  using Microsoft.AspNetCore.Identity;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.IdentityModel.Tokens;
  using System.Text;

  var builder = WebApplication.CreateBuilder(args);

  // Add Controllers
  builder.Services.AddControllers();

  // Database Connection
  builder.Services.AddDbContext<AppDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

  builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

  // Identity Configuration
  builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
  {
      options.Password.RequireDigit = true;
      options.Password.RequiredLength = 8;
      options.Password.RequireUppercase = false;
      options.Password.RequireNonAlphanumeric = false;
      options.SignIn.RequireConfirmedAccount = false;
  })
  .AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();

  // JWT Configuration
  var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
  var jwtIssuer = builder.Configuration["JwtSettings:Issuer"];
  var jwtAudience = builder.Configuration["JwtSettings:Audience"];
  var key = Encoding.ASCII.GetBytes(jwtSecret);

  builder.Services.AddAuthentication(options =>
  {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
  })
  .AddJwtBearer(options =>
  {
      options.RequireHttpsMetadata = false;
      options.SaveToken = true;
      options.TokenValidationParameters = new TokenValidationParameters
      {
          ValidateIssuerSigningKey = true,
          IssuerSigningKey = new SymmetricSecurityKey(key),
          ValidateIssuer = true,
          ValidIssuer = jwtIssuer,
          ValidateAudience = true,
          ValidAudience = jwtAudience,
          ValidateLifetime = true,
          ClockSkew = TimeSpan.Zero
      };
  });

  // CORS Policy
  builder.Services.AddCors(options =>
  {
      options.AddPolicy("AllowAll", policy =>
      {
          policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
      });
  });

  // Application Services Registration
  builder.Services.AddScoped<IProductService, ProductService>();
  builder.Services.AddScoped<IContentService, ContentService>();
  builder.Services.AddScoped<IOrderService, OrderService>();
  builder.Services.AddScoped<ICartService, CartService>();
  builder.Services.AddScoped<IMessageService, MessageService>();
  builder.Services.AddScoped<IDashboardService, DashboardService>();

  // Email Configuration
  var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
  builder.Services.AddSingleton(emailSettings);
  builder.Services.AddScoped<IEmailService, EmailService>();

  var app = builder.Build();

  app.UseHttpsRedirection();
  app.UseCors("AllowAll");
  app.UseAuthentication();
  app.UseAuthorization();

  app.MapControllers();

  app.Run();
  ```

- [ ] **Step 4: Verify Project Compilation**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 5: Commit changes**
  Run:
  ```bash
  git add FTD.Api/appsettings.json FTD.Api/Program.cs FTD.Api/Properties/launchSettings.json
  git commit -m "feat: configure FTD.Api appsettings, launchsettings, CORS, and Program.cs pipeline"
  ```
