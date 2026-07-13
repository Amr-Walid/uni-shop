using FTD.Domain.Entities;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FTD.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>, IAppDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Brand> Brands => Set<Brand>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
        public DbSet<AttributeValue> AttributeValues => Set<AttributeValue>();
        public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
        public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderDetail> SalesOrderDetails => Set<SalesOrderDetail>();
        public DbSet<ContentBlock> ContentBlocks => Set<ContentBlock>();
        public DbSet<ContentPage> ContentPages => Set<ContentPage>();
        public DbSet<PageSection> PageSections => Set<PageSection>();
        public DbSet<NavigationItem> NavigationItems => Set<NavigationItem>();
        public DbSet<ContactInfo> ContactInfos => Set<ContactInfo>();
        public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unique constraints
            builder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();
            builder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();
            builder.Entity<ContentPage>().HasIndex(p => p.Slug).IsUnique();
            builder.Entity<ContentBlock>().HasIndex(b => b.Key).IsUnique();
            builder.Entity<SiteSetting>().HasIndex(s => s.Key).IsUnique();

            // Brand FK
            builder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.SetNull);

            // Fix cascade delete cycles - SQL Server does not allow multiple cascade paths
            builder.Entity<ProductAttributeValue>()
                .HasOne(av => av.Attribute)
                .WithMany(a => a.ProductValues)
                .HasForeignKey(av => av.AttributeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductAttributeValue>()
                .HasOne(av => av.AttributeValue)
                .WithMany(a => a.ProductValues)
                .HasForeignKey(av => av.AttributeValueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductAttributeValue>()
                .HasOne(av => av.Product)
                .WithMany(p => p.AttributeValues)
                .HasForeignKey(av => av.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Protect order history: a Product that was ever sold must never
            // cascade-delete its SalesOrderDetail rows (financial snapshots).
            // The service layer soft-deletes such products; the schema now
            // enforces the same rule as defense-in-depth (was Cascade by convention).
            builder.Entity<SalesOrderDetail>()
                .HasOne(d => d.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing navigation
            builder.Entity<NavigationItem>()
                .HasOne(n => n.Parent)
                .WithMany(n => n.Children)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── SEED: Order Statuses ──────────────────────────────────────────
            builder.Entity<OrderStatus>().HasData(
                new OrderStatus { Id = 1, NameAr = "جديد", NameEn = "New", ColorHex = "#1A6BFF", Icon = "🆕", SortOrder = 1 },
                new OrderStatus { Id = 2, NameAr = "مؤكد", NameEn = "Confirmed", ColorHex = "#0E4FCC", Icon = "✅", SortOrder = 2 },
                new OrderStatus { Id = 3, NameAr = "في انتظار الشحن", NameEn = "Pending Shipment", ColorHex = "#FF9500", Icon = "📦", SortOrder = 3 },
                new OrderStatus { Id = 4, NameAr = "مع شركة الشحن", NameEn = "With Courier", ColorHex = "#FF6B35", Icon = "🚚", SortOrder = 4 },
                new OrderStatus { Id = 5, NameAr = "تم التسليم", NameEn = "Delivered", ColorHex = "#00C48C", Icon = "🎉", SortOrder = 5 },
                new OrderStatus { Id = 6, NameAr = "مرتجع", NameEn = "Returned", ColorHex = "#FF3B30", Icon = "↩️", SortOrder = 6 },
                new OrderStatus { Id = 7, NameAr = "ملغي", NameEn = "Cancelled", ColorHex = "#6c757d", Icon = "❌", SortOrder = 7 }
            );

            // ── SEED: Brands ─────────────────────────────────────────────────
            builder.Entity<Brand>().HasData(
                new Brand
                {
                    Id = 1,
                    NameAr = "DOOGEE",
                    NameEn = "DOOGEE",
                    Slug = "doogee",
                    LogoPath = "/images/brands/doogee.png",
                    DescAr = "هواتف وتابلتات متينة بمواصفات عالية",
                    DescEn = "Rugged phones and tablets with high specs",
                    SortOrder = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Brand
                {
                    Id = 2,
                    NameAr = "JisuLife",
                    NameEn = "JisuLife",
                    Slug = "jisulife",
                    LogoPath = "/images/brands/jisulife.png",
                    DescAr = "مراوح محمولة وأجهزة ذكية للحياة اليومية",
                    DescEn = "Portable fans and smart devices for daily life",
                    SortOrder = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Brand
                {
                    Id = 3,
                    NameAr = "Dreame",
                    NameEn = "Dreame",
                    Slug = "dreame",
                    LogoPath = "/images/brands/dreame.png",
                    DescAr = "أجهزة تنظيف ذكية ومتطورة",
                    DescEn = "Smart and advanced cleaning devices",
                    SortOrder = 3,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            builder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    NameAr = "تابلتات",
                    NameEn = "Tablets",
                    Slug = "tablets",
                    Emoji = "📱",
                    SortOrder = 1,
                    IsActive = true,
                    ShowOnHomepage = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Category
                {
                    Id = 2,
                    NameAr = "مراوح محمولة",
                    NameEn = "Fans",
                    Slug = "fans",
                    Emoji = "🌀",
                    SortOrder = 2,
                    IsActive = true,
                    ShowOnHomepage = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Category
                {
                    Id = 3,
                    NameAr = "كاميرات ويب",
                    NameEn = "Webcams",
                    Slug = "webcams",
                    Emoji = "📷",
                    SortOrder = 3,
                    IsActive = true,
                    ShowOnHomepage = true,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // ── SEED: Products ───────────────────────────────────────────────
            builder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    CategoryId = 1,
                    BrandName = "DOOGEE",
                    Emoji = "📱",
                    Badge = "NEW",
                    NameAr = "تابلت T30 Ultra",
                    NameEn = "T30 Ultra Tablet",
                    Slug = "doogee-t30-ultra",
                    ShortDescAr = "شاشة 11 بوصة، بطارية 8300mAh، معالج Helio G99",
                    ShortDescEn = "11\" display, 8300mAh battery, Helio G99 processor",
                    DescAr = "تابلت متطور بشاشة 11 بوصة بدقة 2K وأداء استثنائي مناسب للعمل والترفيه في كل مكان.",
                    DescEn = "Advanced tablet with 11\" 2K display and exceptional performance for work and entertainment everywhere.",
                    Price = 8999,
                    IsActive = true,
                    IsFeatured = true,
                    Stock = 50,
                    SortOrder = 1,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Product
                {
                    Id = 2,
                    CategoryId = 1,
                    BrandName = "DOOGEE",
                    Emoji = "💪",
                    Badge = "RUGGED",
                    NameAr = "تابلت R20 متين",
                    NameEn = "R20 Rugged Tablet",
                    Slug = "doogee-r20-rugged",
                    ShortDescAr = "مقاوم للماء IP68، شاشة 10.4 بوصة، بطارية 15600mAh",
                    ShortDescEn = "IP68 waterproof, 10.4\" screen, 15600mAh battery",
                    DescAr = "تابلت متين مقاوم للماء والغبار والصدمات وفق معيار IP68. مثالي للاستخدام الصناعي والمغامرات.",
                    DescEn = "Rugged tablet resistant to water, dust and shocks per IP68 standard. Ideal for industrial use and outdoor adventures.",
                    Price = 12500,
                    IsActive = true,
                    IsFeatured = true,
                    Stock = 30,
                    SortOrder = 2,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Product
                {
                    Id = 3,
                    CategoryId = 2,
                    BrandName = "JISULIFE",
                    Emoji = "🌀",
                    Badge = "HOT",
                    NameAr = "مروحة محمولة F40",
                    NameEn = "F40 Portable Fan",
                    Slug = "jisulife-f40",
                    ShortDescAr = "بطارية 10000mAh، تدوم حتى 24 ساعة، 4 سرعات",
                    ShortDescEn = "10000mAh battery, up to 24hrs, 4 speeds",
                    DescAr = "مروحة محمولة ذكية ببطارية قوية تدوم حتى 24 ساعة بالسرعة المنخفضة.",
                    DescEn = "Smart portable fan with powerful battery lasting up to 24 hours on low speed.",
                    Price = 1299,
                    IsActive = true,
                    IsFeatured = true,
                    Stock = 100,
                    SortOrder = 1,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Product
                {
                    Id = 4,
                    CategoryId = 2,
                    BrandName = "JISULIFE",
                    Emoji = "🌬️",
                    Badge = "",
                    NameAr = "مروحة رقبة F8 Pro",
                    NameEn = "F8 Pro Neck Fan",
                    Slug = "jisulife-f8-pro",
                    ShortDescAr = "تُلبس حول الرقبة، خفيفة 135 جرام، USB-C",
                    ShortDescEn = "Wearable neck design, 135g ultra-light, USB-C",
                    DescAr = "مروحة تُلبس حول الرقبة لراحة مستمرة. خفيفة جداً بوزن 135 جرام فقط.",
                    DescEn = "Wearable neck fan for continuous comfort without holding it. Ultra-light at only 135g.",
                    Price = 899,
                    IsActive = true,
                    Stock = 80,
                    SortOrder = 2,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Product
                {
                    Id = 5,
                    CategoryId = 3,
                    BrandName = "DREAME",
                    Emoji = "📷",
                    Badge = "4K",
                    NameAr = "كاميرا ويب 4K Pro",
                    NameEn = "4K Pro Webcam",
                    Slug = "dreame-4k-pro",
                    ShortDescAr = "دقة 4K، عدسة واسعة 90 درجة، ميكروفون مدمج",
                    ShortDescEn = "4K resolution, 90° wide lens, built-in microphone",
                    DescAr = "كاميرا ويب احترافية بدقة 4K وعدسة واسعة مع ميكروفون مزدوج بإلغاء الضوضاء.",
                    DescEn = "Professional webcam with 4K resolution and 90° wide lens with dual noise-canceling microphone.",
                    Price = 2499,
                    IsActive = true,
                    IsFeatured = true,
                    Stock = 60,
                    SortOrder = 1,
                    CreatedAt = new DateTime(2026, 1, 1)
                },
                new Product
                {
                    Id = 6,
                    CategoryId = 3,
                    BrandName = "DREAME",
                    Emoji = "🎥",
                    Badge = "",
                    NameAr = "كاميرا ويب HD Stream",
                    NameEn = "HD Stream Webcam",
                    Slug = "dreame-hd-stream",
                    ShortDescAr = "1080p Full HD، تصحيح تلقائي للإضاءة، Plug & Play",
                    ShortDescEn = "1080p Full HD, auto light correction, Plug & Play",
                    DescAr = "كاميرا ويب 1080p بتصحيح تلقائي للإضاءة. Plug & Play بدون تثبيت برامج.",
                    DescEn = "1080p Full HD webcam with automatic light correction. Plug & Play with no software needed.",
                    Price = 1199,
                    IsActive = true,
                    Stock = 70,
                    SortOrder = 2,
                    CreatedAt = new DateTime(2026, 1, 1)
                }
            );

            // ── SEED: Content Blocks ─────────────────────────────────────────
            builder.Entity<ContentBlock>().HasData(
                new ContentBlock { Id = 1, Key = "about.title.ar", BodyAr = "نقدم أفضل التكنولوجيا لحياة أفضل", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 2, Key = "about.title.en", BodyEn = "Delivering The Best Technology For Better Life", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 3, Key = "about.body.ar", BodyAr = "الفجر للتجارة شركة رائدة في بيع وتوزيع أجهزة التكنولوجيا الحديثة بأفضل الأسعار وأعلى جودة في السوق المصري.", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 4, Key = "about.body.en", BodyEn = "Alfajr For Trade is a leading company in selling and distributing modern technology devices at the best prices and highest quality in the Egyptian market.", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 5, Key = "about.stat.years", BodyAr = "10+", BodyEn = "10+", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 6, Key = "about.stat.clients", BodyAr = "50K+", BodyEn = "50K+", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 7, Key = "about.stat.products", BodyAr = "200+", BodyEn = "200+", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 8, Key = "mission.text.ar", BodyAr = "توفير أحدث التقنيات العالمية بأسعار عادلة وخدمة مبنية على الثقة والجودة والاحترافية في كل تفاصيل تجربة العميل.", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 9, Key = "mission.text.en", BodyEn = "Providing the latest global technologies at fair prices with service built on trust, quality and professionalism in every detail of the customer experience.", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 10, Key = "vision.text.ar", BodyAr = "أن نكون الوجهة الأولى والأكثر ثقة للتكنولوجيا في مصر والشرق الأوسط.", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 11, Key = "vision.text.en", BodyEn = "To be the first and most trusted technology destination in Egypt and the Middle East.", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 12, Key = "hero.slide1.tag.ar", BodyAr = "وصل حديثاً", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 13, Key = "hero.slide1.tag.en", BodyEn = "New Arrival", UpdatedAt = new DateTime(2026, 1, 1) },
                // ── Features Section ──────────────────────────────────────────
                new ContentBlock { Id = 14, Key = "features.title.ar", BodyAr = "لماذا Uni-Shop؟", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 15, Key = "features.title.en", BodyEn = "Why Uni-Shop?", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 16, Key = "features.1", TitleAr = "✓", BodyAr = "ضمان أصلي على جميع المنتجات", BodyEn = "Original warranty on all products", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 17, Key = "features.2", TitleAr = "🚚", BodyAr = "توصيل سريع لجميع المحافظات", BodyEn = "Fast delivery to all governorates", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 18, Key = "features.3", TitleAr = "💬", BodyAr = "دعم فني على مدار الساعة", BodyEn = "24/7 technical support", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 19, Key = "features.4", TitleAr = "↩", BodyAr = "سياسة إرجاع 14 يوم", BodyEn = "14-day return policy", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 20, Key = "features.5", TitleAr = "💳", BodyAr = "تقسيط بدون فوائد", BodyEn = "0% interest installments", UpdatedAt = new DateTime(2026, 1, 1) },
                new ContentBlock { Id = 21, Key = "features.6", TitleAr = "⭐", BodyAr = "منتجات أصلية 100% معتمدة", BodyEn = "100% authentic certified products", UpdatedAt = new DateTime(2026, 1, 1) },
                // ── Homepage Dynamic Content (Hero) ──────────────────────────
                new ContentBlock { Id = 22, Key = "hero.chip1", BodyAr = "مباشر · الأكثر مبيعاً", BodyEn = "LIVE · Best Seller Q3", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 23, Key = "hero.chip2", BodyAr = "وصل حديثاً · ٢٤", BodyEn = "New Arrivals · 24", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 24, Key = "hero.title.line1", BodyAr = "منصة التحكم", BodyEn = "The command deck", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 25, Key = "hero.title.line2", BodyAr = "للتكنولوجيا العصرية.", BodyEn = "for modern tech.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 26, Key = "hero.subtitle", BodyAr = "لوحة تحكم ومنصة منسقة لأجهزة DOOGEE و JisuLife و Dreame — صُممت للمحترفين وتمت معايرتها للحياة اليومية. توصيل لكافة محافظات مصر بضمان معتمد.", BodyEn = "A curated instrument panel of DOOGEE, JisuLife and Dreame hardware — engineered for professionals, calibrated for daily life. Delivered across Egypt with certified warranty.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 27, Key = "hero.btn1.text", BodyAr = "اكتشف الكتالوج", BodyEn = "Explore Catalog", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 28, Key = "hero.btn1.url", BodyAr = "/Products", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 29, Key = "hero.btn2.text", BodyAr = "المميز هذا الأسبوع", BodyEn = "Featured this week", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 30, Key = "hero.btn2.url", BodyAr = "#featured", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 31, Key = "hero.stat1.label", BodyAr = "سنوات خبرة", BodyEn = "Years Experience", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 32, Key = "hero.stat2.label", BodyAr = "عميل سعيد", BodyEn = "Delivered Orders", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 33, Key = "hero.stat3.label", BodyAr = "منتج فريد", BodyEn = "Curated SKUs", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── Value Props (icon stored in TitleAr as Bootstrap Icons class) ──
                new ContentBlock { Id = 34, Key = "value.prop1.title", TitleAr = "bi-shield-check", BodyAr = "ضمان معتمد", BodyEn = "Certified Warranty", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 35, Key = "value.prop1.desc", BodyAr = "على جميع المنتجات", BodyEn = "On every product", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 36, Key = "value.prop2.title", TitleAr = "bi-truck", BodyAr = "توصيل سريع", BodyEn = "Fast Delivery", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 37, Key = "value.prop2.desc", BodyAr = "لكل محافظات مصر", BodyEn = "To all governorates", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 38, Key = "value.prop3.title", TitleAr = "bi-headset", BodyAr = "دعم فني 24/7", BodyEn = "24/7 Support", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 39, Key = "value.prop3.desc", BodyAr = "فريق متخصص دائماً", BodyEn = "Expert team, always on", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 40, Key = "value.prop4.title", TitleAr = "bi-arrow-counterclockwise", BodyAr = "استرجاع 14 يوم", BodyEn = "14-Day Returns", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 41, Key = "value.prop4.desc", BodyAr = "بدون أي تعقيدات", BodyEn = "Hassle-free policy", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── Category Showcase Section ─────────────────────────────────
                new ContentBlock { Id = 42, Key = "cat.showcase.eyebrow", BodyAr = "تسوق حسب الفئة · ٠١", BodyEn = "Shop by Category · 01", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 43, Key = "cat.showcase.title", BodyAr = "اختر فئتك، واكتشف الأفضل.", BodyEn = "Pick a category, discover the best.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 44, Key = "cat.showcase.desc", BodyAr = "كل فئة منسقة بعناية — أجهزة مختارة من أفضل العلامات العالمية بمواصفات مؤكدة وأسعار تنافسية.", BodyEn = "Every category is hand-curated — devices from top global brands with verified specs and competitive prices.", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── Featured Catalog Section ──────────────────────────────────
                new ContentBlock { Id = 45, Key = "feat.catalog.eyebrow", BodyAr = "كتالوج المنتجات · ٠٢", BodyEn = "Product Catalog · 02", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 46, Key = "feat.catalog.title", BodyAr = "أجهزة منسقة لكل أسلوب عمل.", BodyEn = "Curated hardware for every workflow.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 47, Key = "feat.catalog.desc", BodyAr = "قم بالتصفية حسب الفئة لاستكشاف المنتجات الهامة فقط. تأتي جميع الوحدات مع ضمان معتمد ودعم فني متاح دائماً.", BodyEn = "Filter by category to explore only what matters. Every unit ships with certified warranty and 24/7 technical support.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 48, Key = "feat.catalog.btn.text", BodyAr = "عرض جميع المنتجات", BodyEn = "View All Products", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── CTA Band ──────────────────────────────────────────────────
                new ContentBlock { Id = 49, Key = "cta.title", BodyAr = "جاهز ترتقي بأجهزتك؟ ابدأ التسوق الآن.", BodyEn = "Ready to upgrade your gear? Start shopping now.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 50, Key = "cta.desc", BodyAr = "منتجات أصلية 100% بضمان معتمد، توصيل سريع لباب البيت، وتقسيط بدون فوائد. تجربة تسوق عالمية بمعايير مصرية.", BodyEn = "100% authentic products with certified warranty, fast door-to-door delivery, and 0% interest installments.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 51, Key = "cta.btn.text", BodyAr = "تسوق الآن", BodyEn = "Shop Now", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 52, Key = "cta.btn.url", BodyAr = "/Products", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── About / Mission / Contact section headers ─────────────────
                new ContentBlock { Id = 53, Key = "about.eyebrow", BodyAr = "من نحن · ٠٣", BodyEn = "About Us · 03", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 54, Key = "about.headline", BodyAr = "تقديم أفضل التكنولوجيا لحياة أكثر سهولة وذكاءً.", BodyEn = "Delivering the best technology for a better life.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 55, Key = "mission.eyebrow", BodyAr = "رؤيتنا ورسالتنا · ٠٤", BodyEn = "Mission & Vision · 04", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 56, Key = "mission.headline", BodyAr = "نبني مستقبل التكنولوجيا في مصر.", BodyEn = "Building the tech future in Egypt.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 57, Key = "contact.eyebrow", BodyAr = "تواصل معنا · ٠٥", BodyEn = "Contact · 05", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 58, Key = "contact.title", BodyAr = "نحن هنا لمساعدتك دائماً.", BodyEn = "We're always here to help.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 59, Key = "contact.desc", BodyAr = "تواصل مع فريق الدعم الفني والمبيعات مباشرة. نقوم بالرد على كافة الاستفسارات خلال ساعات العمل الرسمية.", BodyEn = "Reach our support team directly. We respond to all requests within official working hours.", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── Newsletter & Footer ───────────────────────────────────────
                new ContentBlock { Id = 60, Key = "newsletter.title", BodyAr = "خلّيك أول من يعرف 🔥", BodyEn = "Be the first to know 🔥", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 61, Key = "newsletter.desc", BodyAr = "اشترك ليصلك كل جديد عن المنتجات والعروض الحصرية قبل أي حد.", BodyEn = "Subscribe for new arrivals and exclusive deals before anyone else.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 62, Key = "newsletter.btn.text", BodyAr = "اشترك", BodyEn = "Subscribe", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 63, Key = "footer.desc", BodyAr = "بوابتك المعتمدة لأحدث الأجهزة التكنولوجية من DOOGEE و JisuLife و Dreame. نوفر جودة فائقة وضمان حقيقي.", BodyEn = "Your certified gateway for the latest tech hardware from DOOGEE, JisuLife, and Dreame. Delivering premium quality and certified warranty.", UpdatedAt = new DateTime(2026, 7, 12) },
                new ContentBlock { Id = 64, Key = "footer.credits", BodyAr = "يونى شوب. جميع الحقوق محفوظة.", BodyEn = "Uni-Shop. All Rights Reserved.", UpdatedAt = new DateTime(2026, 7, 12) }
            );

            // ── SEED: Contact Info ───────────────────────────────────────────
            builder.Entity<ContactInfo>().HasData(new ContactInfo
            {
                Id = 1,
                Phone = "+20 100 000 0000",
                Email = "info@unishop.com",
                AddressAr = "القاهرة، مصر الجديدة",
                AddressEn = "Cairo, Heliopolis",
                City = "Cairo",
                Facebook = "https://facebook.com/unishop",
                Instagram = "https://instagram.com/unishop",
                WhatsApp = "+201000000000",
                WorkingHoursAr = "السبت – الخميس: 10ص – 10م",
                WorkingHoursEn = "Sat – Thu: 10AM – 10PM",
                ShowPhone = true,
                ShowPhone2 = false,
                ShowEmail = true,
                ShowAddress = true,
                ShowMap = true,
                ShowWorkingHours = true,
                ShowFacebook = true,
                ShowInstagram = true,
                ShowWhatsApp = true,
                ShowTikTok = false,
                UpdatedAt = new DateTime(2026, 1, 1)
            });

            // ── SEED: Site Settings ──────────────────────────────────────────
            builder.Entity<SiteSetting>().HasData(
                new SiteSetting { Id = 1, Key = "site.name", Value = "Uni-Shop", Description = "اسم الموقع", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 2, Key = "site.tagline.ar", Value = "يونى شوب للمبيعات", Description = "الشعار بالعربي", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 3, Key = "site.tagline.en", Value = "Uni-Shop E-Commerce", Description = "Tagline English", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 4, Key = "site.primary.color", Value = "#1A6BFF", Description = "اللون الأساسي", Type = "color", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 5, Key = "shipping.free.above", Value = "5000", Description = "شحن مجاني فوق (EGP)", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 6, Key = "shipping.fee", Value = "150", Description = "رسوم الشحن (EGP)", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 7, Key = "meta.title.ar", Value = "Uni-Shop — وجهتك للتسوق", Description = "Meta Title", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 8, Key = "meta.desc.ar", Value = "وجهتك الأولى لأحدث التكنولوجيا في مصر", Description = "Meta Desc AR", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                new SiteSetting { Id = 9, Key = "meta.desc.en", Value = "Your first destination for the latest technology in Egypt", Description = "Meta Desc EN", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) },
                // ── Homepage product selections (comma-separated product IDs, order matters) ──
                new SiteSetting { Id = 10, Key = "homepage.hero.products", Value = "1,3,5", Description = "منتجات سلايدر الهيرو (IDs مرتبة)", Type = "text", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 11, Key = "homepage.featured.products", Value = "1,2,3,4,5,6", Description = "منتجات الكتالوج المميز (IDs مرتبة)", Type = "text", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── Homepage section visibility toggles ───────────────────────
                new SiteSetting { Id = 12, Key = "homepage.show.hero", Value = "1", Description = "إظهار قسم الهيرو", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 13, Key = "homepage.show.values", Value = "1", Description = "إظهار شريط المزايا", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 15, Key = "homepage.show.categories", Value = "1", Description = "إظهار معرض الفئات", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 16, Key = "homepage.show.featured", Value = "1", Description = "إظهار الكتالوج المميز", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 17, Key = "homepage.show.about", Value = "1", Description = "إظهار قسم من نحن", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 18, Key = "homepage.show.mission", Value = "1", Description = "إظهار الرؤية والرسالة", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 19, Key = "homepage.show.cta", Value = "1", Description = "إظهار شريط CTA", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 20, Key = "homepage.show.contact", Value = "1", Description = "إظهار قسم التواصل", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 21, Key = "homepage.show.newsletter", Value = "1", Description = "إظهار النشرة البريدية بالفوتر", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 22, Key = "homepage.show.payments", Value = "1", Description = "إظهار شارات الدفع بالفوتر", Type = "bool", UpdatedAt = new DateTime(2026, 7, 12) },
                // ── Homepage section ordering & misc ──────────────────────────
                new SiteSetting { Id = 23, Key = "homepage.sections.order", Value = "hero,values,categories,featured,about,mission,cta,contact", Description = "ترتيب أقسام الرئيسية", Type = "text", UpdatedAt = new DateTime(2026, 7, 12) },
                new SiteSetting { Id = 24, Key = "homepage.categories.count", Value = "3", Description = "عدد بلاطات الفئات بالرئيسية", Type = "text", UpdatedAt = new DateTime(2026, 7, 12) }
            );
        }
    }
}
