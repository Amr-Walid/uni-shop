using FTD.Application.DTOs;
using FTD.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Web.ViewModels
{
    // ── HOME ──────────────────────────────────────────────────────────────────
    public class HomeViewModel
    {
        public List<ProductDto> HeroProducts { get; set; } = new();
        public List<ProductDto> FeaturedProducts { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public Dictionary<string, string> ContentBlocks { get; set; } = new();
        public ContactInfoDto? ContactInfo { get; set; }
        public List<SiteSettingDto> Settings { get; set; } = new();
        public Dictionary<string, string> SettingsMap { get; set; } = new();

        // مساعدات قراءة إعدادات الرئيسية
        public bool ShowSection(string section)
            => !SettingsMap.TryGetValue($"homepage.show.{section}", out var v)
               || v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase);

        public List<string> SectionsOrder
            => (SettingsMap.TryGetValue("homepage.sections.order", out var v) && !string.IsNullOrWhiteSpace(v)
                    ? v : "hero,values,marquee,categories,featured,about,mission,cta,contact")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

        public int HomepageCategoriesCount
            => SettingsMap.TryGetValue("homepage.categories.count", out var v) && int.TryParse(v, out var n) && n > 0 ? n : 3;
    }

    // ── PRODUCTS LIST ─────────────────────────────────────────────────────────
    public class ProductsViewModel
    {
        public List<ProductDto> Products { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<BrandDto> Brands { get; set; } = new();
        // Current context
        public CategoryDto? CurrentCategory { get; set; }
        public BrandDto? CurrentBrand { get; set; }
        // Active filters
        public string? BrandFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public string? SearchQuery { get; set; }
        public string? SortBy { get; set; }
        public List<int> SelectedAttrValues { get; set; } = new();
        // Uses AttributeFilterGroupDto from Application — no duplicate class
        public List<AttributeFilterGroupDto> AttributeGroups { get; set; } = new();
        public int TotalCount { get; set; }
    }

    // ── PRODUCT DETAIL ────────────────────────────────────────────────────────
    public class ProductDetailViewModel
    {
        public ProductDto Product { get; set; } = null!;
        public List<ProductDto> RelatedProducts { get; set; } = new();
        public List<ProductAttributeValueDto> Attributes { get; set; } = new();
    }

    // ── CART ──────────────────────────────────────────────────────────────────
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductNameEn { get; set; } = "";
        public string? Emoji { get; set; }
        public string? ImagePath { get; set; }
        public string? BrandName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.SubTotal);
        public decimal ShippingFee { get; set; } = 150;
        public decimal Total => SubTotal + ShippingFee;
        public bool FreeShipping => SubTotal >= 5000;
    }

    // ── CHECKOUT ─────────────────────────────────────────────────────────────
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم الكامل")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string CustomerPhone { get; set; } = "";

        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string? CustomerEmail { get; set; }

        [Display(Name = "العنوان")]
        public string? Address { get; set; }

        [Display(Name = "المحافظة")]
        public string? Governorate { get; set; }

        [Display(Name = "المدينة")]
        public string? City { get; set; }

        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public CartViewModel Cart { get; set; } = new();
    }

    // ── SEARCH ───────────────────────────────────────────────────────────────
    public class SearchResultsViewModel
    {
        public string Query { get; set; } = "";
        public List<ProductDto> Products { get; set; } = new();
        public int TotalCount { get; set; }
    }

    // ── CONTENT PAGE ─────────────────────────────────────────────────────────
    public class ContentPageViewModel
    {
        public ContentPageDto Page { get; set; } = null!;
        public string CurrentLang { get; set; } = "ar";
    }

    // ── ADMIN: Dashboard ──────────────────────────────────────────────────────
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public List<SalesOrderDto> RecentOrders { get; set; } = new();
        // Uses OrderStatusCountDto directly — no duplicate class needed
        public List<OrderStatusCountDto> OrdersByStatus { get; set; } = new();
    }

    // ── ADMIN: Product Form ───────────────────────────────────────────────────
    public class ProductFormViewModel
    {
        public ProductDto Product { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public List<BrandDto> Brands { get; set; } = new();
        public List<ProductAttributeDto> Attributes { get; set; } = new();
        public Dictionary<int, int> SelectedAttributeValues { get; set; } = new();
        // Free-text specs: attributes that have no predefined values render as a
        // plain text input — the typed value is created in the DB on save and
        // linked to the product (key = AttributeId, value = the typed text).
        public Dictionary<int, string> FreeTextAttributeValues { get; set; } = new();
        public IFormFile? MainImage { get; set; }
        public List<IFormFile> ProductImages { get; set; } = new();
        public List<ProductImageDto> ExistingImages { get; set; } = new();
    }

    // ── ADMIN: Home Content bulk block input ───────────────────────────
    public class BlockInput
    {
        public string? Ar { get; set; }
        public string? En { get; set; }
        public string? Icon { get; set; }
    }

    // ── ADMIN: Order Detail ───────────────────────────────────────────────────
    public class OrderDetailViewModel
    {
        public SalesOrderDto Order { get; set; } = null!;
        public List<OrderStatusDto> AllStatuses { get; set; } = new();
    }

    // ── LAYOUT VIEW COMPONENTS ────────────────────────────────────────────────
    public class NavbarViewModel
    {
        public List<NavigationItemDto> NavItems { get; set; } = new();
        public List<BrandDto> NavBrands { get; set; } = new();
        public List<CategoryDto> NavCategories { get; set; } = new();
    }

    public class FooterViewModel
    {
        public List<NavigationItemDto> FootItems { get; set; } = new();
        public ContactInfoDto? ContactInfo { get; set; }
        public List<BrandDto> NavBrands { get; set; } = new();
        public List<CategoryDto> NavCategories { get; set; } = new();
        public Dictionary<string, string> Blocks { get; set; } = new();
        public Dictionary<string, string> Settings { get; set; } = new();

        public bool ShowSection(string section)
            => !Settings.TryGetValue($"homepage.show.{section}", out var v)
               || v == "1" || v.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
