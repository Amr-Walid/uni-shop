# 🛒 يوني شوب (Uni-Shop) — الدليل الشامل الكامل للمشروع
# The Complete Master Guide — من الصفر إلى أقوى متجر إيكومرس ممكن

> **آخر تحديث:** 2026-07-13
> **الإصدار:** 3.0 (بعد التدقيق الشامل — 26 إصلاحاً)
> **الحالة:** `dotnet build` = 0 أخطاء / 0 تحذيرات على FTD.Web و FTD.Api
> **المستودع:** https://github.com/Amr-Walid/uni-shop

---

## 📖 فهرس المحتويات

| القسم | العنوان |
|------|---------|
| 1 | نظرة عامة وتعريف المشروع |
| 2 | التقنيات المستخدمة (Tech Stack) |
| 3 | المعمارية — Clean Architecture بالتفصيل |
| 4 | هيكل المجلدات والملفات الكامل |
| 5 | طبقة الدومين (FTD.Domain) — كل كيان بالتفصيل |
| 6 | طبقة التطبيق (FTD.Application) — DTOs والواجهات والخدمات |
| 7 | طبقة البنية التحتية (FTD.Infrastructure) — قاعدة البيانات والهجرات |
| 8 | طبقة الويب (FTD.Web) — المتحكمات والعروض |
| 9 | خدمة REST API المستقلة (FTD.Api) |
| 10 | الواجهة الأمامية (Frontend) — CSS/JS ونظام اللغتين |
| 11 | لوحة التحكم (Admin Panel) — كل شاشة بالتفصيل |
| 12 | منشئ الصفحات المرئي (Visual Page Builder) |
| 13 | نظام المحتوى الديناميكي (Content Blocks & Settings) |
| 14 | سلة التسوق ودورة حياة الطلب |
| 15 | نظام المواصفات والفلترة (Attributes & Faceted Search) |
| 16 | الأمان (Security) — الطبقات الدفاعية كاملة |
| 17 | الأداء (Performance) — القرارات والتحسينات |
| 18 | مخطط قاعدة البيانات الكامل (Database Schema) |
| 19 | التشغيل المحلي والنشر (Run & Deploy) |
| 20 | سجل الجودة — التدقيقات والإصلاحات |
| 21 | 🔍 تحليل الفجوات — ما هو ناقص بالتفصيل |
| 22 | 🚀 خارطة الطريق المستقبلية — المرحلة 1 (الأساسيات التجارية) |
| 23 | 🚀 خارطة الطريق — المرحلة 2 (تجربة العميل المتقدمة) |
| 24 | 🚀 خارطة الطريق — المرحلة 3 (التسويق والنمو) |
| 25 | 🚀 خارطة الطريق — المرحلة 4 (التوسع والذكاء الاصطناعي) |
| 26 | 💎 التحكم الكامل من لوحة الإدارة — الرؤية النهائية |
| 27 | 📐 معايير التطوير الإلزامية للمساهمين |
| 28 | الملاحق — مرجع سريع |

---
---

# 1. نظرة عامة وتعريف المشروع

## 1.1 ما هو يوني شوب؟

**يوني شوب (Uni-Shop)** — المعروف سابقاً باسم **FTD TechZone / الفجر للتجارة** — هو متجر إلكتروني متكامل مبني بتقنية **ASP.NET Core 9 MVC** متخصص في بيع أجهزة التكنولوجيا الحديثة في السوق المصري:

* 📱 **تابلتات متينة** (Rugged Tablets) — براند DOOGEE
* 🌀 **مراوح محمولة** (Portable Fans) — براند JisuLife
* 📷 **كاميرات ويب 4K** (Webcams) — براند Dreame

## 1.2 الخصائص الجوهرية

| الخاصية | الوصف |
|---------|-------|
| **ثنائي اللغة بالكامل** | عربي (RTL) / إنجليزي (LTR) مع تبديل لحظي بدون إعادة تحميل، وكوكي `ftd_lang` يضمن العرض الصحيح من أول بايت (SSR) |
| **لوحة تحكم شاملة** | إدارة كاملة للمنتجات والتصنيفات والبراندات والمواصفات والطلبات والمحتوى والصفحات والقوائم والإعدادات والرسائل |
| **صفحة رئيسية ديناميكية 100%** | كل نص وصورة وقسم وترتيب يُدار من الداشبورد (64 بلوك محتوى + 23 إعداداً) |
| **منشئ صفحات مرئي** | Drag & Drop مع 7 أنواع أقسام ومحرر Quill.js |
| **سلة تسوق بدون تسجيل** | Session-based، الدفع عند الاستلام (COD) |
| **API مستقل** | خدمة REST منفصلة بتوثيق JWT للتكاملات المستقبلية (موبايل / SPA) |
| **بحث وفلترة متقدمة** | فلترة بالمواصفات (Faceted) + بحث لحظي AJAX |

## 1.3 أرقام المشروع الحالية

```text
إجمالي أسطر C#:            ~35,500 سطر (بما فيها الهجرات المولدة)
ملفات C#:                   ~100 ملف
ملفات Razor Views:           49 ملف .cshtml
CSS:                         5,041 سطر (site.css 4455 + admin.css 586)
JavaScript:                  710 أسطر (site.js + admin.js + page-builder.js)
كيانات الدومين:              16 كياناً
خدمات التطبيق:               6 خدمات + خدمة بريد
هجرات EF Core:               12 هجرة
جداول قاعدة البيانات:        16 جدولاً + 7 جداول ASP.NET Identity
بلوكات المحتوى المزروعة:     64 بلوكاً
إعدادات النظام المزروعة:     23 إعداداً
حالات الطلب:                 7 حالات
```

## 1.4 فلسفة التصميم

المشروع مبني على 5 مبادئ صارمة:

1. **Pure MVC** — لا Razor Pages، لا Blazor. متحكمات وعروض تقليدية واضحة.
2. **Clean Architecture** — اتجاه الاعتماديات دائماً نحو الداخل: `Web/Api → Application → Domain`، و`Infrastructure` تنفّذ واجهات `Application`.
3. **Database-Driven Content** — لا نص ثابت (hardcoded) في الواجهة؛ كل المحتوى قابل للتعديل من الداشبورد مع Fallbacks آمنة.
4. **Bilingual by Design** — كل كيان محتوى يحمل `NameAr/NameEn` أو `BodyAr/BodyEn` من اليوم الأول، والواجهة تستخدم `data-ar`/`data-en`.
5. **Zero-Warning Policy** — البناء يجب أن يمر دائماً بـ 0 أخطاء و0 تحذيرات.

---
---

# 2. التقنيات المستخدمة (Tech Stack)

## 2.1 الخادم (Backend)

| التقنية | الإصدار | الاستخدام |
|---------|---------|-----------|
| **.NET** | 9.0 (`net9.0`) | إطار العمل الأساسي |
| **ASP.NET Core MVC** | 9.0 | طبقة العرض للموقع + لوحة التحكم |
| **ASP.NET Core Web API** | 9.0 | خدمة REST المستقلة (FTD.Api) |
| **Entity Framework Core** | 9.0 | ORM — Code-First مع Migrations |
| **SQL Server** | 2019+ / LocalDB | قاعدة البيانات |
| **ASP.NET Core Identity** | 9.0 | مصادقة الأدمن (Cookies) ومستخدمي API (JWT) |
| **JWT Bearer** | 9.0 | توثيق الـ API |

## 2.2 الواجهة (Frontend)

| التقنية | الاستخدام |
|---------|-----------|
| **Razor Views (.cshtml)** | القوالب — SSR كامل |
| **Vanilla JavaScript** | لا أطر ثقيلة — سرعة قصوى (site.js 291 سطراً فقط) |
| **CSS مخصص بالكامل** | نظام تصميم Design Tokens خاص — لا Bootstrap CSS |
| **Bootstrap Icons 1.11** | مكتبة الأيقونات الوحيدة (CDN) |
| **Google Fonts** | Tajawal (عربي) + Plus Jakarta Sans (إنجليزي) |
| **Quill.js** | محرر النصوص الغنية في منشئ الصفحات (Admin فقط) |

## 2.3 حزم NuGet الأساسية

```xml
<!-- FTD.Infrastructure -->
Microsoft.EntityFrameworkCore.SqlServer (9.x)
Microsoft.EntityFrameworkCore.Design (9.x)
Microsoft.AspNetCore.Identity.EntityFrameworkCore (9.x)

<!-- FTD.Web -->
Microsoft.AspNetCore.Identity.UI (بدون Scaffolding)

<!-- FTD.Api -->
Microsoft.AspNetCore.Authentication.JwtBearer (9.x)
Swashbuckle.AspNetCore (Swagger)
```

## 2.4 أدوات التطوير

| الأداة | الأمر |
|--------|-------|
| بناء الحل | `dotnet build FTD.Web/FTD.Web.csproj` |
| هجرة جديدة | `dotnet ef migrations add <Name> --project FTD.Infrastructure --startup-project FTD.Web` |
| تحديث القاعدة | تلقائي عند الإقلاع (`db.Database.Migrate()`) أو `dotnet ef database update` |
| تشغيل الكل | `run-all.bat` (الموقع :5000 + الـ API :5100) |

---
---

# 3. المعمارية — Clean Architecture بالتفصيل

## 3.1 المخطط العام

```text
┌─────────────────────────────────────────────────────────────────┐
│                        طبقة العرض (Presentation)                 │
│  ┌──────────────────────────┐   ┌──────────────────────────┐    │
│  │        FTD.Web           │   │        FTD.Api           │    │
│  │  MVC Controllers + Views │   │  REST Controllers + JWT  │    │
│  │  ViewComponents          │   │  Swagger                 │    │
│  │  wwwroot (css/js/images) │   │                          │    │
│  └────────────┬─────────────┘   └────────────┬─────────────┘    │
└───────────────┼──────────────────────────────┼──────────────────┘
                │            تعتمد على          │
                ▼                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                   طبقة التطبيق (FTD.Application)                 │
│   Interfaces (IProductService, IOrderService, IAppDbContext…)   │
│   Services (ProductService, OrderService, CartService…)         │
│   DTOs (ProductDto, SalesOrderDto, CartDto…)                    │
│   Mappers (MappingExtensions — Entity ⇄ DTO)                    │
│   Common (HtmlSanitizer)                                        │
└───────────────┬─────────────────────────────────────────────────┘
                │            تعتمد على
                ▼
┌─────────────────────────────────────────────────────────────────┐
│                     طبقة الدومين (FTD.Domain)                    │
│   16 كياناً نقياً (POCO Entities) — لا اعتماديات خارجية إطلاقاً   │
└─────────────────────────────────────────────────────────────────┘
                ▲
                │            تنفّذ واجهات Application
┌───────────────┴─────────────────────────────────────────────────┐
│                طبقة البنية التحتية (FTD.Infrastructure)          │
│   AppDbContext : IdentityDbContext, IAppDbContext               │
│   Migrations (12 هجرة) + Seed Data                              │
│   EmailService : IEmailService (SMTP)                           │
└─────────────────────────────────────────────────────────────────┘
```

## 3.2 قاعدة الاعتماديات الذهبية

```text
FTD.Domain          ← لا يعتمد على أي شيء
FTD.Application     ← يعتمد على Domain فقط
FTD.Infrastructure  ← يعتمد على Application + Domain
FTD.Web             ← يعتمد على Application + Infrastructure (للتسجيل في DI فقط)
FTD.Api             ← يعتمد على Application + Infrastructure (للتسجيل في DI فقط)
```

**النتيجة العملية:** يمكن استبدال SQL Server بـ PostgreSQL، أو استبدال واجهة MVC بتطبيق موبايل، دون لمس منطق الأعمال إطلاقاً.

## 3.3 نمط تجريد قاعدة البيانات — `IAppDbContext`

بدلاً من حقن `AppDbContext` الخرساني في الخدمات، تُحقن الواجهة `IAppDbContext` المعرفة في طبقة Application:

```csharp
public interface IAppDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }
    DbSet<AttributeValue> AttributeValues { get; }
    DbSet<ProductAttributeValue> ProductAttributeValues { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderDetail> SalesOrderDetails { get; }
    DbSet<OrderStatus> OrderStatuses { get; }
    DbSet<ContentBlock> ContentBlocks { get; }
    DbSet<ContentPage> ContentPages { get; }
    DbSet<PageSection> PageSections { get; }
    DbSet<NavigationItem> NavigationItems { get; }
    DbSet<SiteSetting> SiteSettings { get; }
    DbSet<ContactInfo> ContactInfos { get; }
    DbSet<ContactMessage> ContactMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

والتسجيل في `Program.cs`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IAppDbContext>(p => p.GetRequiredService<AppDbContext>());
```

**لماذا؟** طبقة Application لا تعرف EF Core SqlServer ولا Identity — تعرف فقط عقداً مجرداً، ما يسهل الاختبار (Mocking) والاستبدال.

## 3.4 نمط الذرّية بدون تسريب المعاملات

قرار معماري مهم اتُّخذ في التدقيق الأخير: بدلاً من إضافة `BeginTransactionAsync` إلى `IAppDbContext` (ما كان سيسرّب مفهوم `IDbContextTransaction` الخاص بـ EF إلى طبقة Application)، تُبنى الكيانات المركبة كـ **Object Graph واحد** وتُحفظ بـ `SaveChangesAsync()` واحدة:

```csharp
// CreateProductAsync — المنتج + صوره + مواصفاته في معاملة ضمنية واحدة
var product = new Product { /* ... */ };
foreach (var img in additionalImages)
    product.Images.Add(new ProductImage { ImagePath = img.ImagePath, ... });
foreach (var kv in attributes)
    product.AttributeValues.Add(new ProductAttributeValue { ... });
_db.Products.Add(product);
await _db.SaveChangesAsync();   // ← الكل أو لا شيء
```

EF Core يغلّف الحفظة الواحدة في معاملة قاعدة بيانات تلقائياً — لا نافذة "منتج نصف محفوظ" ولا تسريب تجريدي.

---
---

# 4. هيكل المجلدات والملفات الكامل

```text
uni-shop/
│
├── AUDIT_REPORT.md                        ← تقرير التدقيق الشامل (26 نتيجة + سجل الحلول)
├── PROJECT_COMPLETE_DOCUMENTATION.md      ← التوثيق التاريخي التراكمي
├── UNISHOP_MASTER_GUIDE.md                ← هذا الملف (الدليل الشامل)
├── run-all.bat                            ← تشغيل الموقع + الـ API معاً
│
├── FTD.Domain/                            ═══ طبقة الدومين ═══
│   ├── FTD.Domain.csproj
│   └── Entities/
│       ├── Product.cs                     ← المنتج (الكيان المركزي)
│       ├── Category.cs                    ← التصنيف
│       ├── Brand.cs                       ← البراند
│       ├── ProductImage.cs                ← صور المنتج الإضافية
│       ├── ProductAttribute.cs            ← تعريف المواصفة (رام/لون/شاشة…)
│       ├── AttributeValue.cs              ← قيمة مواصفة (8GB / أسود…)
│       ├── ProductAttributeValue.cs       ← ربط منتج ↔ قيمة مواصفة (Junction)
│       ├── SalesOrder.cs                  ← الطلب (رأس الفاتورة)
│       ├── SalesOrderDetail.cs            ← سطر الطلب (بند الفاتورة)
│       ├── OrderStatus.cs                 ← حالة الطلب (7 حالات)
│       ├── ContentBlock.cs                ← بلوك محتوى نصي (Key/Value ثنائي اللغة)
│       ├── ContentPage.cs                 ← صفحة حرة (سياسة الخصوصية…)
│       ├── PageSection.cs                 ← قسم داخل صفحة حرة (JSON محتوى)
│       ├── NavigationItem.cs              ← عنصر قائمة تنقل (هيدر/فوتر)
│       ├── SiteSetting.cs                 ← إعداد نظام (Key/Value)
│       ├── ContactInfo.cs                 ← بيانات التواصل (صف واحد)
│       └── ContactMessage.cs              ← رسالة من نموذج التواصل
│
├── FTD.Application/                       ═══ طبقة التطبيق ═══
│   ├── FTD.Application.csproj
│   ├── Common/
│   │   └── HtmlSanitizer.cs               ← تنظيف HTML من XSS (Whitelist)
│   ├── DTOs/                              ← 7 ملفات DTO مقسمة حسب المجال
│   │   ├── ProductDtos.cs                 ← ProductDto (+ Validation Attributes)
│   │   ├── CatalogDtos.cs                 ← BrandDto, CategoryDto
│   │   ├── OrderDtos.cs                   ← SalesOrderDto, CheckoutDto, OrderStatusDto
│   │   ├── CartDtos.cs                    ← CartDto, CartItemDto
│   │   ├── ContentDtos.cs                 ← ContentBlockDto, ContentPageDto, PageSectionDto
│   │   ├── DashboardDtos.cs               ← DashboardDto, OrderStatusCountDto
│   │   └── SiteDtos.cs                    ← SiteSettingDto, NavigationItemDto, ContactInfoDto…
│   ├── Interfaces/                        ← 9 واجهات
│   │   ├── IAppDbContext.cs               ← تجريد قاعدة البيانات
│   │   ├── IProductService.cs             ← 32 دالة كتالوج
│   │   ├── IOrderService.cs / ICartService.cs / ICartStorage.cs
│   │   ├── IContentService.cs / IDashboardService.cs
│   │   └── IMessageService.cs / IEmailService.cs
│   ├── Mappers/
│   │   └── MappingExtensions.cs           ← Entity → DTO يدوي (ToDto extensions)
│   └── Services/                          ← 6 خدمات (كل منطق الأعمال)
│       ├── ProductService.cs              ← 907 أسطر — الكتالوج كاملاً
│       ├── OrderService.cs                ← إنشاء الطلبات وإدارة الحالات
│       ├── CartService.cs                 ← منطق السلة (Session JSON)
│       ├── ContentService.cs              ← البلوكات/الصفحات/الإعدادات/القوائم
│       ├── DashboardService.cs            ← إحصائيات لوحة التحكم
│       └── MessageService.cs              ← رسائل التواصل + إشعار بريدي
│
├── FTD.Infrastructure/                    ═══ طبقة البنية التحتية ═══
│   ├── FTD.Infrastructure.csproj
│   ├── Data/
│   │   └── AppDbContext.cs                ← 440 سطراً: DbSets + Fluent Config + Seeds
│   ├── Migrations/                        ← 12 هجرة + Snapshot
│   └── Services/
│       └── EmailService.cs                ← SMTP (MailKit-style عبر SmtpClient)
│
├── FTD.Web/                               ═══ الموقع + لوحة التحكم ═══
│   ├── FTD.Web.csproj
│   ├── Program.cs                         ← DI + Middleware + Routes + Seed
│   ├── appsettings.json                   ← ConnectionString + EmailSettings + SeedAdmin
│   ├── Controllers/
│   │   ├── HomeController.cs              ← الرئيسية + نموذج التواصل
│   │   ├── ProductsController.cs          ← الكتالوج/الفلترة/البحث/التفاصيل/البراند
│   │   ├── CartOrderController.cs         ← Cart + Order + Page (3 متحكمات)
│   │   └── Admin/
│   │       ├── AdminControllers.cs        ← Dashboard/Products/Categories/Orders/Content/Settings/Account
│   │       ├── AdminBrandsController.cs
│   │       ├── AdminAttributesAndSectionsControllers.cs
│   │       ├── AdminNavigationController.cs
│   │       └── AdminMessagesController.cs
│   ├── Helpers/
│   │   ├── ImageUploadHelper.cs           ← رفع الصور الموحد (امتداد + 5MB + GUID)
│   │   └── SectionStyleHelper.cs          ← أنماط أقسام منشئ الصفحات
│   ├── Infrastructure/
│   │   └── SessionCartStorage.cs          ← ICartStorage عبر Session
│   ├── ViewComponents/
│   │   ├── NavbarViewComponent.cs         ← الهيدر (قوائم + تصنيفات ديناميكية)
│   │   ├── FooterViewComponent.cs         ← الفوتر (روابط + تواصل + براندات)
│   │   └── AdminMessageCountViewComponent.cs ← عداد الرسائل غير المقروءة
│   ├── ViewModels/
│   │   ├── ViewModels.cs                  ← 14 ViewModel
│   │   └── CartViewModelMapper.cs
│   ├── Views/                             ← 49 ملف .cshtml
│   │   ├── Home/Index.cshtml              ← الرئيسية (663 سطراً — أقسام ديناميكية)
│   │   ├── Products/  (Index, Detail, _ProductsGrid)
│   │   ├── Cart/Index.cshtml
│   │   ├── Order/  (Checkout, Confirmation)
│   │   ├── Page/Show.cshtml               ← عرض الصفحات الحرة
│   │   ├── Admin/                         ← 22 شاشة إدارة
│   │   └── Shared/
│   │       ├── _Layout.cshtml             ← قالب الموقع (+ Antiforgery عام)
│   │       ├── _AdminLayout.cshtml        ← قالب لوحة التحكم
│   │       ├── _ProductCard.cshtml        ← كارت المنتج الموحد
│   │       ├── _Section_*.cshtml          ← 7 أقسام منشئ الصفحات (عرض)
│   │       └── Components/                ← عروض الـ ViewComponents
│   └── wwwroot/
│       ├── css/  (site.css 4455 سطراً + admin.css)
│       ├── js/   (site.js + admin.js + page-builder.js)
│       └── images/  (products/ categories/ brands/ …)
│
└── FTD.Api/                               ═══ REST API مستقل ═══
    ├── FTD.Api.csproj
    ├── Program.cs                         ← JWT + Swagger + Rate Limiting
    ├── appsettings.json                   ← JWT Secret (placeholder — User Secrets)
    ├── Models/Requests/                   ← LoginRequest, CheckoutRequest, UpdateStatusRequest
    └── Controllers/
        ├── ProductsController.cs          ← GET كتالوج عام
        ├── OrdersController.cs            ← POST checkout
        ├── ContactController.cs           ← POST رسالة تواصل
        ├── AuthController.cs              ← POST login → JWT
        └── AdminController.cs             ← [Authorize] داشبورد/طلبات
```

---
---

# 5. طبقة الدومين (FTD.Domain) — كل كيان بالتفصيل

## 5.1 المنتج `Product` — الكيان المركزي

```csharp
public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }              // FK → Category (إلزامي)
    public int? BrandId { get; set; }                // FK → Brand (اختياري)

    [Required, MaxLength(200)] public string NameAr { get; set; } = "";
    [Required, MaxLength(200)] public string NameEn { get; set; } = "";
    [Required, MaxLength(100)] public string Slug { get; set; } = "";   // فهرس فريد

    public string? ShortDescAr / ShortDescEn;        // وصف مختصر (كروت)
    public string? DescAr / DescEn;                  // وصف كامل (صفحة التفاصيل)

    [Column(TypeName = "decimal(18,2)")] public decimal Price;
    [Column(TypeName = "decimal(18,2)")] public decimal? OldPrice;  // للخصومات

    public string? Badge;        // شارة: NEW / HOT / RUGGED / 4K …
    public string? ImagePath;    // الصورة الرئيسية
    public string? BrandName;    // اسم عرض قديم (fallback)
    public string? Emoji;        // بديل الصورة عند غيابها

    public bool IsActive = true;     // إخفاء من المتجر دون حذف
    public bool IsFeatured = false;  // يظهر في قسم "المميز" تلقائياً
    public int SortOrder = 0;
    public int Stock = 0;            // المخزون — يُخصم عند الطلب
    public DateTime CreatedAt = DateTime.UtcNow;

    public string? MetaTitle / MetaDesc;   // SEO لكل منتج
    public string? FeaturesJson;           // ميزات "ماذا في العلبة" (JSON ديناميكي)

    // Navigation
    public Category Category;
    public Brand? Brand;
    public ICollection<ProductAttributeValue> AttributeValues;  // المواصفات
    public ICollection<ProductImage> Images;                    // الصور الإضافية
    public ICollection<SalesOrderDetail> OrderDetails;          // تاريخ المبيعات
}
```

**نقاط تصميمية مهمة:**
- `Slug` عليه **فهرس فريد** في قاعدة البيانات → الخدمة تتحقق مسبقاً وترمي رسالة عربية ودية بدل SqlException.
- `FeaturesJson` يخزن مصفوفة `[{icon, ar, en}]` — ميزات مخصصة لكل منتج تُدار من الأدمن وتُعرض كمربعات في صفحة التفاصيل (تختفي تلقائياً إن كانت فارغة).
- `OrderDetails` علاقة **Restrict** — حذف منتج له مبيعات = تعطيل (Soft-Delete) وليس مسح التاريخ المالي.

## 5.2 التصنيف `Category` والبراند `Brand`

```csharp
public class Category
{
    public int Id;
    [Required, MaxLength(150)] public string NameAr / NameEn;
    [Required, MaxLength(100)] public string Slug;      // فهرس فريد
    public string? ImagePath;      // صورة بلاطة الرئيسية
    public string? Emoji;          // بديل الصورة
    public string? Description;
    public int SortOrder;
    public bool IsActive;
    public bool ShowOnHomepage = true;   // ظهور مستقل في بلاطات الرئيسية
    public ICollection<Product> Products;
    public ICollection<ProductAttribute> Attributes;  // مواصفات خاصة بالتصنيف
}

public class Brand
{
    public int Id;
    [Required, MaxLength(100)] public string NameAr / NameEn;
    [Required, MaxLength(100)] public string Slug;    // بدون فهرس فريد (ملاحظة تدقيق)
    public string? LogoPath;       // لوجو (هيدر/فوتر/كروت)
    public string? BannerPath;     // بانر صفحة البراند المخصصة
    public string? DescAr / DescEn;
    public bool IsActive;
    public int SortOrder;
    public ICollection<Product> Products;
}
```

**لكل براند صفحة مخصصة** على المسار `/brand/{slug}` تعرض البانر والوصف ومنتجات البراند مفلترة.

## 5.3 نظام المواصفات (3 كيانات)

```text
ProductAttribute (المواصفة)        AttributeValue (القيمة)
┌──────────────────────┐          ┌──────────────────────┐
│ Id                   │          │ Id                   │
│ CategoryId  ← FK     │ 1 ─── ∞  │ AttributeId ← FK     │
│ NameAr: "الرام"      │          │ ValueAr: "8 جيجا"    │
│ NameEn: "RAM"        │          │ ValueEn: "8GB"       │
│ SortOrder            │          └──────────┬───────────┘
└──────────────────────┘                     │
                                             │ ∞
                              ProductAttributeValue (الربط)
                              ┌──────────────────────────┐
                              │ ProductId / AttributeId  │
                              │ AttributeValueId         │
                              └──────────────────────────┘
```

- كل مواصفة **مرتبطة بتصنيف** — نموذج المنتج يعرض مواصفات تصنيفه فقط (AJAX عند تغيير التصنيف).
- `DeleteBehavior.Restrict` على الربط — الخدمة تفك الارتباطات أولاً ثم تحذف (إصلاح I-04).
- القيم الرقمية تُرتب **ترتيباً طبيعياً** (8GB < 12GB < 128GB) وليس أبجدياً.

## 5.4 الطلبات (3 كيانات)

```csharp
public class SalesOrder
{
    public int Id;
    [Required, MaxLength(50)] public string OrderNumber;   // FTD20260713xxxxxx###
    public int StatusId;                                   // FK → OrderStatus
    [Required, MaxLength(150)] public string CustomerName;
    [Required, MaxLength(20)]  public string CustomerPhone;
    [MaxLength(200)] public string? CustomerEmail;
    [MaxLength(300)] public string? Address;
    [MaxLength(100)] public string? City / Governorate;
    public string? Notes;
    public decimal SubTotal / ShippingFee / TotalAmount;   // decimal(18,2)
    public DateTime CreatedAt; public DateTime? UpdatedAt;
    public OrderStatus Status;
    public ICollection<SalesOrderDetail> Details;          // Cascade مع الطلب
}

public class SalesOrderDetail
{
    public int Id, OrderId, ProductId, Quantity;
    [MaxLength(200)] public string ProductName;   // ← Snapshot وقت الشراء
    public decimal UnitPrice / SubTotal;          // ← Snapshot وقت الشراء
    public SalesOrder Order; public Product Product;   // Product = Restrict
}
```

**مبدأ الـ Snapshot:** اسم المنتج وسعره يُنسخان في سطر الطلب وقت الشراء — تعديل المنتج لاحقاً لا يغيّر الفواتير القديمة أبداً.

**حالات الطلب السبع المزروعة:**

| Id | الحالة | اللون |
|----|--------|------|
| 1 | جديد | أزرق |
| 2 | مؤكد | سماوي |
| 3 | في انتظار الشحن | برتقالي |
| 4 | مع شركة الشحن | بنفسجي |
| 5 | تم التسليم | أخضر |
| 6 | مرتجع | رمادي |
| 7 | ملغي | أحمر |

## 5.5 كيانات المحتوى (5 كيانات)

| الكيان | الغرض | ملاحظات |
|--------|-------|---------|
| `ContentBlock` | نص ثنائي اللغة بمفتاح (`hero.title.line1`) | `TitleAr` يُعاد استخدامه كحقل أيقونة |
| `ContentPage` | صفحة حرة (`/page/{slug}`) | Title/Body ثنائي + SEO + IsPublished |
| `PageSection` | قسم داخل صفحة حرة | `Type` (hero/richtext/cards/faq/gallery/video/testimonials) + `ContentJson` + ترتيب + إظهار |
| `NavigationItem` | عنصر قائمة | `LinkType` (Static/Page/External) + `Location` (Header/Footer/Both/Hidden) + دعم Parent للتداخل |
| `SiteSetting` | إعداد Key/Value | `Type` (text/bool/number) للعرض الصحيح في الأدمن |

## 5.6 كيانات التواصل

- **`ContactInfo`** (صف واحد): هاتفان + بريد + عنوان ثنائي + مدينة + 4 روابط سوشيال (فيسبوك/إنستجرام/واتساب/تيكتوك) + ساعات عمل ثنائية + رابط خريطة مضمنة — **ولكل حقل مفتاح إظهار/إخفاء مستقل** (`ShowPhone`, `ShowEmail`, `ShowMap`…).
- **`ContactMessage`**: Name(100) + Email(100) + Phone(20) + Message + IsRead + CreatedAt — مع إشعار بريدي غير حاجب في الخلفية.

---
---

# 6. طبقة التطبيق (FTD.Application) — DTOs والواجهات والخدمات

طبقة التطبيق هي «عقل» المشروع: كل منطق العمل (Business Logic) يعيش هنا، معزولاً تماماً عن تفاصيل الويب وقاعدة البيانات. المتحكمات (Controllers) لا تعرف EF Core، والخدمات لا تعرف HTTP — كل طرف يتكلم عبر عقود (Interfaces) وكائنات نقل بيانات (DTOs).

## 6.1 ملفات الـ DTOs السبعة

| الملف | المحتوى | الغرض |
|---|---|---|
| `ProductDtos.cs` | `ProductDto`, `ProductListItemDto`, `ProductCreateDto`, `ProductUpdateDto`, `ProductImageDto`, `ProductAttributeGroupDto` | عرض/إنشاء/تعديل المنتجات مع DataAnnotations كاملة |
| `CatalogDtos.cs` | `CategoryDto`, `BrandDto`, `CategoryCreateDto`, `BrandCreateDto` | الفئات والماركات مع حدود أطوال النصوص |
| `CartDtos.cs` | `CartDto`, `CartItemDto`, `CartSummaryDto` | تمثيل السلة داخل الجلسة (JSON) |
| `OrderDtos.cs` | `CheckoutDto`, `OrderDto`, `OrderDetailDto`, `OrderListItemDto`, `OrderStatusUpdateDto` | دورة حياة الطلب من الدفع للتتبع |
| `ContentDtos.cs` | `ContentBlockDto`, `SiteSettingDto`, `PageSectionDto`, `NavigationLinkDto` | نظام المحتوى الديناميكي |
| `DashboardDtos.cs` | `DashboardStatsDto`, `RecentOrderDto`, `TopProductDto` | إحصائيات لوحة التحكم |
| `MessageDtos.cs` | `ContactMessageDto`, `ContactInfoDto` | الرسائل ومعلومات التواصل |

### مبدأ تصميم الـ DTOs
- **لا كيان دومين يعبر حدود الطبقة أبداً** — المتحكمات والعروض تتعامل مع DTOs فقط.
- **التحقق مدمج**: كل DTO إدخال (`Create`/`Update`) يحمل `[Required]`, `[StringLength]`, `[Range]`, `[EmailAddress]` — تحقق الخادم يعمل حتى لو تعطّل JavaScript.
- **DTOs العرض خفيفة**: `ProductListItemDto` تحمل فقط ما تحتاجه بطاقة المنتج (اسم، سعر، صورة، slug) — لا تحميل زائد.

```csharp
// مثال: ProductCreateDto مع التحقق الكامل
public class ProductCreateDto
{
    [Required, StringLength(200)]
    public string NameAr { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string NameEn { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string Slug { get; set; } = string.Empty;

    [Range(0, 1_000_000)]
    public decimal Price { get; set; }

    [Range(0, 1_000_000)]
    public decimal? OldPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }
    public int? BrandId { get; set; }
    // ... الوصف ثنائي اللغة، SKU، الحالة، القيم الوصفية
}
```

## 6.2 الواجهات التسع (العقود)

| الواجهة | تنفَّذ في | الدور |
|---|---|---|
| `IAppDbContext` | `FTD.Infrastructure.AppDbContext` | تجريد قاعدة البيانات — `DbSet<T>` لكل كيان + `SaveChangesAsync` |
| `IProductService` | `ProductService` | 32 عملية على المنتجات والفئات والماركات |
| `ICartService` | `CartService` | إدارة السلة (إضافة/حذف/كمية/ملخص) |
| `IOrderService` | `OrderService` | إنشاء الطلبات وتتبعها وتغيير حالتها |
| `IContentService` | `ContentService` | البلوكات والإعدادات والأقسام والروابط |
| `IDashboardService` | `DashboardService` | إحصائيات الإدارة |
| `IMessageService` | `MessageService` | رسائل التواصل ومعلومات الاتصال |
| `ICartStorage` | `SessionCartStorage` (Web) | تجريد مخزن السلة — الجلسة حالياً، قابل للاستبدال بـ Redis/DB |
| `IEmailService` | `EmailService` (Infrastructure) | إرسال البريد (SMTP) بشكل غير حاجب |

**لماذا `ICartStorage` منفصلة؟** لأن الجلسة (Session) مفهوم ويب — لو خُزّنت السلة في `CartService` مباشرة عبر `IHttpContextAccessor` لتلوثت طبقة التطبيق بتفاصيل HTTP. بدلاً من ذلك، `CartService` تطلب "مخزناً" مجرداً، وطبقة الويب تحقن التنفيذ الجلسوي. غداً يمكن استبداله بمخزن Redis لسلات دائمة بدون لمس منطق العمل.

## 6.3 ProductService — الخدمة الأكبر (907 سطر، 32 طريقة عامة)

### أ) طرق العرض العام (Storefront)

| الطريقة | الوصف |
|---|---|
| `GetFeaturedAsync(count)` | المنتجات المميزة للصفحة الرئيسية — `AsNoTracking` + إسقاط مباشر |
| `GetLatestAsync(count)` | أحدث المنتجات حسب `CreatedAt` |
| `GetBySlugAsync(slug)` | تفاصيل منتج كاملة (صور + مواصفات + فئة + ماركة) |
| `GetByIdsOrderedAsync(ids)` | جلب منتجات بترتيب قائمة معرفات محددة (لأقسام الصفحة الرئيسية المخصصة) |
| `GetFilteredBySlugAsync(categorySlug, filters, page)` | الفلترة الوجهية (Faceted) داخل فئة |
| `GetAvailableFacetsAsync(categorySlug)` | حساب الفلاتر المتاحة وأعداد كل قيمة |
| `SearchAsync(term, page)` | بحث نصي في الاسمين والوصفين وSKU |
| `GetByBrandSlugAsync(slug, page)` | صفحة ماركة كاملة |
| `GetRelatedAsync(productId, count)` | منتجات ذات صلة (نفس الفئة، استبعاد الحالي) |

### ب) طرق الإدارة (Admin CRUD)

| الطريقة | ملاحظات جوهرية |
|---|---|
| `GetAllForAdminAsync(search, categoryId, page)` | ترقيم + بحث + فلترة للجدول الإداري |
| `GetByIdForEditAsync(id)` | تحميل كامل الرسم البياني للتعديل |
| `CreateAsync(dto)` | **فحص مسبق لتفرد الـ Slug** ثم حفظ الرسم كاملاً (منتج + صور + قيم مواصفات) في `SaveChangesAsync` **واحدة** — ذرّية بلا معاملات مسربة |
| `UpdateAsync(dto)` | مزامنة الصور والمواصفات (حذف المزال، إضافة الجديد، تحديث الموجود) في حفظة واحدة |
| `DeleteAsync(id)` | **آمن ضد FK**: يفحص وجود تفاصيل طلبات مرتبطة (بسبب `Restrict`) ويرفض الحذف برسالة واضحة بدلاً من انفجار SQL |
| `DuplicateProductAsync(id)` | استنساخ منتج كاملاً (صور + مواصفات) مع slug جديد فريد — يوفر وقت إدخال هائل |
| `ToggleActiveAsync(id)` / `ToggleFeaturedAsync(id)` | تبديلات سريعة من الجدول |
| `GetCategoryByIdAsync(id)` / `GetBrandByIdAsync(id)` | جلب صف واحد (استُبدلت بها طرق كانت تجلب قوائم كاملة — إصلاح تدقيق I-14) |

### ج) طرق الفئات والماركات والمواصفات

`GetAllCategoriesAsync`, `CreateCategoryAsync`, `UpdateCategoryAsync`, `DeleteCategoryAsync` (يرفض لو فيها منتجات)، ونظيراتها للماركات، بالإضافة إلى `BuildAttributeGroupsAsync(categoryId)` التي تبني شجرة «مجموعات المواصفات ← قيمها» لنموذج المنتج ديناميكياً حسب الفئة المختارة.

### د) القرارات الهندسية داخل ProductService

1. **إسقاط مبكر (Early Projection)**: كل استعلامات القوائم تُسقط إلى DTO داخل `Select` — EF يولّد `SELECT` بالأعمدة المطلوبة فقط.
2. **`AsNoTracking` افتراضي للقراءة**: لا Change Tracker overhead في 100% من مسارات العرض.
3. **عدّ بالإسقاط**: `CountAsync` منفصلة للترقيم بدل جلب القائمة كلها.
4. **لا Transactions يدوية**: الرسم البياني للكائنات (Object Graph) يُبنى في الذاكرة ثم `SaveChangesAsync` واحدة — EF يلفّها في معاملة ضمنياً.

## 6.4 OrderService — دورة الطلب

```csharp
// جوهر CreateOrderAsync (مبسّط):
// 1) جلب كل المنتجات المطلوبة دفعة واحدة (استعلام واحد بدل N)
var products = await _db.Products
    .Where(p => productIds.Contains(p.Id) && p.IsActive)
    .ToDictionaryAsync(p => p.Id);

// 2) بناء الطلب + التفاصيل بأسعار لحظية (Snapshot)
foreach (var item in cart.Items)
{
    if (!products.TryGetValue(item.ProductId, out var product)) continue;
    var qty = Math.Max(1, item.Quantity);           // حارس الكمية
    order.Details.Add(new SalesOrderDetail
    {
        ProductId = product.Id,
        ProductNameAr = product.NameAr,             // لقطة الاسم
        ProductNameEn = product.NameEn,
        UnitPrice = product.Price,                  // لقطة السعر
        Quantity = qty
    });
}

// 3) رقم طلب فريد: FTD + طابع زمني + عداد
order.OrderNumber = $"FTD{DateTime.UtcNow:yyyyMMddHHmmss}{counter:D3}";

// 4) حفظة واحدة ذرّية
await _db.SaveChangesAsync(ct);
```

باقي الطرق: `GetByNumberAsync` (تتبع عام بالرقم)، `GetAllForAdminAsync` (فلترة بالحالة + بحث + ترقيم)، `GetByIdForAdminAsync`، `UpdateStatusAsync` (مع تحقق `AnyAsync` أن الحالة الجديدة موجودة فعلاً — إصلاح I-09)، وكلها قراءاتها `AsNoTracking`.

## 6.5 CartService — سلة الجلسة

- السلة كائن `CartDto` يُسلسل JSON ويُخزن عبر `ICartStorage` (الجلسة).
- `AddAsync(productId, qty)`: يتحقق من وجود المنتج ونشاطه من قاعدة البيانات (لا يثق بالعميل)، `qty = Math.Max(1, qty)`.
- `UpdateQuantityAsync` / `RemoveAsync` / `ClearAsync`: عمليات مباشرة على الـ JSON.
- `GetSummaryAsync`: يعيد حساب الأسعار **من قاعدة البيانات** عند كل عرض — لو تغيّر سعر منتج بعد إضافته للسلة، السلة تعكس السعر الحالي (الحماية من التلاعب بالأسعار المخزنة عميلاً).
- رسوم الشحن: تُقرأ من إعدادات `shipping.fee` و`shipping.free.above` — قابلة للتغيير من لوحة التحكم فوراً.

## 6.6 ContentService — محرك المحتوى الديناميكي

الخدمة الأوسع استخداماً (كل صفحة تمر بها):

| مجموعة الطرق | الوظيفة |
|---|---|
| `GetBlocksAsync()` / `GetBlockAsync(key)` / `UpsertBlockAsync` | قاموس النصوص ثنائية اللغة (64 بلوك مزروع) |
| `GetSettingsAsync()` / `GetSettingAsync(key)` / `UpsertSettingAsync` | 23 إعداد سلوكي (ألوان، شحن، إظهار أقسام…) |
| `GetPageSectionsAsync(pageKey)` / `SaveSectionAsync` / `DeleteSectionAsync` / `ReorderAsync` | منشئ الصفحات المرئي |
| `GetNavigationAsync(location)` / CRUD للروابط | قوائم الهيدر/الفوتر الديناميكية |

- **13 استعلام قراءة كلها `AsNoTracking`** (إصلاح I-06) — مسارات الكتابة تبقى متعقَّبة.
- **تعقيم HTML**: أي محتوى HTML قادم من محرر Quill يمر عبر `HtmlSanitizer` بقائمة وسوم/خصائص بيضاء قبل التخزين — حماية XSS مخزَّن.

## 6.7 DashboardService & MessageService

- **DashboardService**: إحصائية واحدة مجمعة (`DashboardStatsDto`) — إجمالي الطلبات/الإيرادات/المنتجات/الرسائل غير المقروءة + آخر 10 طلبات + المنتجات الأكثر مبيعاً (تجميع على `SalesOrderDetails`). كل الاستعلامات `AsNoTracking` وبإسقاط.
- **MessageService**: حفظ رسائل التواصل، تعليم كمقروءة، حذف، وإدارة صف `ContactInfo` الوحيد بكل مفاتيح الإظهار.

## 6.8 MappingExtensions — الخرائط اليدوية

لا AutoMapper في المشروع — قرار واعٍ:
- **خرائط يدوية** بامتدادات `ToDto()` / `ToEntity()` — صريحة، قابلة للتنقيح، صفر انعكاس (Reflection) في وقت التشغيل.
- تعمل داخل `Select` في استعلامات EF فتترجم لـ SQL مباشرة.
- كلفة الصيانة مقبولة لحجم المشروع (16 كياناً) مقابل شفافية كاملة.

---
---

# 7. طبقة البنية التحتية (FTD.Infrastructure)

## 7.1 AppDbContext — القلب (440 سطر)

```csharp
public class AppDbContext : IdentityDbContext<IdentityUser>, IAppDbContext
{
    // 16 DbSet — واحد لكل كيان دومين
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    // ... إلخ
}
```

يرث `IdentityDbContext` (جداول المستخدمين والأدوار جاهزة) وينفّذ `IAppDbContext` (عقد طبقة التطبيق) — عصفوران بحجر.

### تكوينات `OnModelCreating` الجوهرية

| التكوين | السبب |
|---|---|
| `decimal` بدقة `(18,2)` لكل الأسعار | لا فقدان قروش، معيار مالي |
| فهرس فريد على `Product.Slug`, `Category.Slug`, `Brand.Slug` | روابط SEO مضمونة التفرد على مستوى القاعدة |
| فهرس فريد على `SalesOrder.OrderNumber` | رقم الطلب مفتاح تتبع عام |
| فهرس فريد مركّب على `ContentBlock.Key` و`SiteSetting.Key` | القواميس لا تقبل تكراراً |
| `SalesOrderDetail → Product`: **`DeleteBehavior.Restrict`** | حذف منتج لا يمسح تاريخ المبيعات أبداً (إصلاح I-01 + هجرة) |
| وصلات المواصفات (`ProductAttributeValue`): `Restrict` | حذف قيمة مواصفة لا يفكك منتجات بصمت |
| `Category → Products`, `Brand → Products`: `Restrict` | الحذف محكوم من الخدمة برسالة، لا تتالي مدمر |

### البذر (HasData Seeding) — المتجر يولد جاهزاً

| البذرة | العدد | أمثلة |
|---|---|---|
| `ContentBlock` | **64** | `hero.title.ar/en`, `about.body.ar/en`, `footer.tagline.ar/en`, `stats.customers`, `contact.heading.*` |
| `SiteSetting` | **23** | `homepage.show.hero`, `homepage.show.featured`, `homepage.sections.order`, `shipping.fee`, `shipping.free.above`, `site.primary.color` |
| `OrderStatus` | **7** | جديد ← مؤكد ← قيد التجهيز ← شُحن ← سُلّم / ملغي / مرتجع (ثنائية اللغة + لون Badge) |

النتيجة: `dotnet run` على قاعدة فارغة = متجر كامل النصوص والإعدادات بدون إدخال يدوي واحد.

## 7.2 سجل الهجرات — 12 هجرة

| # | الهجرة | ماذا فعلت |
|---|---|---|
| 1 | `InitialCreate` | الكيانات الأساسية + Identity |
| 2 | `AddContentBlocks` | نظام البلوكات + البذر الأولي |
| 3 | `AddSiteSettings` | جدول الإعدادات + مفاتيح البذر |
| 4 | `AddPageSections` | منشئ الصفحات |
| 5 | `AddNavigationLinks` | القوائم الديناميكية |
| 6 | `AddContactEntities` | معلومات التواصل + الرسائل |
| 7 | `AddAttributeSystem` | المواصفات (Attributes/Values/الوصلات) |
| 8 | `AddProductEnhancements` | SKU، OldPrice، صور متعددة |
| 9 | `AddOrderStatusSeed` | حالات الطلب السبع |
| 10 | `ExpandContentSeed` | توسيع البلوكات لـ 64 |
| 11 | `AddHomepageSettings` | مفاتيح إظهار/ترتيب أقسام الرئيسية |
| 12 | `HardenOrderDetailProductFk` | **Cascade → Restrict** على تفاصيل الطلب (تدقيق 2026-07-13) |

**قاعدة ذهبية مطبقة**: أي تغيير في `AppDbContext` = هجرة جديدة فوراً، ولا تعديل يدوي على هجرة سابقة مُطبّقة.

## 7.3 الترحيل الذاتي عند الإقلاع

```csharp
// Program.cs — المتجر يرقّي قاعدته بنفسه
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
try
{
    db.Database.Migrate();   // يطبق الهجرات الناقصة فقط
    await IdentitySeeder.SeedAdminAsync(scope.ServiceProvider);
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Database migration failed at startup");
    throw;   // فشل الهجرة = لا إقلاع بقاعدة نصف مرقّاة (إصلاح I-22)
}
```

## 7.4 EmailService

- SMTP عبر `System.Net.Mail` بإعدادات من `appsettings.json` (`Smtp:Host/Port/User/Pass/From`).
- **غير حاجب**: إشعار «رسالة تواصل جديدة» يُرسل في مهمة خلفية — فشل البريد لا يُفشل حفظ الرسالة أبداً.
- لو الإعدادات فارغة (بيئة تطوير) الخدمة تتجاوز الإرسال بصمت مع تسجيل Log.

## 7.5 IdentitySeeder

- ينشئ دور `Admin` إن لم يوجد.
- ينشئ مستخدم الأدمن الافتراضي من `appsettings` (`AdminSeed:Email/Password`) ويربطه بالدور.
- Idempotent بالكامل — تشغيله ألف مرة آمن.

---
---

# 8. طبقة الويب (FTD.Web) — المتحكمات والعروض

## 8.1 خريطة المتحكمات العامة (Storefront)

| المتحكم | الإجراءات | المسارات |
|---|---|---|
| `HomeController` | `Index`, `About`, `Contact` (GET/POST), `Privacy`, `SetLanguage` | `/`, `/about`, `/contact` |
| `ProductsController` | `Index`, `Filter`, `BrandPage`, `Detail`, `Search` | `/products`, `/category/{slug}`, `/brand/{slug}`, `/product/{slug}`, `/search` |
| `CartOrderController` | `Cart`, `AddToCart`, `UpdateQuantity`, `RemoveItem`, `ClearCart`, `Checkout` (GET/POST), `Confirmation`, `Track` | `/cart`, `/checkout`, `/order/track` |

### تفصيل الإجراءات الجوهرية

**`HomeController.Index`** — أثقل صفحة وأكثرها ديناميكية:
1. يجلب البلوكات والإعدادات دفعة واحدة (قاموسان في الذاكرة).
2. يقرأ `homepage.sections.order` ليعرف ترتيب الأقسام.
3. يفحص مفاتيح `homepage.show.*` العشرة (hero, featured, categories, latest, brands, stats, about, custom sections…).
4. يجلب فقط بيانات الأقسام الظاهرة — قسم مخفي = صفر استعلامات له.
5. أقسام منشئ الصفحات (`PageSection`) تُدمج في نفس الترتيب.

**`HomeController.Contact` (POST)**:
- `[ValidateAntiForgeryToken]` + تحقق `ModelState`.
- حدود صارمة: Name≤100, Email≤100 (بصيغة بريد), Phone≤20, Message≤4000 (إصلاح I-19).
- الحفظ أولاً، ثم إشعار بريدي خلفي غير حاجب.

**`ProductsController.Filter`** — قلب التسوق:
```
GET /category/{slug}?attrs=color:red|blue;size:xl&min=100&max=500&sort=price_asc&page=2
```
- يفكك سلسلة الفلاتر إلى قاموس `attributeId → [valueIds]`.
- يستدعي `GetFilteredBySlugAsync` + `GetAvailableFacetsAsync` بالتوازي.
- يعيد صفحة كاملة أو Partial (لطلبات AJAX) — فلترة بلا إعادة تحميل.

**`CartOrderController`** — كل POSTs الأربعة (`AddToCart/UpdateQuantity/RemoveItem/ClearCart`) محمية بـ `[ValidateAntiForgeryToken]` (إصلاح I-02)، والتوكن يُرسل من `site.js` تلقائياً عبر هيدر `RequestVerificationToken` المقروء من حقل مخفي عام في `_Layout`.

**`Checkout` (POST)**:
1. تحقق `ModelState` على `CheckoutViewModel` (أسماء≤150، هاتف≤20، عنوان≤300…).
2. سلة فارغة؟ → إعادة توجيه للسلة برسالة.
3. `CreateOrderAsync` → نجاح: مسح السلة + `Confirmation` برقم الطلب.
4. `Track`: إدخال رقم `FTD...` يعرض حالة الطلب وتفاصيله — بلا حساب مستخدم.

## 8.2 متحكمات الإدارة (Admin) — 9 متحكمات

كلها `[Authorize(Roles = "Admin")]` + `[Area("Admin")]`:

| المتحكم | يدير |
|---|---|
| `AdminDashboardController` | الإحصائيات والنظرة العامة |
| `AdminProductsController` | المنتجات (جدول/نموذج/نسخ/تبديلات) |
| `AdminCategoriesController` | الفئات |
| `AdminBrandsController` | الماركات |
| `AdminOrdersController` | الطلبات وحالاتها |
| `AdminContentController` | البلوكات والإعدادات ومنشئ الصفحات |
| `AdminAttributesAndSectionsController` | المواصفات + أقسام الرئيسية المخصصة |
| `AdminNavigationController` | قوائم الهيدر/الفوتر |
| `AdminMessagesController` | رسائل التواصل + معلومات الاتصال |

### أنماط موحدة عبر متحكمات الإدارة (بعد التدقيق)

```csharp
// النمط القياسي لكل POST إداري:
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(ProductCreateDto dto)
{
    if (!ModelState.IsValid)
        return await RedisplayFormAsync(dto);      // إعادة عرض بالقوائم المنسدلة

    try
    {
        await _productService.CreateAsync(dto);
        TempData["Success"] = "تم الحفظ بنجاح";
        return RedirectToAction(nameof(Index));
    }
    catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
    {
        // فقط أخطاء العمل المتوقعة — لا ابتلاع أعطال حقيقية (إصلاح I-11)
        ModelState.AddModelError("", ex.Message);
        return await RedisplayFormAsync(dto);
    }
}
```

- **رفع الصور موحّد** عبر `ImageUploadHelper.SaveAsync` (إصلاح I-15): امتدادات بيضاء `{.jpg,.jpeg,.png,.gif,.webp}`، حد 5MB، اسم GUID (لا حقن مسارات)، حفظ في `wwwroot/uploads/{folder}`.
- **تسجيل الدخول الإداري**: Identity Lockout (5 محاولات → قفل) + Rate Limiting نافذة ثابتة على مسار `/admin/login`.

## 8.3 العروض (Views) — الهيكل

```
Views/
├── Shared/
│   ├── _Layout.cshtml          ← الهيكل العام + توكن antiforgery عام + تبديل اللغة
│   ├── _AdminLayout.cshtml     ← هيكل لوحة التحكم (Sidebar + Topbar)
│   ├── _ProductCard.cshtml     ← بطاقة المنتج الموحدة (تستخدم في 6 صفحات)
│   └── _Pagination.cshtml      ← ترقيم موحد
├── Home/Index.cshtml           ← 663 سطر — الرئيسية الديناميكية بالكامل
├── Products/ (Index, Filter, Detail, Search, BrandPage)
├── CartOrder/ (Cart, Checkout, Confirmation, Track)
└── Admin/
    ├── Content/Blocks.cshtml   ← 740 سطر — محرر البلوكات والإعدادات
    ├── Products/Form.cshtml    ← 535 سطر — نموذج المنتج الديناميكي
    └── ... (شاشة لكل وظيفة)
```

### مساعدا اللغة `Get` / `GetOr`

```csharp
// داخل الـ Views — قراءة بلوك بالمفتاح مع سقوط آمن:
@Get("hero.title")                       // يعيد النص حسب لغة الكوكي
@GetOr("stats.customers", "1000+")       // قيمة افتراضية لو المفتاح غائب
```
- `Get` يقرأ من قاموس `ViewBag.Blocks` المحمّل مرة واحدة لكل طلب.
- إصلاح I-24: بطاقات الإحصائيات تستخدم `GetOr(...).TrimEnd('+')` كي لا تتراكم علامة `+` عند الإلحاق البرمجي.

## 8.4 ViewComponents وViewModels

- **`CartBadgeViewComponent`**: عدّاد السلة في الهيدر — يقرأ الجلسة فقط (صفر استعلام DB).
- **`ViewModels.cs`**: `HomeViewModel` (كل بيانات الرئيسية)، `ProductFilterViewModel`، `CheckoutViewModel` (بحدود StringLength كاملة بعد I-19)، `ContactViewModel`، `TrackOrderViewModel`.

## 8.5 Program.cs — التوصيل الكامل

```csharp
// حقن الخدمات — الاتجاه الوحيد المسموح:
builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IProductService, ProductService>();
// ... باقي الخدمات الست + ICartStorage → SessionCartStorage

// حدود النماذج (إصلاح I-21):
builder.Services.Configure<FormOptions>(o =>
{
    o.ValueCountLimit = 5000;                       // نماذج المنتجات الكبيرة
    o.MultipartBodyLengthLimit = 32 * 1024 * 1024;  // 32MB للصور المتعددة
});

// الجلسة + Identity + Rate Limiting + الترحيل الذاتي...
```

خط الأنابيب: `UseStaticFiles → UseSession → UseRouting → UseRateLimiter → UseAuthentication → UseAuthorization → MapControllerRoute`.

---
---

# 9. مشروع FTD.Api — الواجهة البرمجية REST

مشروع منفصل يشارك نفس Application/Infrastructure — جاهز لتطبيق موبايل أو واجهة SPA مستقبلية.

## 9.1 نقاط النهاية

| Endpoint | Method | الوصف | الحماية |
|---|---|---|---|
| `api/Products` | GET | قائمة منتجات مرقّمة + فلترة | عام |
| `api/Products/{slug}` | GET | تفاصيل منتج | عام |
| `api/Products/categories` | GET | كل الفئات | عام |
| `api/Products/brands` | GET | كل الماركات | عام |
| `api/Orders/checkout` | POST | إنشاء طلب (سلة في الجسم) | عام |
| `api/Orders/track/{number}` | GET | تتبع طلب | عام |
| `api/Contact` | POST | إرسال رسالة تواصل | عام |
| `api/Auth/login` | POST | تسجيل دخول → JWT | عام |
| `api/Admin/dashboard` | GET | إحصائيات | **JWT + Admin** |
| `api/Admin/orders` | GET | الطلبات | **JWT + Admin** |
| `api/Admin/orders/{id}` | GET | تفاصيل طلب | **JWT + Admin** |
| `api/Admin/orders/{id}/status` | PUT | تغيير الحالة | **JWT + Admin** |

## 9.2 المصادقة JWT

- `api/Auth/login` يتحقق عبر Identity (`SignInManager.CheckPasswordSignInAsync` — يحترم الـ Lockout) ثم يصدر توكن موقّعاً HMAC-SHA256 بمفتاح من `appsettings` (`Jwt:Key/Issuer/Audience`)، صلاحية محدودة، يحمل claim الدور.
- مسارات `api/Admin/*` تتطلب `[Authorize(Roles = "Admin")]` بمخطط Bearer.

## 9.3 Swagger

مفعّل في التطوير مع تعريف أمان Bearer — زر "Authorize" يقبل التوكن فيُختبر كل شيء من المتصفح.

---
---

# 10. الواجهة الأمامية (Frontend) — CSS/JS ونظام اللغتين

## 10.1 site.css — 4455 سطر بلا أي إطار عمل

لا Bootstrap ولا Tailwind — CSS يدوي كامل مبني على متغيرات:

```css
:root {
    --primary: #0d6efd;        /* يُحقن ديناميكياً من إعداد site.primary.color */
    --surface: #ffffff;
    --text: #1a1a2e;
    --radius: 14px;
    --shadow-card: 0 6px 24px rgba(0,0,0,.07);
    --transition: all .25s ease;
}
```

### المكونات المبنية يدوياً
- **بطاقة المنتج**: صورة بنسبة ثابتة + Badge خصم محسوب + hover برفع وظل + زر إضافة سريعة.
- **الهيدر**: ثابت (Sticky)، بحث منبثق (Overlay)، عدّاد سلة حي، مبدّل لغة.
- **Hero**: خلفية متدرجة/صورة من الإعدادات + CTA مزدوج.
- **الفوتر**: 4 أعمدة ديناميكية (روابط من `NavigationLink` بموقع footer).
- **Toast notifications**: نظام إشعارات يدوي (نجاح/خطأ) بحركة انزلاق.
- **Skeleton loading** لبطاقات المنتجات أثناء الفلترة بـ AJAX.
- **استجابة كاملة**: Grid يتدرج 4→3→2→1 أعمدة عبر 3 نقاط كسر.

### دعم RTL/LTR المزدوج
```css
html[dir="rtl"] .product-card { text-align: right; }
html[dir="ltr"] .product-card { text-align: left; }
/* خصائص منطقية حيثما أمكن: margin-inline-start بدل margin-left */
```

## 10.2 site.js — 291 سطر Vanilla JS

| الدالة | الوظيفة |
|---|---|
| `initLang()` | قراءة كوكي `ftd_lang` وضبط `dir`/`lang` وإظهار spans اللغة الصحيحة |
| `toggleLang()` | التبديل ar↔en + كتابة الكوكي + إعادة ضبط الاتجاه فورياً |
| `addToCartQuick(id)` | POST بـ fetch + هيدر antiforgery + تحديث العداد + Toast |
| `setCartBadge(n)` / `updateCartCount()` | مزامنة عدّاد الهيدر |
| `openSearch()` / `doSearch()` | Overlay البحث + التوجيه لنتائجه |
| `showToast(msg, type)` | إشعار مؤقت (3 ثوانٍ) |
| `subscribeNewsletter()` | نموذج النشرة (واجهة جاهزة) |

**نمط antiforgery في AJAX** (إصلاح I-03):
```javascript
// _Layout يضع @Html.AntiForgeryToken() مرة واحدة قبل site.js
const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
fetch('/cart/add', {
    method: 'POST',
    headers: { 'RequestVerificationToken': token, 'Content-Type': 'application/json' },
    body: JSON.stringify({ productId, qty })
});
```

## 10.3 نظام اللغتين — SSR أولاً

المشكلة الكلاسيكية: تبديل لغة بـ JS فقط = وميض اللغة الخطأ عند أول تحميل (FOUC).

**الحل المطبق**:
1. اللغة تُخزن في كوكي `ftd_lang` (وليس localStorage) — **الخادم يقرأها**.
2. `_Layout` يضبط `<html dir="rtl" lang="ar">` أو العكس **من الخادم** — أول Paint صحيح دائماً.
3. النصوص الثنائية داخل الصفحة: `<span data-ar="سلة" data-en="Cart">` — `initLang()` يظهر الصحيح.
4. نصوص المحتوى (بلوكات) تُحل خادمياً عبر `Get()` حسب الكوكي — لا ازدواج في الـ HTML المرسل.

## 10.4 admin.css (586 سطر) + page-builder.js (398 سطر)

- **admin.css**: Sidebar قابل للطي، جداول بأسطر تحويم، بطاقات إحصائية بأيقونات ملونة، نماذج بشبكة عمودين، Badges حالات بألوان من قاعدة البيانات.
- **page-builder.js**: منطق منشئ الصفحات — سحب/إفلات لإعادة الترتيب، تهيئة محررات Quill، معاينة حية، رفع صور بالسحب، حفظ AJAX لكل قسم على حدة (تفصيله في §12).

---
---

# 11. لوحة التحكم — كل شاشة بالتفصيل

الدخول: `/admin/login` (محمي بـ Lockout + Rate Limiting). بعد الدخول: `_AdminLayout` بشريط جانبي ثابت يضم كل الأقسام.

## 11.1 الشاشة الرئيسية — Dashboard

**المسار**: `/admin` — **المتحكم**: `AdminDashboardController.Index`

| البطاقة | المصدر |
|---|---|
| إجمالي الطلبات | `Count(SalesOrders)` |
| إجمالي الإيرادات | `Sum(TotalAmount)` للطلبات غير الملغاة |
| عدد المنتجات النشطة | `Count(Products.IsActive)` |
| رسائل غير مقروءة | `Count(ContactMessages.!IsRead)` — مع Badge أحمر في الشريط الجانبي أيضاً |

- **جدول آخر 10 طلبات**: رقم + عميل + إجمالي + حالة ملونة + رابط سريع للتفاصيل.
- **الأكثر مبيعاً**: أعلى 5 منتجات بمجموع الكميات المباعة (تجميع `SalesOrderDetails`).

## 11.2 إدارة المنتجات

**المسار**: `/admin/products`

### شاشة الجدول (Index)
- بحث نصي (اسم/‏SKU) + فلترة بالفئة + ترقيم.
- كل صف: صورة مصغرة، الاسمان، السعر (+القديم مشطوباً)، المخزون، الفئة، الماركة.
- **أزرار سريعة لكل صف**: تعديل | **نسخ** (Duplicate — يستنسخ كل شيء بـ slug جديد) | تبديل نشط | تبديل مميز | حذف (يُرفض برسالة لو للمنتج طلبات مسجلة — حماية `Restrict`).

### شاشة النموذج (Form.cshtml — 535 سطر)
النموذج الأضخم في اللوحة، أقسامه:
1. **المعلومات الأساسية**: الاسمان + slug (مع توليد تلقائي مقترح) + SKU + الفئة + الماركة.
2. **التسعير**: السعر + السعر القديم (يولّد Badge خصم تلقائياً في الواجهة) + المخزون.
3. **الوصف الثنائي**: محررا Quill (عربي RTL / إنجليزي LTR) — المخرجات تُعقَّم خادمياً.
4. **الصور المتعددة**: رفع عدة صور، تحديد الرئيسية، ترتيب بالسحب، حذف فردي — كلها عبر `ImageUploadHelper` الآمن.
5. **المواصفات الديناميكية**: عند اختيار الفئة، AJAX يجلب `BuildAttributeGroupsAsync` فتظهر مجموعات المواصفات الخاصة بهذه الفئة فقط (مقاس/لون للملابس، رام/شاشة للموبايلات…) — checkboxes لكل قيمة.
6. **الحالة**: نشط / مميز.

## 11.3 إدارة الفئات والماركات

- **`/admin/categories`**: جدول + نموذج (اسمان، slug، صورة، ترتيب، نشط). الحذف يُرفض لو الفئة فيها منتجات.
- **`/admin/brands`**: نفس النمط (اسمان، slug، شعار، نشط) — `AdminBrandsController` بعد إصلاحات I-14/I-15 يستخدم `GetBrandByIdAsync` و`ImageUploadHelper`.

## 11.4 إدارة الطلبات

**المسار**: `/admin/orders`
- **الجدول**: فلترة بالحالة (Dropdown من `OrderStatuses`) + بحث برقم/اسم/هاتف + ترقيم.
- **شاشة التفاصيل**: بيانات العميل كاملة، جدول البنود **بأسعار وأسماء اللقطة** (كما كانت وقت الشراء)، المجاميع (فرعي + شحن + كلي)، وملاحظات العميل.
- **تغيير الحالة**: Dropdown + حفظ — الخدمة تتحقق بـ `AnyAsync` أن الحالة موجودة (لا حقن معرفات وهمية). ألوان الحالات تأتي من عمود `ColorHex` في قاعدة البيانات — قابلة للتخصيص.

## 11.5 إدارة المحتوى — Blocks.cshtml (740 سطر)

**المسار**: `/admin/content` — أهم شاشة «تحكم كامل»:

### تبويب البلوكات (64 بلوك)
- مجمعة بحسب الصفحة (Hero / About / Footer / Contact / Stats…).
- كل بلوك: حقلان (عربي/إنجليزي) جنباً إلى جنب + حفظ فردي AJAX.
- البلوكات الطويلة (about.body) بمحرر Quill.

### تبويب الإعدادات (23 مفتاحاً)
| المجموعة | المفاتيح | الأثر |
|---|---|---|
| إظهار أقسام الرئيسية | `homepage.show.hero/featured/categories/latest/brands/stats/about/…` (10 مفاتيح) | Toggle يخفي القسم كلياً (ولا يُستعلم عن بياناته) |
| ترتيب الأقسام | `homepage.sections.order` | قائمة سحب/إفلات تعيد ترتيب الرئيسية بالكامل |
| الشحن | `shipping.fee`, `shipping.free.above` | تسري فوراً على السلة والدفع |
| الهوية | `site.primary.color`, شعار، أيقونة | اللون يُحقن في `--primary` فيتغير الموقع كله |

## 11.6 المواصفات وأقسام الرئيسية — AdminAttributesAndSections

**المسار**: `/admin/attributes`

### إدارة المواصفات
- `Index/Create/Edit/Delete`: مواصفة (اسمان + الفئات المرتبطة بها).
- `AddValue/DeleteValue`: قيم كل مواصفة (أحمر/أزرق… — S/M/L/XL…).
- الربط بالفئات يحدد أي مواصفات تظهر في نموذج المنتج وأي فلاتر تظهر للزائر.

### الأقسام المخصصة للرئيسية (HomepageSections)
- `Manage`: قائمة الأقسام المخصصة.
- `AddSection/SaveSection/DeleteSection`: قسم بعنوان ثنائي + اختيار منتجات محددة يدوياً (تُعرض بترتيبها المحفوظ عبر `GetByIdsOrderedAsync`).
- `ToggleSectionVisible` + `MoveSectionUp/Down` + `UpdateSortOrders`: تحكم فوري بالظهور والترتيب.
- `UploadImage`: بانر اختياري للقسم.

**المعنى العملي**: الأدمن يبني «عروض الجمعة البيضاء» كقسم كامل بمنتجات مختارة وبانر — بدون سطر كود.

## 11.7 إدارة القوائم — AdminNavigation

**المسار**: `/admin/navigation`
- `Index`: روابط الهيدر والفوتر في قائمتين.
- `Create`: نص ثنائي + URL + ترتيب + الموقع (header/footer) + فتح بتبويب جديد.
- `SetLocation` / `Delete`: نقل وحذف فوري.
- الهيدر والفوتر في `_Layout` يُبنيان من هذه البيانات — تعديل القائمة لا يتطلب نشراً.

## 11.8 الرسائل ومعلومات التواصل — AdminMessages

**المسار**: `/admin/messages`
- **الرسائل**: جدول بغير المقروء مميزاً، عرض كامل يعلّم كمقروء تلقائياً، حذف.
- **معلومات التواصل**: نموذج صف `ContactInfo` الوحيد — هاتفان، بريد، عنوان ثنائي، 4 روابط سوشيال، ساعات عمل، رابط خريطة، **ومفتاح إظهار/إخفاء مستقل لكل حقل** — صفحة «اتصل بنا» تتشكل بالكامل من هنا.

---
---

# 12. منشئ الصفحات المرئي (Visual Page Builder)

الميزة الأكثر تمايزاً: الأدمن يبني صفحات كاملة بأقسام مرئية — page-builder.js (398 سطر) + `PageSection` + `ContentService`.

## 12.1 أنواع الأقسام السبعة

| النوع | المحتوى | حالات الاستخدام |
|---|---|---|
| `hero` | عنوان + وصف + صورة خلفية + زر CTA | بانر ترويجي |
| `text` | HTML غني (Quill) ثنائي اللغة | فقرات، سياسات، قصص |
| `image` | صورة كاملة العرض + تعليق | فواصل بصرية |
| `text_image` | نص + صورة (يمين/يسار) | ميزة + توضيح |
| `gallery` | شبكة صور متعددة | معارض، شهادات مصورة |
| `cta` | شريط دعوة لفعل بخلفية ملونة | «تسوق الآن» |
| `spacer` | مسافة رأسية قابلة للضبط | تحكم بالإيقاع البصري |

## 12.2 كيف يعمل

1. **الإضافة**: زر «+ قسم» يفتح منتقي الأنواع → يُدرج قالب فارغ في القائمة.
2. **التحرير**: كل قسم بطاقة قابلة للطي فيها حقوله (نصوص ثنائية، Quill للـ HTML، رفع صور بالسحب).
3. **الترتيب**: سحب/إفلات — `SortOrder` يُحدّث بـ AJAX فوراً.
4. **الإظهار**: Toggle `IsVisible` لكل قسم (مسودات مخفية).
5. **الحفظ**: كل قسم يُحفظ منفرداً (`SaveSectionAsync`) — لا فقدان عمل عند خطأ في قسم واحد.
6. **التخزين**: الحقول تُسلسل JSON في `ContentJson` — مخطط مرن بلا أعمدة جديدة لكل نوع.
7. **العرض**: الواجهة تقرأ `GetPageSectionsAsync(pageKey)` وترسم كل نوع بقالبه الجزئي — **مع تعقيم HTML دائماً**.

```json
// مثال ContentJson لقسم text_image:
{
  "titleAr": "لماذا نحن؟", "titleEn": "Why Us?",
  "bodyAr": "<p>...</p>", "bodyEn": "<p>...</p>",
  "imageUrl": "/uploads/sections/a1b2c3.webp",
  "imageSide": "right", "ctaText": "اعرف أكثر", "ctaUrl": "/about"
}
```

---
---

# 13. نظام المحتوى الديناميكي — المرجع الكامل للمفاتيح

## 13.1 فلسفة النظام

**«لا نص ثابت في الكود»** — كل جملة يراها الزائر قابلة للتعديل من اللوحة. ثلاث طبقات:
1. **ContentBlocks** = النصوص (ماذا يُقال) — مفاتيح `xxx.ar` / `xxx.en`.
2. **SiteSettings** = السلوك (ماذا يظهر وكيف) — مفاتيح قيمها نص/رقم/bool/JSON.
3. **PageSections** = الهيكل (أقسام كاملة) — منشئ الصفحات.

## 13.2 مرجع مفاتيح البلوكات (مختارات من الـ 64)

| المفتاح | الموضع |
|---|---|
| `hero.title` / `hero.subtitle` / `hero.cta` | بانر الرئيسية |
| `about.heading` / `about.body` | قسم من نحن (+ صفحة about) |
| `stats.customers` / `stats.products` / `stats.orders` | بطاقات الإحصائيات (مع لاحقة `+` تُدار بـ TrimEnd) |
| `footer.tagline` / `footer.copyright` | الفوتر |
| `contact.heading` / `contact.subheading` | صفحة التواصل |
| `featured.heading` / `latest.heading` / `brands.heading` | عناوين أقسام الرئيسية |
| `checkout.note` / `confirmation.message` | نصوص الدفع والتأكيد |

كل مفتاح أعلاه له نسختان `.ar` و`.en` — الحل عبر `Get("hero.title")` الذي يلحق اللاحقة حسب كوكي اللغة.

## 13.3 مرجع مفاتيح الإعدادات (الـ 23)

| المفتاح | النوع | الافتراضي |
|---|---|---|
| `homepage.show.hero` | bool | true |
| `homepage.show.featured` | bool | true |
| `homepage.show.categories` | bool | true |
| `homepage.show.latest` | bool | true |
| `homepage.show.brands` | bool | true |
| `homepage.show.stats` | bool | true |
| `homepage.show.about` | bool | true |
| `homepage.show.newsletter` | bool | true |
| `homepage.show.customsections` | bool | true |
| `homepage.show.pagesections` | bool | true |
| `homepage.sections.order` | CSV | hero,featured,categories,… |
| `shipping.fee` | decimal | 50 |
| `shipping.free.above` | decimal | 1000 (0 = معطل) |
| `site.primary.color` | hex | #0d6efd |
| `site.logo.url` / `site.favicon.url` | مسار | — |
| + مفاتيح عناوين/سلوك إضافية | | |

**نمط القراءة الآمن في الكود**: قيمة غائبة/فاسدة = السقوط للافتراضي — إعداد مكسور لا يكسر صفحة أبداً.

---
---

# 14. السلة ودورة حياة الطلب — من الضغطة للتسليم

## 14.1 رحلة العميل الكاملة

```
تصفّح → إضافة للسلة (AJAX) → مراجعة السلة → إدخال بيانات الشحن
      → تأكيد (COD) → صفحة تأكيد برقم FTD → تتبع بالرقم لاحقاً
```

## 14.2 السلة خطوة بخطوة

### الإضافة (زر البطاقة السريع أو صفحة التفاصيل)
1. `addToCartQuick(id)` يرسل POST بـ fetch + توكن antiforgery.
2. `CartOrderController.AddToCart` → `CartService.AddAsync`.
3. الخدمة **تتحقق من قاعدة البيانات**: منتج موجود؟ نشط؟ (لا ثقة بمعرف قادم من العميل).
4. `qty = Math.Max(1, qty)` — الكميات السالبة/الصفرية مستحيلة (إصلاح I-08).
5. موجود في السلة؟ زيادة الكمية. جديد؟ إضافة بند.
6. سيريالايز JSON → الجلسة → إرجاع العدد الجديد → تحديث Badge + Toast.

### العرض والتعديل
- صفحة `/cart`: جدول البنود (صورة + اسم + سعر حي + كمية قابلة للتعديل + إجمالي بند).
- **الأسعار تُعاد قراءتها من قاعدة البيانات** عند كل عرض — سعر الجلسة مرجع للهوية فقط لا للحساب.
- ملخص: مجموع فرعي + شحن (من الإعدادات، مجاني فوق الحد) + الكلي.
- منتج حُذف أو عُطّل بعد إضافته؟ يُستبعد من الملخص بهدوء.

## 14.3 الدفع (Checkout) بالتفصيل

**GET `/checkout`**: سلة فارغة → توجيه للسلة. غير ذلك: نموذج `CheckoutViewModel`:

| الحقل | الحد | إلزامي |
|---|---|---|
| الاسم الكامل | 150 | ✅ |
| الهاتف | 20 | ✅ |
| العنوان | 300 | ✅ |
| المدينة | 100 | ✅ |
| المحافظة | 100 | — |
| البريد | 200 | — |
| ملاحظات | 1000 | — |

**POST**: antiforgery → ModelState → `CreateOrderAsync`:
1. جلب منتجات السلة **دفعة واحدة** بقاموس (إصلاح N+1 — I-05).
2. بناء البنود بلقطات (اسمان + سعر لحظة الشراء).
3. حساب المجاميع خادمياً 100% (مدخلات العميل = عنوان فقط، أرقام = صفر ثقة).
4. رقم `FTD{yyyyMMddHHmmss}{عداد}` + الحالة الابتدائية «جديد».
5. `SaveChangesAsync` واحدة ذرّية → مسح السلة → صفحة تأكيد.

## 14.4 دورة الحالات السبع

```
        ┌──────────────────────────────┐
جديد → مؤكد → قيد التجهيز → شُحن → سُلّم
  │       │                    
  └───────┴──→ ملغي            سُلّم ──→ مرتجع
```
كل تغيير يفعله الأدمن من شاشة الطلب — والعميل يرى الحالة الحية عبر `/order/track` برقم طلبه فقط (لا حساب مطلوب — نموذج Guest Checkout كامل).

---
---

# 15. نظام المواصفات والفلترة الوجهية (Faceted Search)

## 15.1 نموذج البيانات (4 جداول)

```
ProductAttribute (لون، مقاس، رام…)
   ├── AttributeValue (أحمر، أزرق… / S, M, L…)
   ├── CategoryAttribute ← أي فئات تستخدم هذه المواصفة
ProductAttributeValue ← أي قيم يحملها كل منتج
```

- المواصفات **معرفة بالبيانات لا بالكود**: متجر ملابس ومتجر إلكترونيات يعملان بنفس الكود.
- ربط المواصفة بالفئة يضبط: (أ) ما يظهر في نموذج المنتج الإداري، (ب) ما يظهر كفلاتر للزائر.

## 15.2 الفلترة عند الزائر

صفحة الفئة تعرض شريطاً جانبياً بالفلاتر المبنية من `GetAvailableFacetsAsync`:
- **لكل مواصفة**: قيمها التي تملكها منتجات هذه الفئة فعلاً + **عدد المنتجات لكل قيمة**.
- فلتر السعر (من/إلى) + ترتيب (الأحدث/سعر تصاعدي/تنازلي).
- التحديد يبني سلسلة `attrs=1:3|4;2:7` في الـ URL — **الفلاتر قابلة للمشاركة والحفظ** (حالة كاملة في الرابط).

## 15.3 منطق الاستعلام

```csharp
// داخل GetFilteredBySlugAsync — منطق AND بين المواصفات، OR داخل الواحدة:
foreach (var (attributeId, valueIds) in filters)
{
    query = query.Where(p => p.AttributeValues
        .Any(av => valueIds.Contains(av.AttributeValueId)));
}
// النتيجة: (أحمر OR أزرق) AND (مقاس XL) — سلوك المتاجر العالمية القياسي
```

كل شيء يُترجم لـ SQL واحد — لا فلترة في الذاكرة.

---
---

# 16. الأمان — الطبقات الثماني

## 16.1 خريطة الدفاعات

| # | الطبقة | التنفيذ |
|---|---|---|
| 1 | **CSRF** | `[ValidateAntiForgeryToken]` على كل POST (عام + إداري + سلة AJAX عبر الهيدر) — إصلاحات I-02/I-03 |
| 2 | **XSS** | Razor encoding افتراضياً + `HtmlSanitizer` بقائمة بيضاء لكل HTML مخزن (Quill) |
| 3 | **SQL Injection** | EF Core parameterized 100% — صفر SQL خام في المشروع |
| 4 | **المصادقة** | Identity + قفل 5 محاولات + Rate Limiting نافذة ثابتة على `/admin/login` |
| 5 | **التفويض** | `[Authorize(Roles="Admin")]` على كل متحكم إداري + JWT بأدوار للـ API |
| 6 | **رفع الملفات** | `ImageUploadHelper`: امتدادات بيضاء، 5MB، أسماء GUID (لا Path Traversal) — I-15 |
| 7 | **حدود المدخلات** | DataAnnotations على كل DTO/ViewModel + `FormOptions` (5000 مفتاح / 32MB) — I-19/I-21 |
| 8 | **منطق العمل** | أسعار خادمية فقط، كميات `Max(1,…)`، تحقق وجود الحالات/المنتجات، `Restrict` على FK التاريخية |

## 16.2 مبدأ «صفر ثقة بالعميل» — أمثلة مطبقة

- السلة تخزن معرفات، **الأسعار تُقرأ من القاعدة** عند العرض والشراء.
- تغيير حالة طلب: `AnyAsync` يؤكد أن `statusId` حقيقي.
- إضافة للسلة: المنتج يُفحص وجوداً ونشاطاً.
- الملفات: الامتداد يُفحص، الاسم يُستبدل بـ GUID، الحجم محدود.

## 16.3 ما يجب فعله عند النشر الفعلي (Production Checklist)

- [ ] HTTPS إجباري + HSTS
- [ ] أسرار (`Jwt:Key`, SMTP, ConnectionString) في متغيرات بيئة/Key Vault — ليس `appsettings.json`
- [ ] تغيير بيانات أدمن البذر فوراً
- [ ] Security Headers (CSP, X-Frame-Options, X-Content-Type-Options)
- [ ] نسخ احتياطي مجدول لقاعدة البيانات و`wwwroot/uploads`

---
---

# 17. الأداء — القرارات المطبقة

| القرار | الأثر |
|---|---|
| `AsNoTracking` على **كل** مسارات القراءة (23+ استعلاماً عبر 6 خدمات — إصلاحات I-04..I-07) | إلغاء كلفة Change Tracker حيث لا حاجة |
| إسقاط مبكر لـ DTO داخل `Select` | SQL بالأعمدة المطلوبة فقط، لا Over-fetching |
| جلب منتجات الطلب بقاموس دفعة واحدة | N+1 → استعلامين ثابتين مهما كبرت السلة |
| عدّاد السلة من الجلسة | صفر DB لكل عرض هيدر |
| أقسام الرئيسية المخفية لا تُستعلم | التخصيص يوفّر أداء تلقائياً |
| فهارس فريدة على Slugs وOrderNumber | بحث O(log n) على المسارات الساخنة |
| ترقيم بـ `Skip/Take` + `CountAsync` منفصلة | لا تحميل قوائم كاملة أبداً |
| CSS/JS يدوي بلا أطر | ~لا وزن زائد؛ حزمة أمامية صغيرة جداً |

**غير مطبق بعد (فرص §21)**: Response Caching، Output Caching لبيانات البلوكات، ضغط الصور تلقائياً/WebP، CDN.

---
---

# 18. مخطط قاعدة البيانات الكامل

## 18.1 الجداول (16 دومين + Identity)

```
┌─────────────┐      ┌──────────────┐      ┌───────────────┐
│  Category   │◄─────│   Product    │─────►│     Brand     │
└─────────────┘ 1  * └──────┬───────┘ *  1 └───────────────┘
                     1│     │1
              ┌───────┘     └────────┐
              ▼ *                    ▼ *
      ┌──────────────┐    ┌─────────────────────┐
      │ ProductImage │    │ProductAttributeValue│──► AttributeValue ──► ProductAttribute
      └──────────────┘    └─────────────────────┘                            ▲
                                                     CategoryAttribute ──────┘
┌────────────┐ 1   * ┌──────────────────┐ *  1(Restrict!) ┌─────────┐
│ SalesOrder │──────►│ SalesOrderDetail │────────────────►│ Product │
└─────┬──────┘       └──────────────────┘                 └─────────┘
      │ *  1
      ▼
┌─────────────┐
│ OrderStatus │ (7 صفوف مزروعة)
└─────────────┘

مستقلة: ContentBlock │ SiteSetting │ PageSection │ NavigationLink
        │ HomepageSection │ ContactInfo │ ContactMessage
Identity: AspNetUsers │ AspNetRoles │ AspNetUserRoles │ ...
```

## 18.2 الأعمدة الحرجة

| الجدول | أعمدة تستحق الانتباه |
|---|---|
| `Products` | `Slug` (فريد)، `Price/OldPrice (18,2)`، `StockQuantity`, `IsActive`, `IsFeatured`, `Sku` |
| `SalesOrders` | `OrderNumber` (فريد)، `SubTotal/ShippingFee/TotalAmount`، بيانات الشحن كاملة، `OrderStatusId` |
| `SalesOrderDetails` | **`ProductNameAr/En` + `UnitPrice` لقطات** + `ProductId` بـ **Restrict** |
| `ContentBlocks` | `Key` (فريد) + `Value` — نمط قاموس |
| `PageSections` | `PageKey` + `SectionType` + `ContentJson` + `SortOrder` + `IsVisible` |
| `OrderStatuses` | `NameAr/NameEn/ColorHex/SortOrder` — الحالات نفسها قابلة للتخصيص |

---
---

# 19. التشغيل والنشر

## 19.1 التشغيل محلياً

```bash
# المتطلبات: .NET 9 SDK + SQL Server (أو LocalDB)
git clone <repo> && cd uni-shop

# ضبط الاتصال في FTD.Web/appsettings.json:
#   "DefaultConnection": "Server=.;Database=UniShop;Trusted_Connection=True;TrustServerCertificate=True"

dotnet run --project FTD.Web
# ✅ الهجرات تُطبق تلقائياً + الأدمن يُزرع + 64 بلوك + 23 إعداد + 7 حالات
# المتجر: https://localhost:xxxx  —  اللوحة: /admin/login
```

## 19.2 أوامر EF Core المرجعية

```bash
# في بيئة الساندبوكس يلزم أولاً:
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"

# هجرة جديدة (إلزامية مع أي تغيير في AppDbContext):
dotnet ef migrations add MigrationName \
  --project FTD.Infrastructure --startup-project FTD.Web

# تطبيق يدوي (اختياري — التطبيق يرحّل ذاتياً):
dotnet ef database update --project FTD.Infrastructure --startup-project FTD.Web
```

## 19.3 النشر

- **IIS / Windows**: `dotnet publish -c Release` → مجلد النشر → App Pool بـ No Managed Code.
- **Linux + Nginx**: Kestrel خلف Nginx كـ Reverse Proxy + systemd service.
- **Docker**: Dockerfile قياسي متعدد المراحل (sdk → aspnet runtime) — غير مضمّن حالياً (فرصة §21).
- **قاعدة البيانات**: Azure SQL / SQL Server — سلسلة الاتصال عبر متغير بيئة `ConnectionStrings__DefaultConnection`.
- **الملفات المرفوعة**: `wwwroot/uploads` تحتاج Volume دائم أو نقلها لتخزين سحابي (§21).

---
---

# 20. سجل الجودة — التدقيق الشامل (2026-07-13)

خضع المشروع لتدقيق هندسي من 4 مراحل غطّى 26 ملاحظة — **كلها أُصلحت والبناء نظيف 0 أخطاء / 0 تحذيرات** (تفاصيل كاملة في `AUDIT_REPORT.md`):

| الفئة | أبرز الإصلاحات |
|---|---|
| سلامة البيانات | FK تفاصيل الطلب Cascade→**Restrict** + هجرة `HardenOrderDetailProductFk` |
| CSRF | تغطية 100% للـ POSTs + توكن عام للـ AJAX في `_Layout` |
| الأداء | `AsNoTracking` على 23+ استعلام قراءة عبر 6 خدمات + إنهاء N+1 في الطلبات |
| التحقق | DataAnnotations كاملة على DTOs/ViewModels + حدود Contact/Checkout |
| رفع الملفات | توحيد عبر `ImageUploadHelper` (امتدادات بيضاء/5MB/GUID) |
| معالجة الأخطاء | فلاتر `when (ex is InvalidOperationException or ArgumentException)` بدل ابتلاع كل شيء |
| النظافة | إزالة usings ميتة، طرق غير مستخدمة، توحيد أنماط ModelState/RedisplayForm |
| المتانة | فشل الهجرة يُسجَّل `LogCritical` ويمنع الإقلاع؛ حدود FormOptions مضبوطة |

**الوثائق الحية**: `PROJECT_COMPLETE_DOCUMENTATION.md` (§1–11) + `AUDIT_REPORT.md` + هذا الدليل.

---
---

# 21. 🔍 تحليل الفجوات — ما الذي ينقص المتجر بالضبط؟

هذا القسم صريح بلا مجاملة: مقارنة Uni-Shop الحالي بمتجر عالمي مكتمل (Shopify/WooCommerce-class)، فجوة فجوة، مع تقدير الأهمية والجهد.

## 21.1 جدول الفجوات الشامل

| # | الفجوة | الأهمية | الجهد | المرحلة المقترحة |
|---|---|---|---|---|
| G-01 | بوابات دفع إلكتروني (لا يوجد غير COD) | 🔴 حرجة | متوسط | 1 |
| G-02 | حسابات العملاء (تسجيل/دخول/طلباتي) | 🔴 حرجة | متوسط | 1 |
| G-03 | خصم المخزون تلقائياً عند الطلب | 🔴 حرجة | صغير | 1 |
| G-04 | كوبونات وأكواد خصم | 🔴 حرجة | متوسط | 1 |
| G-05 | إشعارات بريد للعميل (تأكيد/شحن) | 🔴 حرجة | صغير | 1 |
| G-06 | تقييمات ومراجعات المنتجات | 🟠 عالية | متوسط | 2 |
| G-07 | قائمة الأمنيات (Wishlist) | 🟠 عالية | صغير | 2 |
| G-08 | متغيرات المنتج (Variants: لون+مقاس بسعر/مخزون مستقل) | 🟠 عالية | كبير | 2 |
| G-09 | سلة دائمة (تنجو من انتهاء الجلسة) | 🟠 عالية | صغير | 2 |
| G-10 | مناطق شحن بأسعار مختلفة (محافظات) | 🟠 عالية | متوسط | 1 |
| G-11 | SEO تقني: sitemap.xml, robots, Schema.org, OG tags | 🟠 عالية | صغير | 2 |
| G-12 | تنبيهات انخفاض المخزون للأدمن | 🟡 متوسطة | صغير | 1 |
| G-13 | تقارير وتحليلات (مبيعات زمنية/فئات/تحويل) | 🟡 متوسطة | متوسط | 3 |
| G-14 | نشرة بريدية فعلية (الواجهة موجودة بلا خلفية) | 🟡 متوسطة | صغير | 3 |
| G-15 | تكامل شركات الشحن (بوليصة/تتبع تلقائي) | 🟡 متوسطة | كبير | 3 |
| G-16 | سلات متروكة + استرجاعها بالبريد | 🟡 متوسطة | متوسط | 3 |
| G-17 | تعدد العملات | 🟢 تحسين | متوسط | 4 |
| G-18 | لغات إضافية (البنية ثنائية حالياً) | 🟢 تحسين | كبير | 4 |
| G-19 | PWA (تثبيت + أوف لاين + إشعارات Push) | 🟢 تحسين | متوسط | 4 |
| G-20 | توصيات ذكية (AI) وبحث دلالي | 🟢 تحسين | كبير | 4 |
| G-21 | تعدد الأدوار الإدارية (محرر/مدير طلبات/مشرف) | 🟡 متوسطة | صغير | 2 |
| G-22 | سجل نشاط إداري (Audit Log — من غيّر ماذا ومتى) | 🟡 متوسطة | متوسط | 2 |
| G-23 | Caching (Output/Response/Redis) | 🟡 متوسطة | صغير | 2 |
| G-24 | اختبارات آلية (Unit/Integration — صفر حالياً) | 🟠 عالية | متوسط | 1 |
| G-25 | Docker + CI/CD Pipeline | 🟡 متوسطة | صغير | 2 |
| G-26 | تحسين صور تلقائي (WebP/أحجام متعددة) | 🟡 متوسطة | متوسط | 2 |
| G-27 | صفحات CMS حرة (منشئ الصفحات لصفحات جديدة كلياً بـ slug) | 🟡 متوسطة | صغير | 2 |
| G-28 | إرجاع/استرداد بتدفق كامل (RMA) | 🟢 تحسين | كبير | 4 |
| G-29 | نقاط ولاء ومكافآت | 🟢 تحسين | متوسط | 4 |
| G-30 | دردشة/واتساب مدمج للدعم | 🟢 تحسين | صغير | 3 |

## 21.2 تفصيل الفجوات الحرجة (🔴)

### G-01: بوابات الدفع — الفجوة رقم واحد
**الوضع**: `PaymentMethod` في الطلب ثابتة "COD". لا تكامل مع أي بوابة.
**الأثر**: خسارة كل عميل يفضّل الدفع الإلكتروني + استحالة البيع الرقمي.
**الحل المعماري المقترح**:
```csharp
public interface IPaymentGateway
{
    string Name { get; }                                       // "paymob", "stripe", "fawry"
    Task<PaymentInitResult> InitiateAsync(OrderDto order);     // إنشاء جلسة دفع → رابط
    Task<PaymentVerifyResult> VerifyCallbackAsync(HttpRequest r); // تحقق Webhook بالتوقيع
}
// تسجيل عدة بوابات + تفعيلها/تعطيلها من SiteSettings (payment.paymob.enabled…)
// جدول جديد PaymentTransaction: OrderId, Gateway, TransactionRef, Amount, Status, RawResponse
```
**بوابات السوق المصري**: Paymob (كروت+محافظ)، Fawry، Vodafone Cash، InstaPay + Stripe/PayPal للدولي.
**قاعدة ذهبية**: الطلب يُنشأ بحالة «بانتظار الدفع» ولا يتأكد إلا بـ Webhook موقَّع من البوابة — **أبداً** من redirect العميل.

### G-02: حسابات العملاء
**الوضع**: Identity موجود لكن للأدمن فقط — الشراء كله Guest.
**الخطة**: فتح التسجيل للعملاء بدور `Customer`، ربط `SalesOrder.UserId` (اختياري nullable — يبقى Guest Checkout متاحاً)، صفحات: طلباتي، عناويني المحفوظة، بياناتي. ربط الطلبات القديمة بالبريد/الهاتف عند التسجيل.

### G-03: خصم المخزون
**الوضع**: `StockQuantity` موجود ويُعرض لكنه **لا يُخصم** عند الطلب ولا يمنع الشراء عند النفاد.
**الحل** (صغير وعالي الأثر):
```csharp
// داخل CreateOrderAsync قبل الحفظ:
if (product.StockQuantity < qty)
    throw new InvalidOperationException($"الكمية المتاحة من {product.NameAr} هي {product.StockQuantity} فقط");
product.StockQuantity -= qty;      // يُحفظ في نفس SaveChangesAsync الذرّية
// + عند الإلغاء/الإرجاع: إعادة الكمية
// + للحماية من التزامن: RowVersion (Concurrency Token) على Products
```

### G-04: الكوبونات
**التصميم**: جدول `Coupon` (Code فريد، نوع النسبة/مبلغ ثابت، حد أدنى للسلة، حد استخدام كلي/للعميل، تاريخا بداية/نهاية، فئات/منتجات مستهدفة اختيارياً، IsActive) + جدول `CouponUsage`. حقل في السلة + تحقق خادمي كامل + لقطة الخصم في `SalesOrder.DiscountAmount` — وإدارة كاملة من اللوحة.

### G-05: بريد العميل
**الوضع**: `EmailService` جاهز لكنه يُستخدم لإشعار الأدمن فقط.
**المطلوب**: قوالب بريد ثنائية اللغة (تأكيد طلب بجدول البنود، تغيّرت الحالة، شُحن برقم تتبع) — تُرسل خلفياً غير حاجبة، وتُدار نصوصها من ContentBlocks (اتساقاً مع فلسفة المشروع).

## 21.3 فجوات هندسية (ليست ميزات)

- **G-24 الاختبارات**: صفر اختبار آلي. الحد الأدنى المقترح: مشروع `FTD.Tests` بـ xUnit + EF InMemory/SQLite — أولويات: `OrderService.CreateOrderAsync` (المجاميع/اللقطات/المخزون)، `CartService` (الكميات)، منطق الفلترة، الكوبونات مستقبلاً.
- **G-23 الكاش**: البلوكات والإعدادات تُقرأ من DB كل طلب — `IMemoryCache` بإبطال عند الحفظ (Cache-Aside) يقلل حمل القاعدة ~70% للصفحات العامة.
- **G-25 Docker/CI**: Dockerfile + docker-compose (تطبيق + SQL Server) + GitHub Actions (build→test→publish) — أساس أي نشر احترافي.
- **G-26 الصور**: المرفوعات تُحفظ كما هي — مطلوب توليد WebP + مقاسات (thumb/medium/full) عبر ImageSharp عند الرفع، و`<picture>/srcset` في البطاقات.

---
---

# 22. 🚀 خارطة الطريق — المرحلة 1: الأساسيات التجارية (شهر 1–2)

**الهدف**: تحويل المتجر من «كتالوج بطلبات COD» إلى **متجر يبيع فعلاً** بدفع إلكتروني ومخزون حقيقي.

## 22.1 نطاق المرحلة

| الأسبوع | التسليمات |
|---|---|
| 1 | خصم المخزون + RowVersion + تنبيه انخفاض المخزون (G-03, G-12) + مشروع الاختبارات (G-24) |
| 2–3 | بوابة دفع أولى (Paymob) بنمط `IPaymentGateway` + جدول المعاملات + Webhooks (G-01) |
| 4–5 | حسابات العملاء: تسجيل/دخول/طلباتي/عناويني + ربط الطلبات (G-02) |
| 6 | الكوبونات كاملة بإدارتها (G-04) |
| 7 | مناطق الشحن بالمحافظات (جدول `ShippingZone`: المحافظة + السعر + مدة التوصيل) (G-10) |
| 8 | بريد العميل بالقوالب الثنائية (G-05) + صقل واختبار شامل |

## 22.2 تغييرات قاعدة البيانات (هجرات المرحلة)

```
AddStockConcurrency        → RowVersion على Products
AddPaymentTransactions     → جدول المعاملات + OrderStatus «بانتظار الدفع»
AddCustomerAccounts        → SalesOrder.UserId (nullable) + جدول CustomerAddress
AddCoupons                 → Coupon + CouponUsage + SalesOrder.DiscountAmount/CouponCode
AddShippingZones           → ShippingZone + ربط Checkout بالمحافظة
```

## 22.3 إضافات لوحة التحكم في هذه المرحلة

- شاشة **بوابات الدفع**: تفعيل/تعطيل كل بوابة + مفاتيحها (مشفرة) + وضع الاختبار/الحي.
- شاشة **الكوبونات**: جدول + نموذج كامل + إحصائية استخدام لكل كوبون.
- شاشة **مناطق الشحن**: جدول المحافظات بالأسعار والمدد.
- Badge «مخزون منخفض» في الداشبورد + فلتر جاهز في جدول المنتجات.
- شاشة **العملاء**: قائمة، تفاصيل عميل بطلباته وإجمالي إنفاقه.

**معيار إتمام المرحلة**: عميل يدفع ببطاقة، المخزون يُخصم، يصله بريد تأكيد، ويرى طلبه في حسابه — من غير تدخل يدوي واحد.

---
---

# 23. 🚀 المرحلة 2: تجربة العميل والثقة (شهر 3–4)

**الهدف**: رفع معدل التحويل والثقة — العميل يشتري أكثر ويعود.

## 23.1 نطاق المرحلة

### التقييمات والمراجعات (G-06)
- جدول `ProductReview`: ProductId, UserId/GuestName, Rating(1–5), Title, Body, **IsApproved**, AdminReply, CreatedAt.
- **الشراء المُتحقق**: شارة «مشترٍ موثّق» لمن له طلب مُسلَّم بهذا المنتج.
- نجوم متوسطة على البطاقة + توزيع النجوم في صفحة المنتج + Schema.org `AggregateRating` (يظهر في نتائج جوجل).
- لوحة: طابور اعتماد، رد الإدارة، حظف/حظر.

### قائمة الأمنيات (G-07)
- للمسجل: جدول `WishlistItem`. للزائر: localStorage يُدمج عند التسجيل.
- قلب على كل بطاقة + صفحة «أمنياتي» + عدّاد بالهيدر.
- ذهبية للتسويق لاحقاً: «منتج بأمنياتك انخفض سعره!»

### متغيرات المنتج (G-08) — الأكبر هندسياً
```
ProductVariant: ProductId + SKU + Price? + StockQuantity + ImageId?
VariantAttributeValue: ربط كل متغير بقيمه (أحمر+XL)
```
- صفحة المنتج: أزرار اختيار تحدّث السعر/الصورة/المخزون فورياً.
- السلة والطلب يشيران للمتغير (لقطة اسمه الكامل «تيشيرت — أحمر / XL»).
- نموذج الإدارة: مصفوفة توليد تلقائي للتوليفات (ألوان × مقاسات) مع تحرير جماعي.
- **قرار متعمد تأجيلها للمرحلة 2**: تتطلب لمس السلة والطلب والفلترة معاً — تُبنى فوق أساس مستقر.

### الباقي
- **سلة دائمة (G-09)**: نقل `ICartStorage` لتنفيذ DB للمسجلين (المعمارية جاهزة — تنفيذ جديد للواجهة فقط، صفر تغيير في `CartService`).
- **SEO التقني (G-11)**: `/sitemap.xml` ديناميكي، robots.txt، Canonical، OG/Twitter Cards من بيانات المنتج، Schema.org (Product/Offer/Breadcrumb)، ALT إلزامي للصور.
- **أدوار إدارية (G-21)**: `SuperAdmin/ContentEditor/OrderManager` بسياسات Authorize لكل قسم.
- **سجل النشاط (G-22)**: جدول `AuditLog` يلتقط (مَن/ماذا/متى/القيم القديمة والجديدة JSON) عبر Interceptor في EF — وشاشة عرض بفلترة.
- **الكاش (G-23)** + **Docker/CI (G-25)** + **الصور WebP (G-26)** + **CMS حر (G-27)**: منشئ الصفحات يُعمم لإنشاء صفحات جديدة كلياً بـ `/p/{slug}` — سياسات، أدلة مقاسات، صفحات هبوط للحملات.

**معيار الإتمام**: منتج بمتغيرات يُشترى بمراجعات ظاهرة في جوجل، وسلة العميل تنتظره أسبوعاً، واللوحة بثلاثة أدوار وسجل كامل.

---
---

# 24. 🚀 المرحلة 3: التسويق والنمو (شهر 5–6)

**الهدف**: المتجر يسوّق نفسه — بيانات، أتمتة، واسترجاع مبيعات ضائعة.

## 24.1 التحليلات والتقارير (G-13)

شاشة تقارير كاملة في اللوحة:
- **مبيعات زمنية**: يومي/أسبوعي/شهري برسم بياني (Chart.js) + مقارنة بالفترة السابقة.
- **حسب الفئة/الماركة/المنتج**: الأعلى إيراداً وكمية.
- **قمع التحويل**: زيارات → إضافات سلة → Checkout → طلبات (يتطلب جدول أحداث خفيف `StoreEvent`).
- **جغرافيا**: الطلبات بالمحافظة (خريطة حرارية).
- **تصدير CSV/Excel** لكل تقرير + تكامل GA4/Meta Pixel يُفعَّل بمفاتيح من الإعدادات.

## 24.2 السلات المتروكة (G-16)

- Checkout يحفظ مسودة (بريد/هاتف + محتوى السلة) عند أول إدخال.
- مهمة خلفية (`BackgroundService`) ترسل بعد 3 ساعات: «سلتك في انتظارك» + رابط استرجاع — وبعد 24 ساعة: نفس الرسالة بكوبون تلقائي.
- شاشة لوحة: السلات المتروكة وقيمتها ونسبة الاسترجاع (معيار الصناعة: استرجاع 5–15% منها = زيادة مبيعات مباشرة).

## 24.3 النشرة والأتمتة (G-14)

- تفعيل خلفية النشرة: جدول `NewsletterSubscriber` + تأكيد مزدوج + إلغاء اشتراك قانوني.
- حملات من اللوحة: محرر رسالة (Quill) + إرسال مجدول على دفعات (تجنب حظر SMTP) + إحصاءات فتح.
- أتمتة: «انخفض سعر منتج بأمنياتك»، «عاد للمخزون»، «كوبون عيد ميلاد» للمسجلين.

## 24.4 الشحن الفعلي (G-15) + الدعم (G-30)

- نمط `IShippingProvider` (شبيه ببوابات الدفع): إنشاء بوليصة، رقم تتبع يُحفظ في الطلب، Webhook حالة التوصيل يحدّث حالة الطلب تلقائياً — تكاملات السوق: Bosta، Mylerz، Aramex.
- زر واتساب عائم (الرقم من `ContactInfo`) + قوالب رسائل جاهزة «استفسار عن المنتج X» من صفحة المنتج.

**معيار الإتمام**: تقرير شهري يُصدَّر بضغطة، وسلة متروكة تعود بكوبون تلقائي، وبوليصة شحن تُنشأ من شاشة الطلب.

---
---

# 25. 🚀 المرحلة 4: التوسع والذكاء (شهر 7+)

**الهدف**: منصة إقليمية ذكية — لا مجرد متجر.

## 25.1 تعدد العملات (G-17) واللغات (G-18)

- **العملات**: جدول `Currency` (رمز/سعر صرف/تنسيق) + تحديث أسعار دوري (API) — العرض يتحول، **التسوية بعملة الأساس** مع لقطة سعر الصرف في الطلب.
- **اللغات**: ترقية نظام الثنائية إلى N لغة — `ContentBlockTranslation` و`ProductTranslation` بدل أعمدة `Ar/En` الثابتة (هجرة بيانات محسوبة)، مع بقاء الكوكي والبنية الحالية كما هي مفهومياً.

## 25.2 PWA (G-19)

- Manifest + Service Worker: تثبيت على الموبايل، تصفح أوف لاين للكتالوج المكشوف، Push Notifications («طلبك شُحن!»، عروض) بموافقة صريحة.
- الـ API الجاهز (FTD.Api) يجعل بناء تطبيق Flutter/React Native لاحقاً مسألة واجهة فقط.

## 25.3 الذكاء الاصطناعي (G-20)

| القدرة | التنفيذ |
|---|---|
| **توصيات** «يُشترى معه عادة» | تحليل سلات الطلبات (Market Basket) — بسيط وفعال بلا نماذج |
| **بحث دلالي** يفهم «جاكيت شتوي رجالي رخيص» | Embeddings للمنتجات + فهرس متجهي |
| **مساعد تسوق** (Chat) | RAG فوق الكتالوج والسياسات — يجيب ويقترح ويضيف للسلة |
| **توليد أوصاف** المنتجات باللغتين | زر في نموذج المنتج يولّد مسودة من الاسم والمواصفات |
| **تنبؤ الطلب** | توصية إعادة تخزين بناء على سرعة البيع |

## 25.4 توسعات منصّاتية

- **RMA كامل (G-28)**: طلب إرجاع من حساب العميل → موافقة → استرداد عبر البوابة → إعادة مخزون.
- **الولاء (G-29)**: نقاط بالشراء تُستبدل خصماً — جدول `LoyaltyTransaction` + إعدادات معدل الكسب/الاستبدال من اللوحة.
- **Multi-Store/White-Label**: البنية الديناميكية (بلوكات/إعدادات/ثيم) تسمح بتشغيل أكثر من متجر بهوية مختلفة على نفس الكود — إضافة `StoreId` للكيانات المحورية عند الحاجة الفعلية فقط.

---
---

# 26. 💎 التحكم الكامل من لوحة الإدارة — الرؤية النهائية

هذا القسم هو الإجابة المباشرة على السؤال: **«كيف يصبح كل شيء في المتجر قابلاً للتحكم من صفحة الأدمن؟»** — من غير ما نفتح الكود أبداً بعد النشر.

## 26.1 أين نقف اليوم؟ (مصفوفة التحكم الحالية)

| العنصر | تحكم كامل ✅ | تحكم جزئي 🟡 | لا تحكم ❌ |
|---|---|---|---|
| نصوص الموقع (64 بلوك ثنائي) | ✅ | | |
| إظهار/ترتيب أقسام الرئيسية | ✅ | | |
| أقسام منتجات مخصصة | ✅ | | |
| صفحات بأقسام مرئية (Page Builder) | ✅ | | |
| القوائم (هيدر/فوتر) | ✅ | | |
| معلومات التواصل + إظهار كل حقل | ✅ | | |
| رسوم الشحن وحد المجانية | ✅ | | |
| اللون الأساسي + الشعار | ✅ | | |
| المنتجات/الفئات/الماركات/المواصفات | ✅ | | |
| الطلبات وحالاتها | ✅ | | |
| باقي ألوان الثيم (ثانوي/خلفيات/نصوص) | | 🟡 (CSS فقط) | |
| الخطوط وأحجامها | | | ❌ |
| المسافات/الاستدارات/الظلال | | | ❌ |
| تخطيط صفحة المنتج/الفئة | | | ❌ |
| نصوص أزرار النظام (سلة/دفع) | | 🟡 (بعضها data-ar/en ثابتة) | |
| البريد وقوالبه | | | ❌ |
| السياسات/صفحات جديدة كلياً | | 🟡 (builder للرئيسية) | |
| تشغيل/إيقاف ميزات (Feature Flags) | | 🟡 (أقسام الرئيسية فقط) | |

**الخلاصة**: المحتوى والهيكل تحت السيطرة الكاملة بالفعل — الفجوة الأساسية في **الثيم البصري العميق** و**تخطيطات الصفحات الداخلية** و**أعلام الميزات**.

## 26.2 الخطة أ: محرر الثيم الكامل (Theme Editor)

### التصميم
توسيع `SiteSettings` بمجموعة مفاتيح `theme.*` تُحقن كلها في `:root`:

```csharp
// ThemeService.BuildCssVariables() — يُخرج <style> واحد في _Layout:
:root {
    --primary:        @Setting("theme.color.primary",   "#0d6efd");
    --secondary:      @Setting("theme.color.secondary", "#6c757d");
    --accent:         @Setting("theme.color.accent",    "#ffc107");
    --bg:             @Setting("theme.color.bg",        "#f8f9fa");
    --surface:        @Setting("theme.color.surface",   "#ffffff");
    --text:           @Setting("theme.color.text",      "#1a1a2e");
    --text-muted:     @Setting("theme.color.muted",     "#6b7280");
    --radius:         @Setting("theme.radius",          "14px");
    --shadow-level:   @Setting("theme.shadow",          "medium");
    --font-ar:        @Setting("theme.font.ar",         "Cairo");
    --font-en:        @Setting("theme.font.en",         "Inter");
    --font-scale:     @Setting("theme.font.scale",      "1.0");
    --container-max:  @Setting("theme.container",       "1280px");
    --section-gap:    @Setting("theme.section.gap",     "72px");
}
```

### شاشة اللوحة المقترحة `/admin/theme`
- **الألوان**: 7 منتقيات لون + معاينة تباين تلقائية (تحذير WCAG لو النص لا يُقرأ).
- **الخطوط**: قائمة خطوط عربية (Cairo/Tajawal/Almarai/IBM Plex Arabic) وإنجليزية (Google Fonts) + مقياس حجم عام.
- **الأشكال**: شريط تمرير للاستدارة (حاد 0 ↔ دائري 24px)، مستوى الظلال (مسطح/خفيف/عميق).
- **الوضع الليلي**: مجموعة `theme.dark.*` موازية + مفتاح `theme.dark.enabled` → زر قمر بالهيدر.
- **قوالب جاهزة (Presets)**: «أنيق داكن»، «متجر أزياء»، «إلكترونيات حيوي» — زر واحد يملأ كل المفاتيح.
- **معاينة حية**: iframe للرئيسية يتحدث لحظياً قبل الحفظ + زر «رجوع للافتراضي» لكل مفتاح.

**الجُهد**: صغير-متوسط. **الأثر**: تغيير هوية المتجر بالكامل في دقائق بلا مطوّر.

## 26.3 الخطة ب: محرر التخطيطات (Layout Control)

تعميم فكرة `homepage.sections.order` على كل الصفحات القالبية:

| الصفحة | مفاتيح التحكم المقترحة |
|---|---|
| صفحة المنتج | `pdp.layout` (صور يمين/يسار/معرض علوي)، `pdp.show.sku/brand/related/attributes/share`، `pdp.related.count` |
| صفحة الفئة | `plp.columns` (2/3/4)، `plp.filters.position` (جانبي/علوي/درج)، `plp.card.style` (3 أنماط بطاقة) |
| السلة/الدفع | `checkout.fields.province.visible/required`، `checkout.show.notes`، `cart.show.coupon` |
| الهيدر | `header.style` (شفاف/صلب/مزدوج)، `header.show.search/lang/phone` |
| الفوتر | `footer.columns` (2–4)، `footer.show.newsletter/social/payments` |

كل مفتاح = شرط Razor بسيط في الـ View — البنية الحالية (`GetOr` + السقوط الآمن) تستوعبها بلا أي تغيير معماري.

## 26.4 الخطة ج: أعلام الميزات (Feature Flags)

مع نمو الميزات (مراجعات، أمنيات، كوبونات، ولاء…)، كل ميزة تولد **بمفتاح تشغيل**:

```
features.reviews.enabled        → إظهار/إخفاء كل نظام المراجعات
features.wishlist.enabled       → القلوب والصفحة والعدّاد
features.coupons.enabled        → حقل الكوبون في السلة
features.newsletter.enabled     → النشرة في الفوتر
features.whatsapp.enabled       → الزر العائم
features.darkmode.enabled       → زر الوضع الليلي
features.guest.checkout         → السماح بالشراء بلا حساب
```

- شاشة `/admin/features`: قائمة مفاتيح بوصف عربي + Toggle — **إطلاق تدريجي وإطفاء فوري عند أي مشكلة** بدون نشر.
- في الكود: `if (!await _features.IsEnabledAsync("reviews")) return NotFound();` — خدمة واحدة فوق `SiteSettings` بكاش.

## 26.5 الخطة د: إدارة البريد من اللوحة

- شاشة `/admin/email`: إعدادات SMTP (مشفرة) + زر «إرسال بريد تجريبي».
- قوالب البريد كـ ContentBlocks خاصة (`email.order.confirmed.ar/en`…) بمتغيرات `{{OrderNumber}}`, `{{CustomerName}}`, `{{Total}}` — تُحرر بـ Quill وتُعاين قبل الحفظ.
- سجل إرسال (`EmailLog`): ماذا أُرسل لمن ومتى ونجح/فشل — تشخيص فوري لشكاوى «ماوصلنيش إيميل».

## 26.6 الخطة هـ: مركز التحكم الموحد — الشكل النهائي للّوحة

إعادة تنظيم الشريط الجانبي عند اكتمال المراحل:

```
📊 نظرة عامة        → داشبورد + تقارير + تنبيهات (مخزون/سلات متروكة)
🛍️ الكتالوج          → منتجات + متغيرات + فئات + ماركات + مواصفات + مراجعات
📦 المبيعات          → طلبات + مرتجعات + سلات متروكة + عملاء
💰 التسويق           → كوبونات + نشرة + حملات + نقاط ولاء + بكسلات التتبع
🎨 المظهر            → الثيم + التخطيطات + منشئ الصفحات + القوائم + الوسائط
📝 المحتوى           → البلوكات + الصفحات الحرة + الترجمات
⚙️ الإعدادات          → عام + شحن ومناطق + دفع + بريد + أعلام الميزات
🔐 النظام            → المستخدمون والأدوار + سجل النشاط + نسخ احتياطي + حالة النظام
```

### لمسات «أقوى لوحة تحكم»
- **بحث شامل** أعلى اللوحة (Cmd+K): يقفز لأي منتج/طلب/إعداد/شاشة.
- **مكتبة وسائط مركزية**: كل المرفوعات في شاشة واحدة (بحث/حذف/إعادة استخدام) بدل الرفع المبعثر.
- **مسودة/نشر للمحتوى الحساس**: تعديل الرئيسية كمسودة → معاينة → نشر بضغطة (+ تراجع لآخر نسخة).
- **استيراد/تصدير**: منتجات CSV/Excel دخولاً وخروجاً — إدخال 500 منتج في دقيقة.
- **نسخ احتياطي من اللوحة**: زر تصدير قاعدة + مرفوعات لملف واحد مؤرشف.
- **صحة النظام**: شاشة تشخيص (اتصال DB/SMTP/بوابة الدفع + مساحة القرص + آخر هجرة).

## 26.7 خلاصة الرؤية

> **المبدأ الحاكم**: أي قرار «منتجي» (نص، لون، ترتيب، تشغيل ميزة، سعر شحن، قالب بريد) قراره في اللوحة؛ وأي قرار «هندسي» (مخطط بيانات، أمان، تكاملات) قراره في الكود بهجرة ومراجعة. المشروع بُني من يومه الأول على هذا المبدأ (بلوكات/إعدادات/builder) — والخطط أعلاه تكمل الطريق حتى آخره: **متجر يديره صاحبه بنسبة 100% بلا مطوّر في التشغيل اليومي.**

---
---

# 27. 📐 معايير التطوير — دستور المساهمة في المشروع

كل سطر كود جديد يجب أن يحترم القواعد التالية — هذه ليست تفضيلات بل **عقد** يحفظ جودة المشروع مع نموه.

## 27.1 قواعد المعمارية (غير قابلة للتفاوض)

1. **اتجاه الاعتماديات مقدس**: `Web/Api → Application → Domain` و`Infrastructure → Application` — أي `using FTD.Infrastructure` داخل متحكم = مرفوض.
2. **لا EF Core خارج Application/Infrastructure**: المتحكم لا يعرف `DbContext` ولا `IQueryable`.
3. **لا كيان دومين في View/Response**: DTOs فقط تعبر الحدود.
4. **منطق عمل جديد = خدمة (أو توسيع خدمة) + واجهة**: المتحكم منسّق رفيع فقط (استقبال → تحقق → استدعاء خدمة → نتيجة).
5. **كل تغيير في `AppDbContext` = هجرة فورية** بنفس الـ PR — بلا استثناء:
   ```bash
   dotnet ef migrations add DescriptiveName --project FTD.Infrastructure --startup-project FTD.Web
   ```
6. **لا تعديل على هجرة سبق تطبيقها** — أخطأت؟ هجرة تصحيحية جديدة.

## 27.2 قواعد الأمان لكل ميزة جديدة

- كل `[HttpPost]` جديد → `[ValidateAntiForgeryToken]` (أو Bearer في الـ API).
- كل DTO إدخال جديد → DataAnnotations كاملة (Required/StringLength/Range) + فحص `ModelState` في المتحكم.
- كل HTML يُخزن → عبر `HtmlSanitizer` حصراً.
- كل رفع ملف → عبر `ImageUploadHelper` حصراً (لا `CopyToAsync` يدوي).
- كل قيمة مالية/كمية → تُحسب وتُتحقق خادمياً، مدخل العميل مرجع فقط.
- كل شاشة إدارية جديدة → `[Authorize(Roles = ...)]` صريح.

## 27.3 قواعد الأداء

- استعلام قراءة؟ → `AsNoTracking()` + إسقاط `Select` إلى DTO.
- قائمة؟ → ترقيم `Skip/Take` + `CountAsync` — ممنوع `ToList` بلا حد.
- حلقة فيها `await` على DB؟ → أعد التفكير: اجلب دفعة واحدة بقاموس.
- بيانات شبه ثابتة تُقرأ كل طلب؟ → مرشح للكاش (مع إبطال عند الكتابة).

## 27.4 قواعد المحتوى واللغة

- **أي نص جديد يراه الزائر** → ContentBlock بمفتاحي `.ar/.en` (أو `data-ar/data-en` للأزرار الثابتة) — لا نص عربي/إنجليزي حرفي في الكود.
- **أي سلوك قابل للتغيير** → SiteSetting بقيمة افتراضية آمنة وقراءة بـ `GetOr`.
- كل View جديد يُختبر بالاتجاهين RTL وEN قبل الدمج.

## 27.5 سير عمل Git المعتمد

```
فرع الميزة: feature/coupons  ←  من genspark_ai_developer
Commit format: type(scope): description
  feat(coupons): add coupon validation service
  fix(cart): clamp negative quantities
  refactor(orders): batch product fetch in checkout
قبل الـ PR: build نظيف 0/0 + هجرات مضمّنة + تحديث الوثائق (هذا الملف/الـ docs)
```

## 27.6 «تعريف الإنجاز» (Definition of Done) لأي ميزة

- [ ] الكود يحترم الطبقات والاتجاهات
- [ ] هجرة مضمّنة (لو مسّت القاعدة) واختُبر Up/Down
- [ ] DataAnnotations + antiforgery + authorize حسب السياق
- [ ] `AsNoTracking` وإسقاطات على القراءات الجديدة
- [ ] النصوص عبر بلوكات ثنائية + اختبار RTL/LTR
- [ ] `dotnet build` = 0 أخطاء / 0 تحذيرات
- [ ] تحكم اللوحة موجود (إعداد/شاشة) لو القرار «منتجي»
- [ ] الوثائق محدثة + PR بوصف وافٍ

---
---

# 28. الملاحق

## ملحق أ — وصفات كود جاهزة لميزات خارطة الطريق

### أ-1: خدمة الكوبونات (هيكل جاهز للتنفيذ — G-04)

```csharp
// Domain/Entities/Coupon.cs
public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;          // فهرس فريد
    public CouponType Type { get; set; }                      // Percentage | FixedAmount
    public decimal Value { get; set; }                        // 10 (%) أو 50 (جنيه)
    public decimal? MinCartTotal { get; set; }                // حد أدنى للتفعيل
    public int? MaxTotalUses { get; set; }                    // سقف كلي
    public int? MaxUsesPerCustomer { get; set; }              // سقف للعميل (بالبريد/الهاتف)
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int UsedCount { get; set; }
}

// Application/Services/CouponService.cs — جوهر التحقق:
public async Task<CouponResult> ValidateAsync(string code, decimal cartTotal, string? customerKey)
{
    var c = await _db.Coupons.AsNoTracking()
        .FirstOrDefaultAsync(x => x.Code == code.Trim().ToUpper() && x.IsActive);

    if (c is null)                                    return CouponResult.Fail("كود غير صحيح");
    var now = DateTime.UtcNow;
    if (c.StartsAt > now || c.ExpiresAt < now)        return CouponResult.Fail("الكود منتهي أو لم يبدأ");
    if (c.MaxTotalUses.HasValue && c.UsedCount >= c.MaxTotalUses) return CouponResult.Fail("استُنفد الكود");
    if (c.MinCartTotal.HasValue && cartTotal < c.MinCartTotal)
        return CouponResult.Fail($"الحد الأدنى للطلب {c.MinCartTotal:0} جنيه");
    if (customerKey is not null && c.MaxUsesPerCustomer.HasValue)
    {
        var used = await _db.CouponUsages.CountAsync(u => u.CouponId == c.Id && u.CustomerKey == customerKey);
        if (used >= c.MaxUsesPerCustomer)             return CouponResult.Fail("استخدمت هذا الكود من قبل");
    }

    var discount = c.Type == CouponType.Percentage
        ? Math.Round(cartTotal * c.Value / 100m, 2)
        : Math.Min(c.Value, cartTotal);               // خصم ثابت لا يتجاوز السلة
    return CouponResult.Ok(c.Id, discount);
}
// ملاحظة: زيادة UsedCount + تسجيل CouponUsage تتم داخل CreateOrderAsync (نفس الحفظة الذرّية)
```

### أ-2: خصم المخزون الآمن ضد التزامن (G-03)

```csharp
// 1) في AppDbContext:
builder.Entity<Product>().Property<byte[]>("RowVersion").IsRowVersion();
// + هجرة AddStockConcurrency

// 2) في CreateOrderAsync — منتجات متعقَّبة هنا (استثناء مبرر لقاعدة AsNoTracking):
var products = await _db.Products
    .Where(p => ids.Contains(p.Id) && p.IsActive)
    .ToDictionaryAsync(p => p.Id, ct);

foreach (var item in items)
{
    var p = products[item.ProductId];
    if (p.StockQuantity < item.Quantity)
        throw new InvalidOperationException($"المتاح من «{p.NameAr}» هو {p.StockQuantity} فقط");
    p.StockQuantity -= item.Quantity;
}

try { await _db.SaveChangesAsync(ct); }
catch (DbUpdateConcurrencyException)
{
    // عميلان اشتريا آخر قطعة في نفس اللحظة — الخاسر يعيد المحاولة أو يُبلَّغ
    throw new InvalidOperationException("تغيّر المخزون أثناء إتمام طلبك — أعد المحاولة");
}
```

### أ-3: هيكل بوابة الدفع Paymob (G-01)

```csharp
public class PaymobGateway : IPaymentGateway
{
    public string Name => "paymob";

    public async Task<PaymentInitResult> InitiateAsync(OrderDto order)
    {
        // 1) auth token  2) order registration  3) payment key
        // القيم من SiteSettings المشفرة: payment.paymob.api_key / integration_id / iframe_id
        var paymentKey = await GetPaymentKeyAsync(order);
        return PaymentInitResult.Redirect(
            $"https://accept.paymob.com/api/acceptance/iframes/{_iframeId}?payment_token={paymentKey}");
    }

    public Task<PaymentVerifyResult> VerifyCallbackAsync(HttpRequest req)
    {
        // HMAC-SHA512 على الحقول المرتبة أبجدياً — مقارنة بتوقيع hmac القادم
        // نجاح + المبلغ مطابق للطلب => تأكيد. أي شيء آخر => رفض وتسجيل
    }
}
// المسار: POST /payment/callback/{gateway} → يفك التوقيع → يحدّث PaymentTransaction
//         → ينقل الطلب من «بانتظار الدفع» إلى «مؤكد» → بريد للعميل
```

### أ-4: sitemap.xml ديناميكي (G-11)

```csharp
[Route("sitemap.xml")]
public async Task<IActionResult> Sitemap()
{
    var urls = new List<(string loc, DateTime mod)>
    {
        ($"{_base}/", DateTime.UtcNow), ($"{_base}/products", DateTime.UtcNow),
        ($"{_base}/about", DateTime.UtcNow), ($"{_base}/contact", DateTime.UtcNow)
    };
    urls.AddRange((await _products.GetAllSlugsAsync()).Select(s => ($"{_base}/product/{s.Slug}", s.UpdatedAt)));
    urls.AddRange((await _products.GetAllCategorySlugsAsync()).Select(s => ($"{_base}/category/{s}", DateTime.UtcNow)));

    var xml = new XDocument(new XElement(XName.Get("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9"),
        urls.Select(u => new XElement("url",
            new XElement("loc", u.loc),
            new XElement("lastmod", u.mod.ToString("yyyy-MM-dd"))))));
    return Content(xml.ToString(), "application/xml");
}
```

### أ-5: كاش البلوكات بإبطال (G-23)

```csharp
public class CachedContentService : IContentService   // Decorator فوق ContentService
{
    private readonly IContentService _inner;
    private readonly IMemoryCache _cache;
    private const string BlocksKey = "content:blocks";

    public async Task<Dictionary<string,string>> GetBlocksAsync() =>
        await _cache.GetOrCreateAsync(BlocksKey, async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return await _inner.GetBlocksAsync();
        });

    public async Task UpsertBlockAsync(ContentBlockDto dto)
    {
        await _inner.UpsertBlockAsync(dto);
        _cache.Remove(BlocksKey);                     // إبطال فوري — الحفظ يظهر لحظياً
    }
    // التسجيل: builder.Services.Decorate<IContentService, CachedContentService>(); (Scrutor)
}
```

## ملحق ب — المرجع السريع للمسارات (Route Map)

### الواجهة العامة
```
GET  /                          الرئيسية الديناميكية
GET  /products                  كل المنتجات
GET  /category/{slug}           فئة + فلاتر وجهية
GET  /brand/{slug}              صفحة ماركة
GET  /product/{slug}            تفاصيل منتج
GET  /search?q=                 بحث
GET  /cart                      السلة
POST /cart/add|update|remove|clear     (AJAX + antiforgery)
GET|POST /checkout              الدفع (COD)
GET  /order/confirmation/{number}
GET|POST /order/track           تتبع بالرقم
GET  /about  /contact  /privacy
POST /contact                   رسالة تواصل
GET  /set-language?lang=ar|en
```

### لوحة التحكم (Admin role)
```
/admin                          داشبورد
/admin/products  [+ /create /edit/{id} /duplicate/{id} /toggle-*]
/admin/categories   /admin/brands
/admin/orders  [+ /details/{id} /update-status]
/admin/content                  بلوكات + إعدادات + builder
/admin/attributes  [+ قيم + أقسام الرئيسية المخصصة]
/admin/navigation
/admin/messages  [+ contact-info]
/admin/login  /admin/logout
```

## ملحق ج — أمثلة طلبات API جاهزة (curl)

```bash
BASE=https://your-domain.com

# قائمة منتجات
curl "$BASE/api/Products?page=1&categorySlug=electronics"

# تفاصيل منتج
curl "$BASE/api/Products/iphone-15-pro"

# إنشاء طلب
curl -X POST "$BASE/api/Orders/checkout" -H "Content-Type: application/json" -d '{
  "customerName": "أحمد محمد", "phone": "01012345678",
  "address": "15 شارع التحرير", "city": "القاهرة",
  "items": [ { "productId": 3, "quantity": 2 } ]
}'

# دخول أدمن → توكن
TOKEN=$(curl -s -X POST "$BASE/api/Auth/login" -H "Content-Type: application/json" \
  -d '{"email":"admin@site.com","password":"***"}' | jq -r .token)

# إحصائيات + تغيير حالة طلب
curl "$BASE/api/Admin/dashboard" -H "Authorization: Bearer $TOKEN"
curl -X PUT "$BASE/api/Admin/orders/12/status" -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" -d '{"statusId": 3}'
```

## ملحق د — استكشاف الأخطاء الشائعة (Troubleshooting)

| العرض | السبب الأرجح | الحل |
|---|---|---|
| `Failed to resolve libhostfxr.so` عند `dotnet ef` | `DOTNET_ROOT` غير مضبوط | `export DOTNET_ROOT="$HOME/.dotnet"` |
| 400 على POST سلة من JS | توكن antiforgery غائب/باسم خطأ | الهيدر `RequestVerificationToken` + الحقل موجود في `_Layout` |
| صفحة أولى بلغة خاطئة تومض | قراءة اللغة بـ JS بدل الكوكي | تأكد أن `dir/lang` يُضبطان خادمياً من `ftd_lang` |
| فشل حذف منتج «مرتبط بطلبات» | سلوك مقصود (`Restrict`) | عطّله (`IsActive=false`) بدل حذفه — التاريخ محفوظ |
| الهجرة تفشل عند الإقلاع والتطبيق لا يعمل | سلوك مقصود (I-22) | راجع اللوج `LogCritical` وصحّح القاعدة ثم أعد التشغيل |
| رفع صورة يُرفض | امتداد خارج القائمة أو > 5MB | jpg/jpeg/png/gif/webp فقط، صغّر الحجم |
| إعداد عدّلته لا يظهر | (بعد إضافة الكاش مستقبلاً) إبطال ناقص | الحفظ عبر الخدمة يُبطل الكاش — لا تعدّل الجدول يدوياً |

## ملحق هـ — قاموس المصطلحات

| المصطلح | المعنى في سياق المشروع |
|---|---|
| **Slug** | معرف نصي في الرابط (`iphone-15-pro`) — فريد بفهرس قاعدة |
| **Snapshot (لقطة)** | نسخ اسم/سعر المنتج داخل بند الطلب لحظة الشراء — الفواتير لا تتغير بأثر رجعي |
| **Faceted Search** | فلترة متعددة الأبعاد بمواصفات ديناميكية مع عدّادات |
| **ContentBlock** | نص ثنائي اللغة بمفتاح، يُدار من اللوحة |
| **SiteSetting** | قيمة سلوكية بمفتاح (bool/رقم/نص) تغيّر عمل الموقع فوراً |
| **PageSection** | قسم مرئي من منشئ الصفحات، محتواه JSON |
| **Guest Checkout** | شراء وتتبع بلا حساب — برقم الطلب فقط |
| **AsNoTracking** | قراءة EF بلا تعقب تغييرات — أسرع وأخف للعرض |
| **Restrict FK** | منع حذف صف مرتبط ببيانات تاريخية على مستوى القاعدة |
| **HasData Seeding** | بذر بيانات افتراضية داخل الهجرات — متجر جاهز من أول تشغيل |
| **Feature Flag** | مفتاح تشغيل/إطفاء ميزة من اللوحة بلا نشر |

---

## كلمة أخيرة

Uni-Shop اليوم: **أساس هندسي صلب** — معمارية نظيفة، أمان مطبّق بطبقات، أداء مدروس، ونظام محتوى يجعل صاحب المتجر متحكماً في كل نص وقسم وقائمة من لوحته. 

الطريق لأقوى متجر ممكن مرسوم أعلاه بدقة: **4 مراحل، 30 فجوة مصنّفة، ووصفات كود جاهزة** — تبدأ بالدفع والمخزون والحسابات (بدونها لا تجارة حقيقية)، وتنتهي بالذكاء الاصطناعي والتوسع الإقليمي، وعلى امتداد الطريق كله قاعدة واحدة لا تتغير: **كل قرار منتجي ينتهي زراً أو حقلاً في لوحة التحكم.**

*آخر تحديث: 2026-07-13 — يُحدَّث هذا الدليل مع كل مرحلة تكتمل.*

---
---

## ملحق و — مرجع الكيانات حقلاً بحقل (Data Dictionary الكامل)

المرجع النهائي لكل عمود في قاعدة البيانات — للرجوع أثناء أي تطوير.

### و-1: Product

| الحقل | النوع | القيود | ملاحظات |
|---|---|---|---|
| `Id` | int | PK, Identity | |
| `NameAr` | nvarchar(200) | Required | اسم العرض بالعربية |
| `NameEn` | nvarchar(200) | Required | اسم العرض بالإنجليزية |
| `Slug` | nvarchar(250) | Required, **Unique Index** | معرف الرابط — يُفحص التفرد قبل الحفظ أيضاً |
| `DescriptionAr` | nvarchar(max) | — | HTML من Quill، مُعقَّم |
| `DescriptionEn` | nvarchar(max) | — | HTML من Quill، مُعقَّم |
| `Sku` | nvarchar(100) | — | كود داخلي، يدخل في البحث |
| `Price` | decimal(18,2) | Range 0–1M | السعر الحالي |
| `OldPrice` | decimal(18,2)? | Range 0–1M | لو موجود وأكبر: Badge خصم محسوب |
| `StockQuantity` | int | ≥ 0 | يُعرض حالياً؛ خصمه التلقائي في المرحلة 1 |
| `IsActive` | bit | default 1 | المعطّل يختفي من الواجهة كلياً |
| `IsFeatured` | bit | default 0 | يظهر في قسم «المميزة» بالرئيسية |
| `CategoryId` | int | FK → Categories (Restrict) | إلزامي |
| `BrandId` | int? | FK → Brands (Restrict) | اختياري |
| `MainImageUrl` | nvarchar(500) | — | الصورة الرئيسية للبطاقات |
| `CreatedAt` / `UpdatedAt` | datetime2 | — | ترتيب «الأحدث» يعتمد CreatedAt |

### و-2: Category / Brand

| الحقل | Category | Brand |
|---|---|---|
| `Id`, `NameAr/En`, `Slug` (فريد) | ✅ | ✅ |
| `ImageUrl` | صورة البطاقة بالرئيسية | الشعار (شريط الماركات) |
| `SortOrder` | ترتيب العرض | — |
| `IsActive` | ✅ | ✅ |
| علاقات | 1→* Products, *→* Attributes (عبر CategoryAttribute) | 1→* Products |

### و-3: SalesOrder

| الحقل | النوع | ملاحظات |
|---|---|---|
| `Id` | int PK | |
| `OrderNumber` | nvarchar(50) **Unique** | `FTD{yyyyMMddHHmmss}{###}` — مفتاح التتبع العام |
| `CustomerName` | nvarchar(150) | |
| `Phone` | nvarchar(20) | وسيلة التواصل الأساسية (COD) |
| `Email` | nvarchar(200)? | اختياري |
| `Address` | nvarchar(300) | |
| `City` | nvarchar(100) | |
| `Province` | nvarchar(100)? | ستربط بـ ShippingZone بالمرحلة 1 |
| `Notes` | nvarchar(1000)? | ملاحظات العميل |
| `SubTotal` | decimal(18,2) | مجموع البنود — محسوب خادمياً |
| `ShippingFee` | decimal(18,2) | لقطة رسوم الشحن وقت الطلب |
| `TotalAmount` | decimal(18,2) | SubTotal + ShippingFee (− خصم مستقبلاً) |
| `PaymentMethod` | nvarchar(50) | "COD" حالياً — سيتوسع |
| `OrderStatusId` | int FK → OrderStatuses | الحالة الحية |
| `CreatedAt` | datetime2 | |

### و-4: SalesOrderDetail — جدول اللقطات

| الحقل | النوع | لماذا لقطة؟ |
|---|---|---|
| `Id`, `SalesOrderId` (FK Cascade من الطلب) | | حذف طلب (نادر) يمسح بنوده |
| `ProductId` | int FK → Products **(Restrict!)** | المرجع الحي — محمي من الحذف |
| `ProductNameAr` / `ProductNameEn` | nvarchar(200) | الاسم قد يتغير — الفاتورة لا |
| `UnitPrice` | decimal(18,2) | السعر قد يتغير — الفاتورة لا |
| `Quantity` | int ≥ 1 | حارس `Math.Max(1,…)` قبل الوصول هنا |

### و-5: نظام المواصفات (4 جداول)

| الجدول | الحقول | الدور |
|---|---|---|
| `ProductAttributes` | Id, NameAr, NameEn, SortOrder | «اللون»، «المقاس»، «الرام» |
| `AttributeValues` | Id, ProductAttributeId (FK), ValueAr, ValueEn, SortOrder | «أحمر»، «XL»، «8GB» |
| `CategoryAttributes` | CategoryId + ProductAttributeId (مفتاح مركب) | أي مواصفات تخص أي فئة |
| `ProductAttributeValues` | ProductId + AttributeValueId (مركب، Restrict) | قيم كل منتج فعلياً |

### و-6: كيانات المحتوى

| الجدول | الحقول الأساسية | ملاحظات |
|---|---|---|
| `ContentBlocks` | Key (**Unique**), Value | 64 صفاً مزروعاً — النمط `xxx.ar` / `xxx.en` |
| `SiteSettings` | Key (**Unique**), Value | 23 صفاً مزروعاً — bool/رقم/CSV/hex كنص |
| `PageSections` | PageKey, SectionType, ContentJson, SortOrder, IsVisible | منشئ الصفحات — 7 أنواع |
| `NavigationLinks` | TextAr, TextEn, Url, Location(header/footer), SortOrder, NewTab | القوائم |
| `HomepageSections` | TitleAr/En, ProductIdsCsv, ImageUrl, SortOrder, IsVisible | أقسام منتجات مخصصة |
| `OrderStatuses` | NameAr/En, ColorHex, SortOrder | 7 صفوف — قابلة للتوسيع |
| `ContactInfo` | صف واحد: هاتفان، بريد، عنوانان، 4 سوشيال، ساعات، خريطة + **Show*** لكل حقل | |
| `ContactMessages` | Name(100), Email(100), Phone(20), Message, IsRead, CreatedAt | |

---

## ملحق ز — سيناريوهات تشغيلية: يوم في حياة مدير المتجر

دليل عملي خطوة-بخطوة للمهام اليومية الحقيقية — يصلح تدريباً لأي موظف جديد.

### ز-1: إضافة منتج جديد كامل (≈ 4 دقائق)

1. **اللوحة → المنتجات → إضافة منتج**.
2. الاسم عربي «تيشيرت قطن رجالي» + إنجليزي — الـ slug يُقترح تلقائياً (`cotton-mens-tshirt`)، عدّله لو أردت.
3. الفئة «ملابس رجالي» → **لحظتها** تظهر مواصفات الفئة (مقاس/لون/خامة) — علّم القيم المتوفرة.
4. السعر 350، القديم 450 → Badge «−22%» سيظهر تلقائياً. المخزون 40. SKU اختياري.
5. الوصف بالمحررين (نقاط قصيرة تبيع أفضل من فقرات).
6. اسحب 3–5 صور دفعة واحدة → علّم الرئيسية → رتّب بالسحب.
7. «نشط» ✓ + «مميز» لو تريده بالرئيسية → **حفظ**. افتح `/product/cotton-mens-tshirt` للتأكد بالاتجاهين.

> 💡 **منتج شبيه موجود؟** زر «نسخ» من الجدول يستنسخه كاملاً — عدّل الاسم والصور فقط.

### ز-2: معالجة طلبات الصباح (≈ دقيقة/طلب)

1. **اللوحة → الطلبات** → فلتر «جديد».
2. افتح الطلب: راجع البنود والعنوان والهاتف — اتصل للتأكيد (COD).
3. أكّد؟ → الحالة «مؤكد». لا يرد بعد محاولتين؟ → «ملغي» + ملاحظة داخلية.
4. جهّزت الشحنة؟ → «قيد التجهيز» ثم «شُحن» عند التسليم للمندوب.
5. العميل يتابع بنفسه عبر `/order/track` — كل تغيير حالة يظهر له فوراً (ومع المرحلة 1: يصله بريد تلقائي).

### ز-3: إطلاق حملة «عروض نهاية الأسبوع» (≈ 10 دقائق، صفر كود)

1. **المنتجات**: ضع `OldPrice` للمنتجات المشاركة (خصومات مرئية).
2. **المواصفات والأقسام → أقسام الرئيسية → إضافة قسم**: «🔥 عروض نهاية الأسبوع» ثنائي اللغة + اختر المنتجات بالترتيب + بانر.
3. **المحتوى → الإعدادات**: اسحب القسم الجديد لأعلى الترتيب.
4. **المحتوى → البلوكات**: عدّل `hero.title` مؤقتاً («خصومات حتى 40% هذا الأسبوع»).
5. انتهى العرض الأحد ليلاً؟ Toggle إخفاء القسم + أرجع الهيرو — **ثوانٍ للإطفاء**.

### ز-4: تغيير هوية الموقع لموسم جديد

1. **المحتوى → الإعدادات**: `site.primary.color` → اللون الموسمي (ذهبي لرمضان مثلاً) — الموقع كله يتلون.
2. الشعار الموسمي من إعداد الشعار.
3. بلوكات الهيرو والإحصائيات بنصوص الموسم.
4. منشئ الصفحات: قسم `cta` موسمي («تخفيضات العيد بدأت») في الرئيسية.
5. (بعد محرر الثيم §26.2: Preset موسمي كامل بضغطة واحدة.)

### ز-5: الرد على رسائل العملاء

1. Badge أحمر بالشريط الجانبي = رسائل جديدة → **الرسائل**.
2. فتح الرسالة يعلّمها مقروءة تلقائياً — رد عبر الهاتف/البريد الظاهرين.
3. سؤال متكرر؟ أضف إجابته كقسم `text` في صفحة عبر منشئ الصفحات — وفّر رسائل قادمة.

---

## ملحق ح — وصفات كود إضافية (تكملة الملحق أ)

### ح-1: قائمة الأمنيات (G-07)

```csharp
// Domain:
public class WishlistItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;   // FK → AspNetUsers
    public int ProductId { get; set; }                   // FK → Products (Cascade هنا مقبول)
    public DateTime CreatedAt { get; set; }
    // فهرس فريد مركب (UserId, ProductId) — لا تكرار
}

// Application — WishlistService:
public async Task<bool> ToggleAsync(string userId, int productId)
{
    var existing = await _db.WishlistItems
        .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
    if (existing is not null) { _db.WishlistItems.Remove(existing); await _db.SaveChangesAsync(); return false; }

    if (!await _db.Products.AnyAsync(p => p.Id == productId && p.IsActive))
        throw new InvalidOperationException("منتج غير متاح");
    _db.WishlistItems.Add(new() { UserId = userId, ProductId = productId, CreatedAt = DateTime.UtcNow });
    await _db.SaveChangesAsync();
    return true;    // true = أُضيف (قلب ممتلئ)، false = أُزيل
}
// للزائر: localStorage['wishlist'] = [ids] — يُدمج عبر MergeAsync عند تسجيل الدخول
```

### ح-2: المراجعات مع الشراء الموثق (G-06)

```csharp
public async Task SubmitAsync(ReviewCreateDto dto, string? userId)
{
    // شارة «مشترٍ موثّق»: له طلب مُسلَّم يحوي هذا المنتج
    var verified = userId != null && await _db.SalesOrders
        .Where(o => o.UserId == userId && o.OrderStatus.NameEn == "Delivered")
        .AnyAsync(o => o.Details.Any(d => d.ProductId == dto.ProductId));

    _db.ProductReviews.Add(new ProductReview
    {
        ProductId = dto.ProductId,
        UserId = userId,
        GuestName = userId is null ? dto.GuestName : null,
        Rating = Math.Clamp(dto.Rating, 1, 5),            // حارس النطاق
        Title = dto.Title, Body = dto.Body,
        IsVerifiedPurchase = verified,
        IsApproved = false,                                // طابور اعتماد إجباري
        CreatedAt = DateTime.UtcNow
    });
    await _db.SaveChangesAsync();
}
// العرض العام: IsApproved فقط + المتوسط بإسقاط:
// .Select(g => new { Avg = g.Average(r => r.Rating), Count = g.Count() })
```

### ح-3: سجل النشاط الإداري بـ EF Interceptor (G-22)

```csharp
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    {
        var ctx = e.Context!;
        var userId = _currentUser.UserId;    // من IHttpContextAccessor مغلف

        foreach (var entry in ctx.ChangeTracker.Entries()
                 .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                          && x.Entity is not AuditLog))    // لا تسجيل ذاتي لانهائي
        {
            ctx.Set<AuditLog>().Add(new AuditLog
            {
                UserId     = userId,
                EntityName = entry.Entity.GetType().Name,
                Action     = entry.State.ToString(),
                OldValues  = entry.State == EntityState.Added   ? null
                           : JsonSerializer.Serialize(entry.OriginalValues.ToObject()),
                NewValues  = entry.State == EntityState.Deleted ? null
                           : JsonSerializer.Serialize(entry.CurrentValues.ToObject()),
                Timestamp  = DateTime.UtcNow
            });
        }
        return base.SavingChangesAsync(e, result, ct);
    }
}
// التسجيل: options.AddInterceptors(new AuditSaveChangesInterceptor(...))
// شاشة اللوحة: جدول بفلترة (المستخدم/الكيان/الفترة) + Diff ملوّن للقيم
```

### ح-4: Dockerfile + docker-compose (G-25)

```dockerfile
# Dockerfile — متعدد المراحل
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY *.sln ./
COPY FTD.Domain/*.csproj FTD.Domain/
COPY FTD.Application/*.csproj FTD.Application/
COPY FTD.Infrastructure/*.csproj FTD.Infrastructure/
COPY FTD.Web/*.csproj FTD.Web/
RUN dotnet restore FTD.Web/FTD.Web.csproj
COPY . .
RUN dotnet publish FTD.Web/FTD.Web.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
VOLUME /app/wwwroot/uploads
EXPOSE 8080
ENTRYPOINT ["dotnet", "FTD.Web.dll"]
```

```yaml
# docker-compose.yml
services:
  web:
    build: .
    ports: ["8080:8080"]
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=UniShop;User=sa;Password=${SA_PASSWORD};TrustServerCertificate=True
    volumes: [ uploads:/app/wwwroot/uploads ]
    depends_on: [ db ]
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment: { ACCEPT_EULA: "Y", SA_PASSWORD: "${SA_PASSWORD}" }
    volumes: [ mssql:/var/opt/mssql ]
volumes: { uploads: {}, mssql: {} }
```

### ح-5: GitHub Actions CI (G-25)

```yaml
# .github/workflows/ci.yml
name: CI
on:
  push: { branches: [master, genspark_ai_developer] }
  pull_request: { branches: [master] }
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.0.x' }
      - run: dotnet restore
      - run: dotnet build --no-restore -c Release /warnaserror   # التحذير = فشل، حماية معيار 0/0
      - run: dotnet test --no-build -c Release                    # عند وجود FTD.Tests
```

### ح-6: معالجة الصور برفع WebP (G-26)

```csharp
// ترقية ImageUploadHelper بـ SixLabors.ImageSharp:
public static async Task<ImageSaveResult> SaveOptimizedAsync(IFormFile file, IWebHostEnvironment env, string folder)
{
    ValidateOrThrow(file);                                   // نفس فحوص الامتداد/الحجم الحالية
    using var image = await Image.LoadAsync(file.OpenReadStream());
    var name = Guid.NewGuid().ToString("N");
    var dir = Path.Combine(env.WebRootPath, "uploads", folder);
    Directory.CreateDirectory(dir);

    var sizes = new Dictionary<string, int> { ["thumb"] = 300, ["medium"] = 800, ["full"] = 1600 };
    foreach (var (label, width) in sizes)
    {
        using var clone = image.Clone(x => x.Resize(new ResizeOptions
            { Mode = ResizeMode.Max, Size = new Size(width, 0) }));
        await clone.SaveAsWebpAsync(Path.Combine(dir, $"{name}-{label}.webp"),
            new WebpEncoder { Quality = 82 });
    }
    return new ImageSaveResult($"/uploads/{folder}/{name}");   // الواجهة تبني -thumb/-medium/-full
}
// في البطاقات: <img src="...-thumb.webp" loading="lazy"> — توفير 60–80% من الحجم
```

---
---
