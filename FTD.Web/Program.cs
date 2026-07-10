using FTD.Application.Interfaces;
using FTD.Application.Services;
using FTD.Infrastructure.Data;
using FTD.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// ── MVC (no Razor Pages - pure MVC only) ─────────────────────────────────────
builder.Services.AddControllersWithViews();

// ── FORM OPTIONS — increase limits to avoid HTTP 400 on large multipart forms ──
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.ValueCountLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

// ── KESTREL — increase max request body size ──────────────────────────────────
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

// ── DATABASE ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

// ── IDENTITY (without UI scaffolding) ────────────────────────────────────────
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

// Redirect unauthorized to our custom admin login page
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Admin/AdminAccount/Login";
    options.AccessDeniedPath = "/Admin/AdminAccount/Login";
});

// ── SESSION ───────────────────────────────────────────────────────────────────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".FTD.Session";
});
builder.Services.AddHttpContextAccessor();

// ── APPLICATION SERVICES ──────────────────────────────────────────────────────
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartStorage, FTD.Web.Infrastructure.SessionCartStorage>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ── EMAIL SERVICE ─────────────────────────────────────────────────────────────
var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>()
    ?? new EmailSettings();
builder.Services.AddSingleton(emailSettings);
builder.Services.AddScoped<IEmailService, EmailService>();

// ── HEALTH CHECKS ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "database");

var app = builder.Build();

// ── MIDDLEWARE PIPELINE ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// ── ROUTES ────────────────────────────────────────────────────────────────────

// /Admin  → Dashboard/Index
app.MapControllerRoute(
    name: "admin-root",
    pattern: "Admin",
    defaults: new { controller = "Dashboard", action = "Index" });

// /Admin/{controller}/{action}/{id?}
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller}/{action=Index}/{id?}");

// /Products/Filter  → AJAX filter endpoint (must be before slug route)
app.MapControllerRoute(
    name: "products-filter",
    pattern: "Products/Filter",
    defaults: new { controller = "Products", action = "Filter" });

// /Products/Search  → AJAX search endpoint
app.MapControllerRoute(
    name: "products-search",
    pattern: "Products/Search",
    defaults: new { controller = "Products", action = "Search" });

// /products/{slug}  → Products/Detail
app.MapControllerRoute(
    name: "product-detail",
    pattern: "products/{slug}",
    defaults: new { controller = "Products", action = "Detail" });

// /page/{slug}  → Page/Show
app.MapControllerRoute(
    name: "content-page",
    pattern: "page/{slug}",
    defaults: new { controller = "Page", action = "Show" });

// /brand/{slug}  → /brand/doogee, /brand/jisulife
app.MapControllerRoute(
    name: "brand-page",
    pattern: "brand/{brandSlug}",
    defaults: new { controller = "Products", action = "BrandPage" });

// Default
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ── SEED: Admin role + user ───────────────────────────────────────────────────
await SeedAsync(app);

app.MapHealthChecks("/health");

app.Run();

static async Task SeedAsync(WebApplication app)
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    try { db.Database.Migrate(); } catch { }

    if (!await roleMgr.RoleExistsAsync("Admin"))
        await roleMgr.CreateAsync(new IdentityRole("Admin"));

    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminEmail = config["SeedAdmin:Email"] ?? "admin@ftdtechzone.com";
    var adminPassword = config["SeedAdmin:Password"] ?? "Admin@123456";

    if (await userMgr.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userMgr.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
            await userMgr.AddToRoleAsync(admin, "Admin");
    }
}
