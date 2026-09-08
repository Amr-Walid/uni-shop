using System;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using Asp.Versioning;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.Interfaces;
using FTD.Application.Services;
using FTD.Domain.Entities;
using FTD.Infrastructure.Data;
using FTD.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Optional, gitignored developer-machine overrides — see LocalConfiguration for
// why it is inserted after the tracked JSON files but before environment
// variables. Absent in every deployed environment.
FTD.Hosting.LocalConfiguration.Insert(builder.Configuration, builder.Environment);

// ── CONTROLLERS ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// Model-validation failures are returned in the SAME ProblemDetails shape as
// every other error, with the stable VALIDATION_FAILED code, so a client needs
// only one error parser.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "بيانات غير صالحة",
            Detail = "تحقق من الحقول المرسلة",
            Type = "https://api.unishop.eg/errors/validation_failed"
        };
        problem.Extensions["errorCode"] = ApiErrorCodes.ValidationFailed;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// ── API VERSIONING ────────────────────────────────────────────────────────────
// Mobile apps in the stores keep calling the version they shipped against for
// months. Versioned URL segments mean a breaking change can ship as v2 while
// v1 keeps serving already-installed builds.
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// ── DATABASE ──────────────────────────────────────────────────────────────────
// Provider is configuration-driven so the API can run on a machine with no SQL
// Server — a demo box, a CI runner, or a reviewer's laptop. It DEFAULTS TO SQL
// Server, so production (and a fresh clone) needs no configuration at all.
//
// Sqlite and InMemory are DEVELOPMENT-ONLY and refuse to start otherwise:
// Sqlite writes to an unmanaged local file, and InMemory has no persistence, no
// transactions and no relational constraints, so serving real traffic from
// either would silently lose orders. Selection logic is shared with FTD.Web via
// DatabaseProviderSetup so the two hosts cannot disagree.
var databaseProvider = DatabaseProviderSetup.AddAppDbContext(
    builder.Services, builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// ── IDENTITY ──────────────────────────────────────────────────────────────────
// Typed on AppUser so the API and the MVC site share one user aggregate.
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedAccount = false;

    // Mirrors FTD.Web exactly. Previously the API did not enable lockout at
    // all, so the same account could be brute-forced through the API even
    // though the website was protected.
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT ───────────────────────────────────────────────────────────────────────
// The secret MUST come from the environment or user-secrets. Startup fails fast
// if it is missing or still the committed placeholder, because booting with a
// known key would let anyone forge an admin token.
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.Secret) ||
    jwtSettings.Secret.Contains("REPLACE_THIS", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret is not configured. Provide at least 32 bytes of entropy via " +
        "the JwtSettings__Secret environment variable or 'dotnet user-secrets set'. " +
        "Never store the production secret in appsettings.json.");
}

builder.Services.AddSingleton(jwtSettings);
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// UTF8 (not ASCII): ASCII silently maps every non-ASCII byte to '?', which
// collapses the effective key space of an otherwise strong secret.
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Distinguish "expired" from "invalid" via a stable error code: the client
    // should silently refresh on the former and force a re-login on the latter.
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
                context.Response.Headers.Append("X-Token-Expired", "true");
            return Task.CompletedTask;
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();

            var expired = context.Response.Headers.ContainsKey("X-Token-Expired");
            var code = expired ? ApiErrorCodes.TokenExpired : ApiErrorCodes.Unauthorized;
            var detail = expired
                ? "انتهت صلاحية رمز الدخول. استخدم رمز التجديد للحصول على رمز جديد."
                : "مطلوب تسجيل الدخول للوصول إلى هذه الخدمة.";

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "غير مصادق",
                Detail = detail,
                Type = $"https://api.unishop.eg/errors/{code.ToLowerInvariant()}"
            };
            problem.Extensions["errorCode"] = code;
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        },
        OnForbidden = async context =>
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "غير مصرح",
                Detail = "حسابك لا يملك صلاحية الوصول إلى هذه الخدمة.",
                Type = $"https://api.unishop.eg/errors/{ApiErrorCodes.Forbidden.ToLowerInvariant()}"
            };
            problem.Extensions["errorCode"] = ApiErrorCodes.Forbidden;
            problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    };
});

// ── CORS ──────────────────────────────────────────────────────────────────────
// Native mobile clients do not send an Origin header, so they are unaffected by
// CORS. This exists for browser-based callers only, and stays deny-by-default in
// production: an API that mints JWTs must never be callable from any origin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("X-Token-Expired")
                  .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .WithExposedHeaders("X-Token-Expired");
        }
    });
});

// ── RATE LIMITING ─────────────────────────────────────────────────────────────
// Previously only the login endpoint was limited. Registration, password reset,
// checkout, search and the contact form are all abusable, so each gets a policy
// sized to its legitimate use.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Partition by IP so one abusive client cannot exhaust everyone's budget.
    static string PartitionKey(HttpContext ctx)
        => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    void AddFixedWindow(string name, int permitLimit, TimeSpan window)
    {
        options.AddPolicy(name, ctx => RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(ctx),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0
            }));
    }

    AddFixedWindow("login-policy", 5, TimeSpan.FromSeconds(30));
    AddFixedWindow("register-policy", 3, TimeSpan.FromMinutes(10));
    AddFixedWindow("forgot-password-policy", 3, TimeSpan.FromHours(1));
    AddFixedWindow("checkout-policy", 10, TimeSpan.FromMinutes(1));
    AddFixedWindow("contact-policy", 3, TimeSpan.FromMinutes(10));
    AddFixedWindow("search-policy", 30, TimeSpan.FromMinutes(1));

    // Blanket read budget — generous enough for normal browsing, low enough to
    // blunt a scraper.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            PartitionKey(ctx),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 300,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Rejections use the same ProblemDetails shape, and include Retry-After so
    // the client can back off correctly instead of hammering.
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/problem+json";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "طلبات كثيرة جداً",
            Detail = "لقد أرسلت طلبات كثيرة في وقت قصير. انتظر قليلاً ثم حاول مرة أخرى.",
            Type = $"https://api.unishop.eg/errors/{ApiErrorCodes.RateLimited.ToLowerInvariant()}"
        };
        problem.Extensions["errorCode"] = ApiErrorCodes.RateLimited;
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        await context.HttpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
    };
});

// ── APPLICATION SERVICES ──────────────────────────────────────────────────────
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Mobile/API-specific services
builder.Services.AddScoped<ICatalogQueryService, CatalogQueryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IWishlistService, WishlistService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<IDeviceTokenService, DeviceTokenService>();

// ── EMAIL ─────────────────────────────────────────────────────────────────────
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<IEmailService, EmailService>();

// ── CACHING & COMPRESSION ─────────────────────────────────────────────────────
// The catalog is near-static reference data read on every app launch; caching it
// removes most database traffic, and compression matters a great deal on a
// metered mobile connection.
builder.Services.AddMemoryCache();
builder.Services.AddResponseCompression(options => options.EnableForHttps = true);

// ── ERROR HANDLING ────────────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── HEALTH CHECKS ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "database");

// ── SWAGGER / OPENAPI ─────────────────────────────────────────────────────────
// Generated from the code, so the contract cannot drift from the implementation
// the way the hand-written docs did.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Uni-Shop API",
        Version = "v1",
        Description =
            "REST API for the Uni-Shop storefront and mobile apps.\n\n" +
            "**Language:** send `Accept-Language: ar` or `en`; responses contain a single " +
            "localized field per text value.\n\n" +
            "**Errors:** every failure is an RFC 7807 `application/problem+json` document with a " +
            "stable `errorCode` — branch on that, never on the message text.\n\n" +
            "**Auth:** `Bearer` access tokens live 15 minutes; use `/api/v1/auth/refresh` to rotate."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the access token only — Swagger adds the \"Bearer \" prefix."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Surface the XML doc comments written on the controllers.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (System.IO.File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────────────────────
// Must run first so it can catch exceptions thrown by everything downstream.
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseResponseCompression();

// Swagger stays enabled outside Development on purpose: it is the contract the
// mobile team codes against, and there is no secret in it. Restrict it at the
// edge if that changes.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Uni-Shop API v1");
    options.DocumentTitle = "Uni-Shop API";
    options.DisplayRequestDuration();
});

app.UseCors("AppCors");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ── ROOT & HEALTH ─────────────────────────────────────────────────────────────
app.MapGet("/", () => Results.Ok(new
{
    name = "Uni-Shop API",
    status = "healthy",
    version = "v1",
    documentation = "/swagger",
    health = "/health"
}))
.ExcludeFromDescription();

app.MapControllers();
app.MapHealthChecks("/health");

// ── LOCAL-PROVIDER BOOTSTRAP ──────────────────────────────────────────────────
// MUST run before any other database access. Sqlite and InMemory materialise
// the model's HasData seeds (categories, brands, products, order statuses,
// settings) only on EnsureCreated — and the FIRST write creates the store
// implicitly without them. Role seeding is such a write, so running this
// afterwards left the catalogue permanently empty.
//
// On SQL Server this is skipped entirely: FTD.Web owns migrations there, and
// EnsureCreated would bypass the migration history table.
if (DatabaseProviderSetup.IsSqlite(databaseProvider)
    || DatabaseProviderSetup.IsInMemory(databaseProvider))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// ── ROLE SEEDING ──────────────────────────────────────────────────────────────
// Ensures Admin/Customer exist even when the API is the first host to boot
// against a fresh database. Deliberately does NOT run migrations: FTD.Web owns
// schema migration, and two hosts racing to migrate is a recipe for deadlocks.
await SeedRolesAsync(app);

app.Run();

static async Task SeedRolesAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in new[] { "Admin", "Customer" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    catch (Exception ex)
    {
        // Fail open, but loudly: the API can still serve the public catalog if
        // the database is briefly unreachable at boot.
        logger.LogError(ex, "Role seeding failed at startup — registration may not assign roles correctly.");
    }
}
