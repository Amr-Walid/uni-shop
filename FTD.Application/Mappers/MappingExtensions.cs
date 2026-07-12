using System;
using System.Collections.Generic;
using System.Linq;
using FTD.Domain.Entities;
using FTD.Application.DTOs;

namespace FTD.Application.Mappers
{
    public static class MappingExtensions
    {
        // Brand
        public static BrandDto? ToDto(this Brand? entity)
        {
            if (entity == null) return null;
            return new BrandDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Slug = entity.Slug,
                LogoPath = entity.LogoPath,
                BannerPath = entity.BannerPath,
                DescAr = entity.DescAr,
                DescEn = entity.DescEn,
                IsActive = entity.IsActive,
                SortOrder = entity.SortOrder,
                CreatedAt = entity.CreatedAt,
                ProductsCount = entity.Products?.Count ?? 0
            };
        }

        // Category
        public static CategoryDto? ToDto(this Category? entity)
        {
            if (entity == null) return null;
            return new CategoryDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Slug = entity.Slug,
                ImagePath = entity.ImagePath,
                Emoji = entity.Emoji,
                Description = entity.Description,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                ShowOnHomepage = entity.ShowOnHomepage,
                CreatedAt = entity.CreatedAt,
                ProductsCount = entity.Products?.Count ?? 0
            };
        }

        // ProductImage
        public static ProductImageDto? ToDto(this ProductImage? entity)
        {
            if (entity == null) return null;
            return new ProductImageDto
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                ImagePath = entity.ImagePath,
                IsMain = entity.IsMain,
                SortOrder = entity.SortOrder
            };
        }

        // ProductAttribute
        public static ProductAttributeDto? ToDto(this ProductAttribute? entity)
        {
            if (entity == null) return null;
            return new ProductAttributeDto
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                SortOrder = entity.SortOrder,
                Category = entity.Category?.ToDto(),
                Values = entity.Values?.Select(v => v.ToDto()).Where(x => x != null).Select(x => x!).ToList() ?? new()
            };
        }

        // AttributeValue
        public static AttributeValueDto? ToDto(this AttributeValue? entity)
        {
            if (entity == null) return null;
            return new AttributeValueDto
            {
                Id = entity.Id,
                AttributeId = entity.AttributeId,
                ValueAr = entity.ValueAr,
                ValueEn = entity.ValueEn
            };
        }

        // ProductAttributeValue
        public static ProductAttributeValueDto? ToDto(this ProductAttributeValue? entity)
        {
            if (entity == null) return null;
            return new ProductAttributeValueDto
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                AttributeId = entity.AttributeId,
                AttributeValueId = entity.AttributeValueId,
                Attribute = entity.Attribute?.ToDto(),
                AttributeValue = entity.AttributeValue?.ToDto()
            };
        }

        // Product
        public static ProductDto? ToDto(this Product? entity)
        {
            if (entity == null) return null;
            return new ProductDto
            {
                Id = entity.Id,
                CategoryId = entity.CategoryId,
                BrandId = entity.BrandId,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Slug = entity.Slug,
                ShortDescAr = entity.ShortDescAr,
                ShortDescEn = entity.ShortDescEn,
                DescAr = entity.DescAr,
                DescEn = entity.DescEn,
                Price = entity.Price,
                OldPrice = entity.OldPrice,
                Badge = entity.Badge,
                ImagePath = entity.ImagePath,
                BrandName = entity.BrandName,
                Emoji = entity.Emoji,
                IsActive = entity.IsActive,
                IsFeatured = entity.IsFeatured,
                SortOrder = entity.SortOrder,
                Stock = entity.Stock,
                CreatedAt = entity.CreatedAt,
                MetaTitle = entity.MetaTitle,
                MetaDesc = entity.MetaDesc,
                Category = entity.Category?.ToDto(),
                Brand = entity.Brand?.ToDto(),
                AttributeValues = entity.AttributeValues?.Select(av => av.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList() ?? new(),
                Images = entity.Images?.Select(i => i.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList() ?? new()
            };
        }

        // OrderStatus
        public static OrderStatusDto? ToDto(this OrderStatus? entity)
        {
            if (entity == null) return null;
            return new OrderStatusDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                ColorHex = entity.ColorHex,
                Icon = entity.Icon,
                SortOrder = entity.SortOrder
            };
        }

        // SalesOrderDetail
        public static SalesOrderDetailDto? ToDto(this SalesOrderDetail? entity)
        {
            if (entity == null) return null;
            return new SalesOrderDetailDto
            {
                Id = entity.Id,
                OrderId = entity.OrderId,
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                SubTotal = entity.SubTotal,
                Product = entity.Product?.ToDto()
            };
        }

        // SalesOrder
        public static SalesOrderDto? ToDto(this SalesOrder? entity)
        {
            if (entity == null) return null;
            return new SalesOrderDto
            {
                Id = entity.Id,
                OrderNumber = entity.OrderNumber,
                StatusId = entity.StatusId,
                CustomerName = entity.CustomerName,
                CustomerPhone = entity.CustomerPhone,
                CustomerEmail = entity.CustomerEmail,
                Address = entity.Address,
                City = entity.City,
                Governorate = entity.Governorate,
                SubTotal = entity.SubTotal,
                ShippingFee = entity.ShippingFee,
                TotalAmount = entity.TotalAmount,
                Notes = entity.Notes,
                AdminNotes = entity.AdminNotes,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Status = entity.Status?.ToDto(),
                Details = entity.Details?.Select(d => d.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList() ?? new()
            };
        }

        // ContentBlock
        public static ContentBlockDto? ToDto(this ContentBlock? entity)
        {
            if (entity == null) return null;
            return new ContentBlockDto
            {
                Id = entity.Id,
                Key = entity.Key,
                TitleAr = entity.TitleAr,
                TitleEn = entity.TitleEn,
                BodyAr = entity.BodyAr,
                BodyEn = entity.BodyEn,
                ImagePath = entity.ImagePath,
                Type = entity.Type,
                UpdatedAt = entity.UpdatedAt
            };
        }

        // PageSection
        public static PageSectionDto? ToDto(this PageSection? entity)
        {
            if (entity == null) return null;
            return new PageSectionDto
            {
                Id = entity.Id,
                PageId = entity.PageId,
                Type = entity.Type,
                ContentJson = entity.ContentJson,
                SortOrder = entity.SortOrder,
                IsVisible = entity.IsVisible
            };
        }

        // ContentPage
        public static ContentPageDto? ToDto(this ContentPage? entity)
        {
            if (entity == null) return null;
            return new ContentPageDto
            {
                Id = entity.Id,
                Slug = entity.Slug,
                TitleAr = entity.TitleAr,
                TitleEn = entity.TitleEn,
                BodyAr = entity.BodyAr,
                BodyEn = entity.BodyEn,
                IsPublished = entity.IsPublished,
                MetaTitle = entity.MetaTitle,
                MetaDesc = entity.MetaDesc,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                Sections = entity.Sections?.Select(s => s.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList() ?? new()
            };
        }

        // NavigationItem
        public static NavigationItemDto? ToDto(this NavigationItem? entity)
        {
            if (entity == null) return null;
            return new NavigationItemDto
            {
                Id = entity.Id,
                LabelAr = entity.LabelAr,
                LabelEn = entity.LabelEn,
                LinkType = entity.LinkType,
                StaticRoute = entity.StaticRoute,
                PageSlug = entity.PageSlug,
                ExternalUrl = entity.ExternalUrl,
                OpenInNewTab = entity.OpenInNewTab,
                Location = entity.Location,
                ParentId = entity.ParentId,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                // Avoid infinite recursive loops by not mapping Parent object hierarchy or its children
                Children = entity.Children?.Select(c => c.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList() ?? new()
            };
        }

        // ContactInfo
        public static ContactInfoDto? ToDto(this ContactInfo? entity)
        {
            if (entity == null) return null;
            return new ContactInfoDto
            {
                Id = entity.Id,
                Phone = entity.Phone,
                Phone2 = entity.Phone2,
                Email = entity.Email,
                AddressAr = entity.AddressAr,
                AddressEn = entity.AddressEn,
                City = entity.City,
                MapEmbedUrl = entity.MapEmbedUrl,
                Facebook = entity.Facebook,
                Instagram = entity.Instagram,
                WhatsApp = entity.WhatsApp,
                TikTok = entity.TikTok,
                WorkingHoursAr = entity.WorkingHoursAr,
                WorkingHoursEn = entity.WorkingHoursEn,
                ShowPhone = entity.ShowPhone,
                ShowPhone2 = entity.ShowPhone2,
                ShowEmail = entity.ShowEmail,
                ShowAddress = entity.ShowAddress,
                ShowMap = entity.ShowMap,
                ShowWorkingHours = entity.ShowWorkingHours,
                ShowFacebook = entity.ShowFacebook,
                ShowInstagram = entity.ShowInstagram,
                ShowWhatsApp = entity.ShowWhatsApp,
                ShowTikTok = entity.ShowTikTok,
                UpdatedAt = entity.UpdatedAt
            };
        }

        // SiteSetting
        public static SiteSettingDto? ToDto(this SiteSetting? entity)
        {
            if (entity == null) return null;
            return new SiteSettingDto
            {
                Id = entity.Id,
                Key = entity.Key,
                Value = entity.Value,
                Description = entity.Description,
                Type = entity.Type,
                UpdatedAt = entity.UpdatedAt
            };
        }

        // ContactMessage
        public static ContactMessageDto? ToDto(this ContactMessage? entity)
        {
            if (entity == null) return null;
            return new ContactMessageDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Phone = entity.Phone,
                Message = entity.Message,
                IsRead = entity.IsRead,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
