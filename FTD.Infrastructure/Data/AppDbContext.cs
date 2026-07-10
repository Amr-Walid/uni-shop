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

            // Brand FK - optional (no cascade issues)
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
                new ContentBlock { Id = 21, Key = "features.6", TitleAr = "⭐", BodyAr = "منتجات أصلية 100% معتمدة", BodyEn = "100% authentic certified products", UpdatedAt = new DateTime(2026, 1, 1) }
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
                new SiteSetting { Id = 9, Key = "meta.desc.en", Value = "Your first destination for the latest technology in Egypt", Description = "Meta Desc EN", Type = "text", UpdatedAt = new DateTime(2026, 1, 1) }
            );
        }
    }
}
