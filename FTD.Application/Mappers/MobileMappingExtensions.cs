using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Domain.Entities;

namespace FTD.Application.Mappers
{
    /// <summary>
    /// Projections from entities to the trimmed, localized, absolute-URL DTOs
    /// the mobile API returns.
    ///
    /// Kept separate from <see cref="MappingExtensions"/> (which serves the MVC
    /// site) because these mappings need two extra inputs the web mappers do
    /// not have: the request language and the media base URL.
    /// </summary>
    public static class MobileMappingExtensions
    {
        /// <summary>
        /// Below this remaining quantity the UI shows an urgency hint
        /// ("only a few left") without revealing the exact stock figure.
        /// </summary>
        private const int LowStockThreshold = 5;

        // ── Products ──────────────────────────────────────────────────────────

        public static ProductListItemDto ToListItem(this Product p, string lang, string? mediaBaseUrl)
        {
            return new ProductListItemDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Name = LocalizationHelper.Pick(p.NameAr, p.NameEn, lang),
                ShortDesc = LocalizationHelper.PickOptional(p.ShortDescAr, p.ShortDescEn, lang),
                Price = p.Price,
                OldPrice = p.OldPrice,
                DiscountPercent = CalculateDiscountPercent(p.Price, p.OldPrice),
                Badge = p.Badge,
                Emoji = p.Emoji,
                ImageUrl = MediaUrlHelper.ToAbsolute(p.ImagePath, mediaBaseUrl),
                // Brand may be a real FK or the legacy free-text fallback.
                BrandName = p.Brand != null
                    ? LocalizationHelper.Pick(p.Brand.NameAr, p.Brand.NameEn, lang)
                    : p.BrandName,
                CategoryName = p.Category != null
                    ? LocalizationHelper.Pick(p.Category.NameAr, p.Category.NameEn, lang)
                    : null,
                CategorySlug = p.Category?.Slug,
                // SECURITY: expose availability, never the raw stock count.
                InStock = p.Stock > 0,
                LowStock = p.Stock > 0 && p.Stock <= LowStockThreshold,
                IsFeatured = p.IsFeatured
            };
        }

        public static ProductDetailDto ToDetail(this Product p, string lang, string? mediaBaseUrl)
        {
            var dto = new ProductDetailDto
            {
                Id = p.Id,
                Slug = p.Slug,
                Name = LocalizationHelper.Pick(p.NameAr, p.NameEn, lang),
                ShortDesc = LocalizationHelper.PickOptional(p.ShortDescAr, p.ShortDescEn, lang),
                Description = LocalizationHelper.PickOptional(p.DescAr, p.DescEn, lang),
                Price = p.Price,
                OldPrice = p.OldPrice,
                DiscountPercent = CalculateDiscountPercent(p.Price, p.OldPrice),
                Badge = p.Badge,
                Emoji = p.Emoji,
                InStock = p.Stock > 0,
                LowStock = p.Stock > 0 && p.Stock <= LowStockThreshold,
                IsFeatured = p.IsFeatured,
                MetaTitle = p.MetaTitle,
                MetaDesc = p.MetaDesc,
                Features = ParseFeatures(p.FeaturesJson),
                BrandId = p.BrandId,
                BrandName = p.Brand != null
                    ? LocalizationHelper.Pick(p.Brand.NameAr, p.Brand.NameEn, lang)
                    : p.BrandName,
                BrandSlug = p.Brand?.Slug,
                BrandLogoUrl = MediaUrlHelper.ToAbsolute(p.Brand?.LogoPath, mediaBaseUrl),
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null
                    ? LocalizationHelper.Pick(p.Category.NameAr, p.Category.NameEn, lang)
                    : null,
                CategorySlug = p.Category?.Slug
            };

            // Gallery: main image first, then the additional images in sort
            // order, de-duplicated so the primary photo is not shown twice.
            var images = new List<string>();
            var main = MediaUrlHelper.ToAbsolute(p.ImagePath, mediaBaseUrl);
            if (main != null) images.Add(main);

            if (p.Images != null)
            {
                foreach (var img in p.Images.OrderByDescending(i => i.IsMain).ThenBy(i => i.SortOrder))
                {
                    var url = MediaUrlHelper.ToAbsolute(img.ImagePath, mediaBaseUrl);
                    if (url != null && !images.Contains(url)) images.Add(url);
                }
            }
            dto.Images = images;

            if (p.AttributeValues != null)
            {
                dto.Specs = p.AttributeValues
                    .Where(av => av.Attribute != null && av.AttributeValue != null)
                    .OrderBy(av => av.Attribute!.SortOrder)
                    .Select(av => new ProductSpecDto
                    {
                        AttributeId = av.AttributeId,
                        Name = LocalizationHelper.Pick(av.Attribute!.NameAr, av.Attribute.NameEn, lang),
                        ValueId = av.AttributeValueId,
                        Value = LocalizationHelper.Pick(av.AttributeValue!.ValueAr, av.AttributeValue.ValueEn, lang)
                    })
                    .ToList();
            }

            return dto;
        }

        // ── Catalog reference data ────────────────────────────────────────────

        public static CategoryListItemDto ToListItem(this Category c, string lang, string? mediaBaseUrl, int productsCount = 0)
            => new()
            {
                Id = c.Id,
                Name = LocalizationHelper.Pick(c.NameAr, c.NameEn, lang),
                Slug = c.Slug,
                Emoji = c.Emoji,
                ImageUrl = MediaUrlHelper.ToAbsolute(c.ImagePath, mediaBaseUrl),
                ProductsCount = productsCount
            };

        public static BrandListItemDto ToListItem(this Brand b, string lang, string? mediaBaseUrl, int productsCount = 0)
            => new()
            {
                Id = b.Id,
                Name = LocalizationHelper.Pick(b.NameAr, b.NameEn, lang),
                Slug = b.Slug,
                LogoUrl = MediaUrlHelper.ToAbsolute(b.LogoPath, mediaBaseUrl),
                BannerUrl = MediaUrlHelper.ToAbsolute(b.BannerPath, mediaBaseUrl),
                ProductsCount = productsCount
            };

        public static OrderStatusSummaryDto ToSummary(this OrderStatus s, string lang)
            => new()
            {
                Id = s.Id,
                Name = LocalizationHelper.Pick(s.NameAr, s.NameEn, lang),
                ColorHex = s.ColorHex,
                Icon = s.Icon,
                SortOrder = s.SortOrder
            };

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Discount percentage, computed on the server so every client shows the
        /// same number. Guards against a zero/lower "old price", which would
        /// otherwise produce a divide-by-zero or a negative discount.
        /// </summary>
        public static int? CalculateDiscountPercent(decimal price, decimal? oldPrice)
        {
            if (!oldPrice.HasValue || oldPrice.Value <= 0) return null;
            if (oldPrice.Value <= price) return null;

            var discount = (oldPrice.Value - price) / oldPrice.Value * 100m;
            return (int)Math.Round(discount, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Parses the admin-authored FeaturesJson.
        ///
        /// Deliberately forgiving: the field is free-form text edited by hand,
        /// so malformed content is expected. It must degrade to an empty list
        /// rather than turning a product page into a 500. Supports both a plain
        /// JSON string array and an array of objects with a text-bearing
        /// property.
        /// </summary>
        public static List<string> ParseFeatures(string? featuresJson)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(featuresJson)) return result;

            try
            {
                using var doc = JsonDocument.Parse(featuresJson);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) return result;

                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        var value = element.GetString();
                        if (!string.IsNullOrWhiteSpace(value)) result.Add(value.Trim());
                    }
                    else if (element.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in new[] { "text", "value", "title", "name" })
                        {
                            if (element.TryGetProperty(prop, out var inner) &&
                                inner.ValueKind == JsonValueKind.String)
                            {
                                var value = inner.GetString();
                                if (!string.IsNullOrWhiteSpace(value))
                                {
                                    result.Add(value.Trim());
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Hand-edited content — an empty feature list is the correct
                // graceful outcome, not an error.
                return new List<string>();
            }

            return result;
        }
    }
}
