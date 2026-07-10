# Web Service API (FTD.Api) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a separate FTD.Api project in the solution to expose public and secure (JWT-protected) endpoints for future client/mobile applications.

**Architecture:** Create an ASP.NET Core 9.0 Web API project that references FTD.Application and FTD.Infrastructure. Configure JWT Bearer Authentication and CORS. Implement public endpoints for product browsing/checkout, and secured endpoints (Roles="Admin") for admin management.

**Tech Stack:** ASP.NET Core 9.0 Web API, JWT Bearer Authentication, Entity Framework Core 9.0, SQL Server.

## Global Constraints
- Target Framework: net9.0
- NuGet Package: Microsoft.AspNetCore.Authentication.JwtBearer Version 9.0.0
- Solution Path: FTD.Web/FTD.Web.sln
- Connection String Name: DefaultConnection

---

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

---

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

---

### Task 3: Implement AuthController for Admin JWT Token Generation
**Files:**
- Create: `FTD.Api/Controllers/AuthController.cs`

**Interfaces:**
- Consumes: Identity `SignInManager<IdentityUser>`, Identity `UserManager<IdentityUser>`
- Produces: API endpoint `POST /api/auth/login` returning token.

- [ ] **Step 1: Create AuthController**
  Create `FTD.Api/Controllers/AuthController.cs`:
  ```csharp
  using Microsoft.AspNetCore.Identity;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.IdentityModel.Tokens;
  using System.IdentityModel.Tokens.Jwt;
  using System.Security.Claims;
  using System.Text;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      public class AuthController : ControllerBase
      {
          private readonly UserManager<IdentityUser> _userManager;
          private readonly SignInManager<IdentityUser> _signInManager;
          private readonly IConfiguration _config;

          public AuthController(
              UserManager<IdentityUser> userManager,
              SignInManager<IdentityUser> signInManager,
              IConfiguration config)
          {
              _userManager = userManager;
              _signInManager = signInManager;
              _config = config;
          }

          [HttpPost("login")]
          public async Task<IActionResult> Login([FromBody] LoginRequest request)
          {
              if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                  return BadRequest("البريد الإلكتروني وكلمة المرور مطلوبة");

              var user = await _userManager.FindByEmailAsync(request.Email);
              if (user == null)
                  return Unauthorized("البريد الإلكتروني أو كلمة المرور غير صحيحة");

              var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
              if (!result.Succeeded)
                  return Unauthorized("البريد الإلكتروني أو كلمة المرور غير صحيحة");

              var roles = await _userManager.GetRolesAsync(user);
              if (!roles.Contains("Admin"))
                  return Forbid("غير مصرح بالدخول لغير المسؤولين");

              var token = GenerateJwtToken(user, roles);
              return Ok(new
              {
                  token,
                  email = user.Email,
                  roles
              });
          }

          private string GenerateJwtToken(IdentityUser user, IList<string> roles)
          {
              var secret = _config["JwtSettings:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.");
              var issuer = _config["JwtSettings:Issuer"];
              var audience = _config["JwtSettings:Audience"];
              var expiryMinutesStr = _config["JwtSettings:ExpiryMinutes"];
              var expiryMinutes = double.TryParse(expiryMinutesStr, out var mins) ? mins : 120;

              var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
              var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

              var claims = new List<Claim>
              {
                  new Claim(ClaimTypes.NameIdentifier, user.Id),
                  new Claim(ClaimTypes.Email, user.Email ?? "")
              };

              foreach (var role in roles)
                  claims.Add(new Claim(ClaimTypes.Role, role));

              var tokenDescriptor = new SecurityTokenDescriptor
              {
                  Subject = new ClaimsIdentity(claims),
                  Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                  SigningCredentials = creds,
                  Issuer = issuer,
                  Audience = audience
              };

              var tokenHandler = new JwtSecurityTokenHandler();
              var token = tokenHandler.CreateToken(tokenDescriptor);
              return tokenHandler.WriteToken(token);
          }

          public class LoginRequest
          {
              public string Email { get; set; } = "";
              public string Password { get; set; } = "";
          }
      }
  }
  ```

- [ ] **Step 2: Verify Build**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 3: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/AuthController.cs
  git commit -m "feat: implement AuthController with JWT token generation for Admin users"
  ```

---

### Task 4: Implement Public Catalog Endpoints (Products, Categories, Brands)
**Files:**
- Create: `FTD.Api/Controllers/ProductsController.cs`

**Interfaces:**
- Consumes: `IProductService`, `IContentService`
- Produces: API endpoints `GET /api/products`, `GET /api/products/{slug}`, `GET /api/categories`, `GET /api/brands`.

- [ ] **Step 1: Create ProductsController**
  Create `FTD.Api/Controllers/ProductsController.cs`:
  ```csharp
  using FTD.Application.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      public class ProductsController : ControllerBase
      {
          private readonly IProductService _productService;
          public ProductsController(IProductService productService) => _productService = productService;

          [HttpGet]
          public async Task<IActionResult> GetProducts([FromQuery] int? categoryId, [FromQuery] int? brandId, [FromQuery] string? query)
          {
              var products = await _productService.GetFilteredAsync(categoryId, brandId, query);
              return Ok(products);
          }

          [HttpGet("{slug}")]
          public async Task<IActionResult> GetProductDetail(string slug)
          {
              var product = await _productService.GetBySlugAsync(slug);
              if (product == null)
                  return NotFound("المنتج غير موجود");
              return Ok(product);
          }

          [HttpGet("categories")]
          public async Task<IActionResult> GetCategories()
          {
              var categories = await _productService.GetActiveCategoriesAsync();
              return Ok(categories);
          }

          [HttpGet("brands")]
          public async Task<IActionResult> GetBrands()
          {
              var brands = await _productService.GetAllBrandsAsync();
              var activeBrands = brands.Where(b => b.IsActive).ToList();
              return Ok(activeBrands);
          }
      }
  }
  ```

- [ ] **Step 2: Verify Build**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 3: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/ProductsController.cs
  git commit -m "feat: implement public ProductsController api endpoints for catalog browsing"
  ```

---

### Task 5: Implement Public Checkout and Contact Endpoints
**Files:**
- Create: `FTD.Api/Controllers/OrdersController.cs`
- Create: `FTD.Api/Controllers/ContactController.cs`

**Interfaces:**
- Consumes: `IOrderService`, `IMessageService`, `ICartService` (to calculate fees if needed, or directly compute in services)
- Produces: API endpoints `POST /api/orders/checkout`, `POST /api/contact`.

- [ ] **Step 1: Create OrdersController**
  Create `FTD.Api/Controllers/OrdersController.cs`:
  ```csharp
  using FTD.Application.DTOs;
  using FTD.Application.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      public class OrdersController : ControllerBase
      {
          private readonly IOrderService _orderService;
          private readonly IProductService _productService;
          private readonly IContentService _contentService;

          public OrdersController(IOrderService orderService, IProductService productService, IContentService contentService)
          {
              _orderService = orderService;
              _productService = productService;
              _contentService = contentService;
          }

          [HttpPost("checkout")]
          public async Task<IActionResult> Checkout([FromBody] ApiCheckoutRequest request)
          {
              if (request.Items == null || !request.Items.Any())
                  return BadRequest("سلة التسوق فارغة");

              if (string.IsNullOrEmpty(request.CustomerName) || string.IsNullOrEmpty(request.CustomerPhone) || string.IsNullOrEmpty(request.Address))
                  return BadRequest("البيانات الشخصية الأساسية مطلوبة (الاسم، الهاتف، العنوان)");

              // Build CartDto programmatically based on sent items
              var cartItems = new List<CartItemDto>();
              foreach (var item in request.Items)
              {
                  var product = await _productService.GetByIdAsync(item.ProductId);
                  if (product != null && product.IsActive)
                  {
                      cartItems.Add(new CartItemDto
                      {
                          ProductId = product.Id,
                          ProductName = product.NameAr,
                          Emoji = product.Emoji,
                          ImagePath = product.ImagePath,
                          BrandName = product.BrandName,
                          UnitPrice = product.Price,
                          Quantity = item.Quantity
                      });
                  }
              }

              if (!cartItems.Any())
                  return BadRequest("لا توجد منتجات صالحة في سلة التسوق");

              var shippingFeeStr = await _contentService.GetSettingAsync("shipping.fee", "150");
              var shippingFee = decimal.TryParse(shippingFeeStr, out var fee) ? fee : 150m;

              var freeAboveStr = await _contentService.GetSettingAsync("shipping.free.above", "5000");
              var freeAbove = decimal.TryParse(freeAboveStr, out var fa) ? fa : 5000m;

              var cartDto = new CartDto { Items = cartItems, ShippingFee = shippingFee };
              if (cartDto.SubTotal >= freeAbove) cartDto.ShippingFee = 0;

              var checkoutDto = new CheckoutDto
              {
                  CustomerName = request.CustomerName,
                  CustomerPhone = request.CustomerPhone,
                  CustomerEmail = request.CustomerEmail,
                  Address = request.Address,
                  City = request.City,
                  Governorate = request.Governorate,
                  Notes = request.Notes
              };

              var order = await _orderService.CreateOrderAsync(checkoutDto, cartDto);
              return Ok(new { success = true, orderNumber = order.OrderNumber });
          }

          public class ApiCheckoutRequest
          {
              public string CustomerName { get; set; } = "";
              public string CustomerPhone { get; set; } = "";
              public string? CustomerEmail { get; set; }
              public string Address { get; set; } = "";
              public string City { get; set; } = "";
              public string Governorate { get; set; } = "";
              public string? Notes { get; set; }
              public List<ApiCartItem> Items { get; set; } = new();
          }

          public class ApiCartItem
          {
              public int ProductId { get; set; }
              public int Quantity { get; set; }
          }
      }
  }
  ```

- [ ] **Step 2: Create ContactController**
  Create `FTD.Api/Controllers/ContactController.cs`:
  ```csharp
  using FTD.Application.DTOs;
  using FTD.Application.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      public class ContactController : ControllerBase
      {
          private readonly IMessageService _messageService;
          public ContactController(IMessageService messageService) => _messageService = messageService;

          [HttpPost]
          public async Task<IActionResult> SendMessage([FromBody] ContactMessageDto dto)
          {
              if (string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Message))
                  return BadRequest("جميع الحقول الإلزامية مطلوبة (الاسم، البريد الإلكتروني، الرسالة)");

              await _messageService.SaveMessageAsync(dto);
              return Ok(new { success = true, message = "تم إرسال الرسالة بنجاح" });
          }
      }
  }
  ```

- [ ] **Step 3: Verify Build**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 4: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/OrdersController.cs FTD.Api/Controllers/ContactController.cs
  git commit -m "feat: implement public orders checkout and contact messages api endpoints"
  ```

---

### Task 6: Implement Secured Admin Dashboard and Orders Management API
**Files:**
- Create: `FTD.Api/Controllers/AdminController.cs`

**Interfaces:**
- Consumes: `IDashboardService`, `IOrderService`
- Produces: API endpoints `GET /api/admin/dashboard`, `GET /api/admin/orders`, `PUT /api/admin/orders/{id}/status`. All secured by JWT Roles="Admin".

- [ ] **Step 1: Create AdminController**
  Create `FTD.Api/Controllers/AdminController.cs`:
  ```csharp
  using FTD.Application.Interfaces;
  using Microsoft.AspNetCore.Authentication.JwtBearer;
  using Microsoft.AspNetCore.Authorization;
  using Microsoft.AspNetCore.Mvc;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
      public class AdminController : ControllerBase
      {
          private readonly IDashboardService _dashboardService;
          private readonly IOrderService _orderService;

          public AdminController(IDashboardService dashboardService, IOrderService orderService)
          {
              _dashboardService = dashboardService;
              _orderService = orderService;
          }

          [HttpGet("dashboard")]
          public async Task<IActionResult> GetDashboardData()
          {
              var data = await _dashboardService.GetDashboardDataAsync();
              return Ok(data);
          }

          [HttpGet("orders")]
          public async Task<IActionResult> GetOrders([FromQuery] string? status)
          {
              var orders = await _orderService.GetOrdersForAdminAsync(status);
              return Ok(orders);
          }

          [HttpPut("orders/{id:int}/status")]
          public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateStatusRequest request)
          {
              if (request.StatusId <= 0)
                  return BadRequest("رمز الحالة غير صالح");

              await _orderService.UpdateStatusAsync(id, request.StatusId, request.Notes);
              return Ok(new { success = true, message = "تم تحديث حالة الطلب بنجاح" });
          }

          public class UpdateStatusRequest
          {
              public int StatusId { get; set; }
              public string? Notes { get; set; }
          }
      }
  }
  ```

- [ ] **Step 2: Verify Final Build**
  Run: `dotnet build FTD.Web/FTD.Web.sln`
  Expected: Whole solution builds successfully with 0 errors.

- [ ] **Step 3: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/AdminController.cs
  git commit -m "feat: implement secured AdminController API endpoints for orders and dashboard"
  ```
