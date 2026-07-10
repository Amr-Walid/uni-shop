using System;
using System.Collections.Generic;

namespace FTD.Application.DTOs
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? LogoPath { get; set; }
        public string? BannerPath { get; set; }
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? ImagePath { get; set; }
        public string? Emoji { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
    }

    public class ProductDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? ShortDescAr { get; set; }
        public string? ShortDescEn { get; set; }
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string? Badge { get; set; }
        public string? ImagePath { get; set; }
        public string? BrandName { get; set; }
        public string? Emoji { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }

        public CategoryDto? Category { get; set; }
        public BrandDto? Brand { get; set; }
        public List<ProductAttributeValueDto> AttributeValues { get; set; } = new();
        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; } = "";
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductAttributeDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public int SortOrder { get; set; }
        public CategoryDto? Category { get; set; }
        public List<AttributeValueDto> Values { get; set; } = new();
    }

    public class AttributeValueDto
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string ValueAr { get; set; } = "";
        public string ValueEn { get; set; } = "";
    }

    public class ProductAttributeValueDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }
        public ProductAttributeDto? Attribute { get; set; }
        public AttributeValueDto? AttributeValue { get; set; }
    }

    public class OrderStatusDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public string? Icon { get; set; }
        public int SortOrder { get; set; }
    }

    public class SalesOrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public int StatusId { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string? CustomerEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public OrderStatusDto? Status { get; set; }
        public List<SalesOrderDetailDto> Details { get; set; } = new();
    }

    public class SalesOrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal SubTotal { get; set; }
        public ProductDto? Product { get; set; }
    }

    public class ContentBlockDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string? TitleAr { get; set; }
        public string? TitleEn { get; set; }
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public string? ImagePath { get; set; }
        public string Type { get; set; } = "text";
        public DateTime UpdatedAt { get; set; }
    }

    public class ContentPageDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string TitleAr { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public bool IsPublished { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PageSectionDto> Sections { get; set; } = new();
    }

    public class PageSectionDto
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public string Type { get; set; } = "";
        public string? ContentJson { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
    }

    public class NavigationItemDto
    {
        public int Id { get; set; }
        public string LabelAr { get; set; } = "";
        public string LabelEn { get; set; } = "";
        public string LinkType { get; set; } = "";
        public string? StaticRoute { get; set; }
        public string? PageSlug { get; set; }
        public string? ExternalUrl { get; set; }
        public bool OpenInNewTab { get; set; }
        public string Location { get; set; } = "";
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public NavigationItemDto? Parent { get; set; }
        public List<NavigationItemDto> Children { get; set; } = new();
    }

    public class ContactInfoDto
    {
        public int Id { get; set; }
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
        public bool ShowPhone { get; set; }
        public bool ShowPhone2 { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowAddress { get; set; }
        public bool ShowMap { get; set; }
        public bool ShowWorkingHours { get; set; }
        public bool ShowFacebook { get; set; }
        public bool ShowInstagram { get; set; }
        public bool ShowWhatsApp { get; set; }
        public bool ShowTikTok { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SiteSettingDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string? Value { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }

    public class ContactMessageDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CheckoutDto
    {
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string? CustomerEmail { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public string? Notes { get; set; }
    }

    public class CartItemDto
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

    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.SubTotal);
        public decimal ShippingFee { get; set; }
        public decimal FreeShippingAbove { get; set; } = 5000;
        public bool FreeShipping => SubTotal >= FreeShippingAbove;
        public decimal Total => SubTotal + (FreeShipping ? 0 : ShippingFee);
    }

    public class DashboardDto
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int NewOrders { get; set; }
        public int PendingOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public List<SalesOrderDto> RecentOrders { get; set; } = new();
        public List<OrderStatusCountDto> OrdersByStatus { get; set; } = new();
    }

    public class OrderStatusCountDto
    {
        public string StatusName { get; set; } = "";
        public string ColorHex { get; set; } = "";
        public int Count { get; set; }
    }

    public class AttributeFilterGroupDto
    {
        public int AttributeId { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public List<AttributeFilterOptionDto> Options { get; set; } = new();
    }

    public class AttributeFilterOptionDto
    {
        public int ValueId { get; set; }
        public string ValueAr { get; set; } = "";
        public string ValueEn { get; set; } = "";
        public int Count { get; set; }
    }
}

