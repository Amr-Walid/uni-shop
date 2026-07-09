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
        public List<ProductDto> FeaturedProducts { get; set; } = new();
        public List<CategoryDto> Categories { get; set; } = new();
        public Dictionary<string, string> ContentBlocks { get; set; } = new();
        public ContactInfoDto? ContactInfo { get; set; }
        public List<SiteSettingDto> Settings { get; set; } = new();
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
        // Attribute filter groups
        public List<AttributeFilterGroup> AttributeGroups { get; set; } = new();
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
        public List<SalesOrder> RecentOrders { get; set; } = new();
        public List<OrderStatusCount> OrdersByStatus { get; set; } = new();
    }

    public class OrderStatusCount
    {
        public string StatusName { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public int Count { get; set; }
    }

    // ── ADMIN: Product Form ───────────────────────────────────────────────────
    public class ProductFormViewModel
    {
        public Product Product { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();
        public List<ProductAttribute> Attributes { get; set; } = new();
        public Dictionary<int, int> SelectedAttributeValues { get; set; } = new();
        public IFormFile? MainImage { get; set; }
        public List<IFormFile> ProductImages { get; set; } = new();
        public List<ProductImage> ExistingImages { get; set; } = new();
    }

    // ── PRODUCTS PAGE VIEWMODEL ───────────────────────────────────────────────
    public class ProductsPageViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();
        public List<ProductAttribute> Attributes { get; set; } = new();

        // Active filters
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? SearchQuery { get; set; }
        public Dictionary<int, List<int>> AttrFilters { get; set; } = new();

        public int TotalCount { get; set; }
        public string? SortBy { get; set; }
    }

    // ── ADMIN: Order Detail ───────────────────────────────────────────────────
    public class OrderDetailViewModel
    {
        public SalesOrder Order { get; set; } = null!;
        public List<OrderStatus> AllStatuses { get; set; } = new();
    }

    // ── PRODUCTS FILTER HELPERS ───────────────────────────────────────────────
    public class AttributeFilterGroup
    {
        public int AttributeId { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public List<AttributeFilterOption> Options { get; set; } = new();
    }

    public class AttributeFilterOption
    {
        public int ValueId { get; set; }
        public string ValueAr { get; set; } = "";
        public string ValueEn { get; set; } = "";
        public int Count { get; set; }
    }
}
