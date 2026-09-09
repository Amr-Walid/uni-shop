using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;

namespace FTD.Api.Controllers.V1
{
    /// <summary>
    /// Public product catalog: paged listings, product detail, categories,
    /// brands, filter facets and search.
    /// </summary>
    [ApiController]
    [Route("api/v1")]
    [Tags("Catalog")]
    public class CatalogController : ApiControllerBase
    {
        /// <summary>
        /// Cache window for near-static reference data. Short enough that an
        /// admin edit appears quickly, long enough to absorb the burst of
        /// identical requests every app launch produces.
        /// </summary>
        private static readonly TimeSpan ReferenceDataCacheDuration = TimeSpan.FromMinutes(2);

        private readonly ICatalogQueryService _catalog;
        private readonly IMemoryCache _cache;

        public CatalogController(ICatalogQueryService catalog, IMemoryCache cache)
        {
            _catalog = catalog;
            _cache = cache;
        }

        /// <summary>
        /// One page of products, with filtering and sorting.
        /// </summary>
        /// <param name="page">1-based page index. Default 1.</param>
        /// <param name="pageSize">Items per page. Default 20, hard maximum 50.</param>
        /// <param name="categoryId">Filter by category id.</param>
        /// <param name="category">Filter by category slug.</param>
        /// <param name="brandId">Filter by brand id.</param>
        /// <param name="brand">Filter by brand slug.</param>
        /// <param name="q">Free-text search across names, descriptions and brands.</param>
        /// <param name="sort">default | price_asc | price_desc | newest | name_asc</param>
        /// <param name="minPrice">Minimum price.</param>
        /// <param name="maxPrice">Maximum price.</param>
        /// <param name="av">Attribute value ids; a product must match ALL of them.</param>
        /// <param name="onSale">Only discounted products.</param>
        /// <param name="inStock">Only products currently in stock.</param>
        [HttpGet("products")]
        [ProducesResponseType(typeof(PagedResult<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] int? categoryId,
            [FromQuery] string? category,
            [FromQuery] int? brandId,
            [FromQuery] string? brand,
            [FromQuery] string? q,
            [FromQuery] string? sort,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] List<int>? av,
            [FromQuery] bool? onSale,
            [FromQuery] bool? inStock)
        {
            var filter = new CatalogFilter
            {
                CategoryId = categoryId,
                CategorySlug = category,
                BrandId = brandId,
                BrandSlug = brand,
                Query = q,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                AttributeValueIds = av,
                OnSaleOnly = onSale,
                InStockOnly = inStock,
                Sort = ParseSort(sort)
            };

            var (normalizedPage, normalizedSize) = PagedResult<ProductListItemDto>.Normalize(page, pageSize);

            var result = await _catalog.GetProductsAsync(
                filter, normalizedPage, normalizedSize, RequestLanguage, MediaBaseUrl);

            return Ok(result);
        }

        /// <summary>Full detail for one product, by slug.</summary>
        /// <response code="404">No active product with this slug (PRODUCT_NOT_FOUND).</response>
        [HttpGet("products/{slug}")]
        [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductBySlug(string slug)
        {
            var product = await _catalog.GetProductBySlugAsync(slug, RequestLanguage, MediaBaseUrl);

            return product == null
                ? NotFoundProblem("المنتج غير موجود", ApiErrorCodes.ProductNotFound)
                : Ok(product);
        }

        /// <summary>Products flagged as featured, for the home screen.</summary>
        [HttpGet("products/featured")]
        [ProducesResponseType(typeof(List<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFeatured([FromQuery] int take = 8)
            => Ok(await _catalog.GetFeaturedProductsAsync(take, RequestLanguage, MediaBaseUrl));

        /// <summary>Other products in the same category.</summary>
        [HttpGet("products/{id:int}/related")]
        [ProducesResponseType(typeof(List<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRelated(int id, [FromQuery] int take = 4)
            => Ok(await _catalog.GetRelatedProductsAsync(id, take, RequestLanguage, MediaBaseUrl));

        /// <summary>All active categories with their active-product counts.</summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CategoryListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            var lang = RequestLanguage;
            var cacheKey = $"api:categories:{lang}:{MediaBaseUrl}";

            var categories = await _cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ReferenceDataCacheDuration;
                return _catalog.GetCategoriesAsync(lang, MediaBaseUrl);
            });

            return Ok(categories);
        }

        /// <summary>All active brands with their active-product counts.</summary>
        [HttpGet("brands")]
        [ProducesResponseType(typeof(List<BrandListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBrands()
        {
            var lang = RequestLanguage;
            var cacheKey = $"api:brands:{lang}:{MediaBaseUrl}";

            var brands = await _cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ReferenceDataCacheDuration;
                return _catalog.GetBrandsAsync(lang, MediaBaseUrl);
            });

            return Ok(brands);
        }

        /// <summary>
        /// Filter facets for a category, used to build the filter sheet.
        /// Only values that match at least one active product are returned, so
        /// the UI never offers an option that yields zero results.
        /// </summary>
        [HttpGet("categories/{categoryId:int}/attributes")]
        [ProducesResponseType(typeof(List<AttributeFilterGroupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryAttributes(int categoryId)
            => Ok(await _catalog.GetCategoryAttributesAsync(categoryId, RequestLanguage));

        /// <summary>Paged full-text product search.</summary>
        [HttpGet("search")]
        [EnableRateLimiting("search-policy")]
        [ProducesResponseType(typeof(PagedResult<ProductListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? sort)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequestProblem("نص البحث مطلوب", ApiErrorCodes.ValidationFailed);

            var (normalizedPage, normalizedSize) = PagedResult<ProductListItemDto>.Normalize(page, pageSize);

            var result = await _catalog.GetProductsAsync(
                new CatalogFilter { Query = q, Sort = ParseSort(sort) },
                normalizedPage, normalizedSize, RequestLanguage, MediaBaseUrl);

            return Ok(result);
        }

        /// <summary>Name-only suggestions for the search box (2+ characters).</summary>
        [HttpGet("search/suggest")]
        [EnableRateLimiting("search-policy")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Suggest([FromQuery] string q, [FromQuery] int take = 8)
            => Ok(await _catalog.GetSearchSuggestionsAsync(q, take, RequestLanguage));

        /// <summary>
        /// Everything the home screen needs in a single request: categories,
        /// brands, hero slider, featured, new arrivals, sale items and public
        /// settings.
        ///
        /// This replaces 5-7 sequential calls. On a mobile link with 200-400 ms
        /// round-trip that is well over a second saved before first paint.
        /// </summary>
        [HttpGet("home")]
        [ProducesResponseType(typeof(HomeDataDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetHome()
        {
            var lang = RequestLanguage;
            var cacheKey = $"api:home:{lang}:{MediaBaseUrl}";

            var data = await _cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ReferenceDataCacheDuration;
                return _catalog.GetHomeDataAsync(lang, MediaBaseUrl);
            });

            return Ok(data);
        }

        /// <summary>
        /// Public store settings (shipping thresholds, theme colour, currency).
        ///
        /// Reading the theme colour from here lets an admin restyle the app
        /// without shipping a new store build.
        /// </summary>
        [HttpGet("settings/public")]
        [ProducesResponseType(typeof(PublicSettingsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicSettings()
        {
            var settings = await _cache.GetOrCreateAsync("api:settings:public", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = ReferenceDataCacheDuration;
                return _catalog.GetPublicSettingsAsync();
            });

            return Ok(settings);
        }

        private static CatalogSort ParseSort(string? sort) => sort?.ToLowerInvariant() switch
        {
            "price_asc" => CatalogSort.PriceAscending,
            "price_desc" => CatalogSort.PriceDescending,
            "newest" => CatalogSort.Newest,
            "name_asc" => CatalogSort.NameAscending,
            _ => CatalogSort.Default
        };
    }
}
