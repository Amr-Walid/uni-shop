using System;
using System.Collections.Generic;

namespace FTD.Application.DTOs
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  LIGHTWEIGHT CATALOG DTOs
    //  Purpose-built for mobile list screens. The existing ProductDto carries
    //  full descriptions in two languages, the whole Category and Brand objects,
    //  every attribute value (each with two nested objects), every image and the
    //  SEO fields — roughly 4 KB per item. Sending 300 of those is >1 MB, which
    //  is unusable on a mobile connection. These trimmed shapes are ~350 bytes.
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>Single product as shown in a grid/list. ~90% smaller than ProductDto.</summary>
    public class ProductListItemDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";

        /// <summary>Already localized by the service using the request language.</summary>
        public string Name { get; set; } = "";
        public string? ShortDesc { get; set; }

        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }

        /// <summary>
        /// Computed server-side so every client (mobile, web, future integrations)
        /// displays an identical discount and cannot drift through rounding.
        /// </summary>
        public int? DiscountPercent { get; set; }

        public string? Badge { get; set; }
        public string? Emoji { get; set; }

        /// <summary>Absolute URL — the app has no way to resolve a relative path.</summary>
        public string? ImageUrl { get; set; }

        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }

        /// <summary>
        /// SECURITY: the raw Stock number is commercially sensitive and is never
        /// exposed to storefront clients — only availability flags.
        /// </summary>
        public bool InStock { get; set; }
        public bool LowStock { get; set; }

        public bool IsFeatured { get; set; }
    }

    /// <summary>Full product detail for the product page.</summary>
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string Name { get; set; } = "";
        public string? ShortDesc { get; set; }
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int? DiscountPercent { get; set; }

        public string? Badge { get; set; }
        public string? Emoji { get; set; }

        public bool InStock { get; set; }
        public bool LowStock { get; set; }
        public bool IsFeatured { get; set; }

        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }

        /// <summary>
        /// Free-form feature bullets authored by the admin. Parsed defensively —
        /// malformed JSON yields an empty list rather than failing the request.
        /// </summary>
        public List<string> Features { get; set; } = new();

        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public string? BrandSlug { get; set; }
        public string? BrandLogoUrl { get; set; }

        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategorySlug { get; set; }

        /// <summary>Absolute URLs, main image first.</summary>
        public List<string> Images { get; set; } = new();

        public List<ProductSpecDto> Specs { get; set; } = new();
    }

    /// <summary>One "attribute: value" row in the specs table.</summary>
    public class ProductSpecDto
    {
        public int AttributeId { get; set; }
        public string Name { get; set; } = "";
        public int ValueId { get; set; }
        public string Value { get; set; } = "";
    }

    /// <summary>Category tile for the mobile catalog.</summary>
    public class CategoryListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? Emoji { get; set; }
        public string? ImageUrl { get; set; }
        public int ProductsCount { get; set; }
    }

    /// <summary>Brand tile for the mobile catalog.</summary>
    public class BrandListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public int ProductsCount { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  AGGREGATED HOME PAYLOAD
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Everything the mobile home screen needs in ONE round-trip.
    ///
    /// Rationale: fetching banners, categories, brands, featured products, new
    /// arrivals, sale items and settings separately costs 7 sequential requests.
    /// On a mobile network with 200-400 ms RTT that is well over a second of
    /// dead time before anything renders. One aggregated (and cached) response
    /// removes that entirely.
    /// </summary>
    public class HomeDataDto
    {
        public List<CategoryListItemDto> Categories { get; set; } = new();
        public List<BrandListItemDto> Brands { get; set; } = new();
        public List<ProductListItemDto> HeroProducts { get; set; } = new();
        public List<ProductListItemDto> FeaturedProducts { get; set; } = new();
        public List<ProductListItemDto> NewArrivals { get; set; } = new();
        public List<ProductListItemDto> OnSale { get; set; } = new();
        public PublicSettingsDto Settings { get; set; } = new();
    }

    /// <summary>
    /// Non-sensitive store configuration the app needs at startup. Notably it
    /// lets the app pick up the admin-configured theme colour and shipping rules
    /// WITHOUT shipping a new build to the stores.
    /// </summary>
    public class PublicSettingsDto
    {
        public string SiteName { get; set; } = "Uni-Shop";
        public string? TaglineAr { get; set; }
        public string? TaglineEn { get; set; }
        public string PrimaryColor { get; set; } = "#1A6BFF";
        public decimal ShippingFee { get; set; }
        public decimal FreeShippingAbove { get; set; }
        public string Currency { get; set; } = "EGP";
        public string CurrencySymbolAr { get; set; } = "ج.م";
        public string CurrencySymbolEn { get; set; } = "EGP";
    }

    /// <summary>
    /// Remote kill-switch / force-update contract.
    ///
    /// This is the ONLY way to react to a critical bug in a build that is
    /// already installed on customers' phones — without it you must wait for
    /// users to update voluntarily, which takes weeks.
    /// </summary>
    public class AppConfigDto
    {
        /// <summary>Clients older than this must block and prompt to update.</summary>
        public string MinSupportedVersion { get; set; } = "1.0.0";
        public string LatestVersion { get; set; } = "1.0.0";
        public bool ForceUpdate { get; set; }
        public string? UpdateMessageAr { get; set; }
        public string? UpdateMessageEn { get; set; }
        public string? AndroidStoreUrl { get; set; }
        public string? IosStoreUrl { get; set; }

        public bool MaintenanceMode { get; set; }
        public string? MaintenanceMessageAr { get; set; }
        public string? MaintenanceMessageEn { get; set; }

        public PublicSettingsDto Settings { get; set; } = new();
    }
}
