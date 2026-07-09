using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FTD.Web.Models
{
    // ── BRAND ─────────────────────────────────────────────────────────────────
    public class Brand
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string NameAr { get; set; } = "";
        [Required, MaxLength(100)] public string NameEn { get; set; } = "";
        [Required, MaxLength(100)] public string Slug { get; set; } = "";
        public string? LogoPath { get; set; }  // colored logo
        public string? BannerPath { get; set; }  // hero banner image
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Nav
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    // ── CATEGORY ──────────────────────────────────────────────────────────────────
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(150)] public string NameAr { get; set; } = "";
        [Required, MaxLength(150)] public string NameEn { get; set; } = "";
        [Required, MaxLength(100)] public string Slug { get; set; } = "";
        public string? ImagePath { get; set; }
        public string? Emoji { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    }

    // ── PRODUCT ───────────────────────────────────────────────────────────────
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }   // FK → Brand

        [Required, MaxLength(200)] public string NameAr { get; set; } = "";
        [Required, MaxLength(200)] public string NameEn { get; set; } = "";
        [Required, MaxLength(100)] public string Slug { get; set; } = "";

        public string? ShortDescAr { get; set; }
        public string? ShortDescEn { get; set; }
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? OldPrice { get; set; }

        public string? Badge { get; set; }   // NEW, HOT, RUGGED, 4K …
        public string? ImagePath { get; set; }
        public string? BrandName { get; set; }   // display / legacy fallback
        public string? Emoji { get; set; }   // fallback when no image

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }

        // Nav
        public Category Category { get; set; } = null!;
        public Brand? Brand { get; set; }
        public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<SalesOrderDetail> OrderDetails { get; set; } = new List<SalesOrderDetail>();
    }

    // ── PRODUCT IMAGES ────────────────────────────────────────────────────────
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required] public string ImagePath { get; set; } = "";
        public bool IsMain { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        public Product Product { get; set; } = null!;
    }

    // ── ATTRIBUTE ─────────────────────────────────────────────────────────────
    public class ProductAttribute
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [Required, MaxLength(100)] public string NameAr { get; set; } = "";
        [Required, MaxLength(100)] public string NameEn { get; set; } = "";
        public int SortOrder { get; set; } = 0;

        public Category Category { get; set; } = null!;
        public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
        public ICollection<ProductAttributeValue> ProductValues { get; set; } = new List<ProductAttributeValue>();
    }

    // ── ATTRIBUTE VALUE ───────────────────────────────────────────────────────
    public class AttributeValue
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }

        [Required, MaxLength(200)] public string ValueAr { get; set; } = "";
        [Required, MaxLength(200)] public string ValueEn { get; set; } = "";

        public ProductAttribute Attribute { get; set; } = null!;
        public ICollection<ProductAttributeValue> ProductValues { get; set; } = new List<ProductAttributeValue>();
    }

    // ── PRODUCT ATTRIBUTE VALUE (Junction) ────────────────────────────────────
    public class ProductAttributeValue
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }

        public Product Product { get; set; } = null!;
        public ProductAttribute Attribute { get; set; } = null!;
        public AttributeValue AttributeValue { get; set; } = null!;
    }

    // ── ORDER STATUS ──────────────────────────────────────────────────────────
    public class OrderStatus
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string NameAr { get; set; } = "";
        [Required, MaxLength(100)] public string NameEn { get; set; } = "";
        [MaxLength(7)] public string ColorHex { get; set; } = "#6c757d";
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;

        public ICollection<SalesOrder> Orders { get; set; } = new List<SalesOrder>();
    }

    // ── SALES ORDER ───────────────────────────────────────────────────────────
    public class SalesOrder
    {
        public int Id { get; set; }
        [Required, MaxLength(30)] public string OrderNumber { get; set; } = "";
        public int StatusId { get; set; }

        // Customer Info
        [Required, MaxLength(150)] public string CustomerName { get; set; } = "";
        [Required, MaxLength(20)] public string CustomerPhone { get; set; } = "";
        [MaxLength(200)] public string? CustomerEmail { get; set; }
        [MaxLength(300)] public string? Address { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        [MaxLength(100)] public string? Governorate { get; set; }

        // Financials
        [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ShippingFee { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Nav
        public OrderStatus Status { get; set; } = null!;
        public ICollection<SalesOrderDetail> Details { get; set; } = new List<SalesOrderDetail>();
    }

    // ── SALES ORDER DETAIL ────────────────────────────────────────────────────
    public class SalesOrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(200)] public string ProductName { get; set; } = "";  // snapshot
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }

        public SalesOrder Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }

    // ── CONTENT BLOCK ─────────────────────────────────────────────────────────
    // Key examples: about.title, about.body, hero.slide1.title, mission.text
    public class ContentBlock
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Key { get; set; } = "";
        [MaxLength(300)] public string? TitleAr { get; set; }
        [MaxLength(300)] public string? TitleEn { get; set; }
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public string? ImagePath { get; set; }
        [MaxLength(50)] public string Type { get; set; } = "text";   // text | html | image
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── CONTENT PAGE ─────────────────────────────────────────────────────────
    // Free pages created from Admin (privacy, shipping, terms …)
    public class ContentPage
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Slug { get; set; } = "";
        [Required, MaxLength(200)] public string TitleAr { get; set; } = "";
        [Required, MaxLength(200)] public string TitleEn { get; set; } = "";
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public bool IsPublished { get; set; } = true;

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Sections (JSON stored per page)
        public ICollection<PageSection> Sections { get; set; } = new List<PageSection>();
    }

    // ── PAGE SECTION ──────────────────────────────────────────────────────────
    public class PageSection
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        [Required, MaxLength(50)] public string Type { get; set; } = "RichText";
        // RichText | FAQ | Cards | Hero | ContactForm
        public string? ContentJson { get; set; }   // JSON payload depends on Type
        public int SortOrder { get; set; } = 0;
        public bool IsVisible { get; set; } = true;

        public ContentPage Page { get; set; } = null!;
    }

    // ── NAVIGATION ITEM ───────────────────────────────────────────────────────
    public class NavigationItem
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string LabelAr { get; set; } = "";
        [Required, MaxLength(100)] public string LabelEn { get; set; } = "";

        // Type: Static | ContentPage | External
        [Required, MaxLength(20)] public string LinkType { get; set; } = "Static";
        public string? StaticRoute { get; set; }
        public string? PageSlug { get; set; }
        public string? ExternalUrl { get; set; }
        public bool OpenInNewTab { get; set; } = false;

        // Location: Navbar | Footer | Both | Hidden
        [Required, MaxLength(20)] public string Location { get; set; } = "Both";

        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public NavigationItem? Parent { get; set; }
        public ICollection<NavigationItem> Children { get; set; } = new List<NavigationItem>();
    }

    // ── CONTACT INFO ──────────────────────────────────────────────────────────
    public class ContactInfo
    {
        public int Id { get; set; } = 1;

        // Values
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? AddressAr { get; set; }
        public string? AddressEn { get; set; }
        public string? City { get; set; }
        public string? MapEmbedUrl { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? WhatsApp { get; set; }
        public string? TikTok { get; set; }
        public string? WorkingHoursAr { get; set; }
        public string? WorkingHoursEn { get; set; }

        // Visibility toggles — false = hidden from public site
        public bool ShowPhone { get; set; } = true;
        public bool ShowPhone2 { get; set; } = false;
        public bool ShowEmail { get; set; } = true;
        public bool ShowAddress { get; set; } = true;
        public bool ShowMap { get; set; } = true;
        public bool ShowWorkingHours { get; set; } = true;
        public bool ShowFacebook { get; set; } = true;
        public bool ShowInstagram { get; set; } = true;
        public bool ShowWhatsApp { get; set; } = true;
        public bool ShowTikTok { get; set; } = false;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── CONTACT MESSAGE ───────────────────────────────────────────────────────
    public class ContactMessage
    {
        public int Id { get; set; }
        [MaxLength(100)] public string? Name { get; set; }
        [MaxLength(100)] public string? Email { get; set; }
        [MaxLength(20)] public string? Phone { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ── SITE SETTINGS ─────────────────────────────────────────────────────────
    public class SiteSetting
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Key { get; set; } = "";
        public string? Value { get; set; }
        [MaxLength(200)] public string? Description { get; set; }
        [MaxLength(50)] public string Type { get; set; } = "text";  // text|bool|image|color
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
