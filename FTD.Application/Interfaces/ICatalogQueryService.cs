using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    /// <summary>Sort options accepted by the paged catalog query.</summary>
    public enum CatalogSort
    {
        /// <summary>Featured first, then the admin's manual sort order.</summary>
        Default = 0,
        PriceAscending = 1,
        PriceDescending = 2,
        Newest = 3,
        NameAscending = 4
    }

    /// <summary>Filter criteria for a catalog page request.</summary>
    public class CatalogFilter
    {
        public int? CategoryId { get; set; }
        public string? CategorySlug { get; set; }
        public int? BrandId { get; set; }
        public string? BrandSlug { get; set; }
        public string? Query { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        /// <summary>Attribute value ids; a product must match ALL of them (AND).</summary>
        public List<int>? AttributeValueIds { get; set; }

        public bool? OnSaleOnly { get; set; }
        public bool? InStockOnly { get; set; }
        public bool? FeaturedOnly { get; set; }
        public CatalogSort Sort { get; set; } = CatalogSort.Default;
    }

    /// <summary>
    /// Read-optimised catalog queries for API/mobile clients.
    ///
    /// Deliberately separate from <see cref="IProductService"/>: that interface
    /// serves the MVC storefront and admin panel and returns the rich
    /// <c>ProductDto</c>. Adding paging and localization overloads there would
    /// have grown an already 30+ member interface and risked changing behaviour
    /// the website depends on. This service instead returns trimmed, localized,
    /// absolute-URL DTOs and never mutates state.
    /// </summary>
    public interface ICatalogQueryService
    {
        /// <summary>
        /// One page of products.
        /// Ordering is always made deterministic by a final tie-break on Id —
        /// without it, SQL Server may return rows in a different order between
        /// requests and items would duplicate or vanish across pages
        /// (the same class of bug as AUDIT_REPORT I-25).
        /// </summary>
        Task<PagedResult<ProductListItemDto>> GetProductsAsync(
            CatalogFilter filter, int page, int pageSize, string lang, string? mediaBaseUrl);

        Task<ProductDetailDto?> GetProductBySlugAsync(string slug, string lang, string? mediaBaseUrl);
        Task<ProductDetailDto?> GetProductByIdAsync(int id, string lang, string? mediaBaseUrl);

        Task<List<ProductListItemDto>> GetRelatedProductsAsync(
            int productId, int take, string lang, string? mediaBaseUrl);

        Task<List<ProductListItemDto>> GetFeaturedProductsAsync(
            int take, string lang, string? mediaBaseUrl);

        Task<List<CategoryListItemDto>> GetCategoriesAsync(string lang, string? mediaBaseUrl);
        Task<List<BrandListItemDto>> GetBrandsAsync(string lang, string? mediaBaseUrl);

        /// <summary>Filter facets for a category, used to build the filter sheet.</summary>
        Task<List<AttributeFilterGroupDto>> GetCategoryAttributesAsync(int categoryId, string lang);

        /// <summary>Lightweight name-only suggestions for the search box.</summary>
        Task<List<string>> GetSearchSuggestionsAsync(string query, int take, string lang);

        /// <summary>
        /// Everything the home screen needs in a single round-trip, so the app
        /// does not pay 5-7 sequential mobile round-trips before first paint.
        /// </summary>
        Task<HomeDataDto> GetHomeDataAsync(string lang, string? mediaBaseUrl);

        Task<PublicSettingsDto> GetPublicSettingsAsync();
    }
}
