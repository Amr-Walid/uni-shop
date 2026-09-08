using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    /// <summary>
    /// Read-only catalog queries for the API. Every query is
    /// <c>AsNoTracking()</c> and bounded, and returns trimmed localized DTOs
    /// with absolute media URLs.
    /// </summary>
    public class CatalogQueryService : ICatalogQueryService
    {
        private readonly IAppDbContext _db;
        private readonly IContentService _content;

        public CatalogQueryService(IAppDbContext db, IContentService content)
        {
            _db = db;
            _content = content;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PRODUCTS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<PagedResult<ProductListItemDto>> GetProductsAsync(
            CatalogFilter filter, int page, int pageSize, string lang, string? mediaBaseUrl)
        {
            var (normalizedPage, normalizedSize) = PagedResult<ProductListItemDto>.Normalize(page, pageSize);
            filter ??= new CatalogFilter();

            var query = _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive);

            query = ApplyFilters(query, filter);

            // Attribute filtering is an AND across selections: the product must
            // carry EVERY chosen value. Expressed as one EXISTS per value so the
            // database does the work. The storefront filters in memory, which
            // cannot work here because TotalCount must also be computed in SQL.
            if (filter.AttributeValueIds != null && filter.AttributeValueIds.Count > 0)
            {
                foreach (var valueId in filter.AttributeValueIds.Distinct())
                {
                    var captured = valueId; // avoid closing over the loop variable
                    query = query.Where(p => p.AttributeValues.Any(av => av.AttributeValueId == captured));
                }
            }

            // COUNT first, over the exact same predicate as the page fetch.
            var totalCount = await query.CountAsync();
            if (totalCount == 0)
                return PagedResult<ProductListItemDto>.Empty(normalizedPage, normalizedSize);

            var entities = await ApplySort(query, filter.Sort)
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToListAsync();

            var items = entities.Select(p => p.ToListItem(lang, mediaBaseUrl)).ToList();
            return new PagedResult<ProductListItemDto>(items, normalizedPage, normalizedSize, totalCount);
        }

        private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, CatalogFilter filter)
        {
            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
            {
                var slug = filter.CategorySlug.Trim().ToLower();
                query = query.Where(p => p.Category.Slug.ToLower() == slug);
            }

            if (filter.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filter.BrandId.Value);

            if (!string.IsNullOrWhiteSpace(filter.BrandSlug))
            {
                var slug = filter.BrandSlug.Trim().ToLower();
                // Match the FK brand OR the legacy free-text BrandName, mirroring
                // the storefront so both surfaces return the same set.
                query = query.Where(p =>
                    (p.Brand != null && p.Brand.Slug.ToLower() == slug) ||
                    (p.BrandName != null && p.BrandName.ToLower() == slug));
            }

            if (!string.IsNullOrWhiteSpace(filter.Query))
                query = ApplyTextSearch(query, filter.Query);

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.OnSaleOnly == true)
                query = query.Where(p => p.OldPrice != null && p.OldPrice > p.Price);

            if (filter.InStockOnly == true)
                query = query.Where(p => p.Stock > 0);

            if (filter.FeaturedOnly == true)
                query = query.Where(p => p.IsFeatured);

            return query;
        }

        /// <summary>
        /// Applies sorting and ALWAYS appends a tie-break on Id.
        ///
        /// The tie-break is not cosmetic: paging over a non-deterministic order
        /// (many products share SortOrder 0) lets SQL Server return rows in a
        /// different sequence per query, so the same product can appear on two
        /// pages while another is never returned at all. Infinite scroll would
        /// show duplicates and silently skip items — the same class of bug as
        /// AUDIT_REPORT I-25.
        /// </summary>
        private static IQueryable<Product> ApplySort(IQueryable<Product> query, CatalogSort sort) => sort switch
        {
            CatalogSort.PriceAscending => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            CatalogSort.PriceDescending => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            CatalogSort.Newest => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id),
            CatalogSort.NameAscending => query.OrderBy(p => p.NameAr).ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.SortOrder).ThenBy(p => p.Id)
        };

        /// <summary>
        /// Bilingual text search. Mirrors the storefront predicate so the mobile
        /// app and the website agree on what "matches".
        /// </summary>
        private static IQueryable<Product> ApplyTextSearch(IQueryable<Product> source, string query)
        {
            var q = query.ToLower().Trim();
            return source.Where(p =>
                p.NameAr.ToLower().Contains(q) ||
                p.NameEn.ToLower().Contains(q) ||
                (p.BrandName != null && p.BrandName.ToLower().Contains(q)) ||
                (p.Brand != null && p.Brand.NameAr.ToLower().Contains(q)) ||
                (p.Brand != null && p.Brand.NameEn.ToLower().Contains(q)) ||
                (p.ShortDescAr != null && p.ShortDescAr.ToLower().Contains(q)) ||
                (p.ShortDescEn != null && p.ShortDescEn.ToLower().Contains(q)) ||
                (p.DescAr != null && p.DescAr.ToLower().Contains(q)) ||
                (p.DescEn != null && p.DescEn.ToLower().Contains(q)) ||
                (p.Badge != null && p.Badge.ToLower().Contains(q)) ||
                p.Category.NameAr.ToLower().Contains(q) ||
                p.Category.NameEn.ToLower().Contains(q));
        }

        public async Task<ProductDetailDto?> GetProductBySlugAsync(string slug, string lang, string? mediaBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(slug)) return null;

            var entity = await BuildDetailQuery()
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            return entity?.ToDetail(lang, mediaBaseUrl);
        }

        public async Task<ProductDetailDto?> GetProductByIdAsync(int id, string lang, string? mediaBaseUrl)
        {
            var entity = await BuildDetailQuery()
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return entity?.ToDetail(lang, mediaBaseUrl);
        }

        private IQueryable<Product> BuildDetailQuery() => _db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues).ThenInclude(av => av.Attribute)
            .Include(p => p.AttributeValues).ThenInclude(av => av.AttributeValue);

        public async Task<List<ProductListItemDto>> GetRelatedProductsAsync(
            int productId, int take, string lang, string? mediaBaseUrl)
        {
            if (take <= 0) take = 4;
            if (take > 20) take = 20;

            // Project only the column needed — no reason to materialise the row.
            var categoryId = await _db.Products
                .AsNoTracking()
                .Where(p => p.Id == productId)
                .Select(p => (int?)p.CategoryId)
                .FirstOrDefaultAsync();

            if (categoryId == null) return new List<ProductListItemDto>();

            var entities = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive && p.CategoryId == categoryId.Value && p.Id != productId)
                .OrderByDescending(p => p.IsFeatured)
                .ThenBy(p => p.SortOrder)
                .ThenBy(p => p.Id)
                .Take(take)
                .ToListAsync();

            return entities.Select(p => p.ToListItem(lang, mediaBaseUrl)).ToList();
        }

        public async Task<List<ProductListItemDto>> GetFeaturedProductsAsync(
            int take, string lang, string? mediaBaseUrl)
        {
            if (take <= 0) take = 8;
            if (take > 50) take = 50;

            var entities = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Id)
                .Take(take)
                .ToListAsync();

            return entities.Select(p => p.ToListItem(lang, mediaBaseUrl)).ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  REFERENCE DATA
        // ══════════════════════════════════════════════════════════════════════

        public async Task<List<CategoryListItemDto>> GetCategoriesAsync(string lang, string? mediaBaseUrl)
        {
            // Correlated COUNT projection rather than Include(c => c.Products):
            // loading whole product collections just to count them was
            // AUDIT_REPORT I-08.
            var rows = await _db.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .Select(c => new { Category = c, Count = c.Products.Count(p => p.IsActive) })
                .ToListAsync();

            return rows.Select(r => r.Category.ToListItem(lang, mediaBaseUrl, r.Count)).ToList();
        }

        public async Task<List<BrandListItemDto>> GetBrandsAsync(string lang, string? mediaBaseUrl)
        {
            var rows = await _db.Brands
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .Select(b => new { Brand = b, Count = b.Products.Count(p => p.IsActive) })
                .ToListAsync();

            return rows.Select(r => r.Brand.ToListItem(lang, mediaBaseUrl, r.Count)).ToList();
        }

        public async Task<List<AttributeFilterGroupDto>> GetCategoryAttributesAsync(int categoryId, string lang)
        {
            var attributes = await _db.ProductAttributes
                .AsNoTracking()
                .Include(a => a.Values)
                .Where(a => a.CategoryId == categoryId)
                .OrderBy(a => a.SortOrder)
                .ThenBy(a => a.Id)
                .ToListAsync();

            if (attributes.Count == 0) return new List<AttributeFilterGroupDto>();

            // Only offer values that match at least one active product, otherwise
            // the filter sheet shows options that can only ever yield zero results.
            var valueCounts = await _db.ProductAttributeValues
                .AsNoTracking()
                .Where(pav => pav.Product.IsActive && pav.Product.CategoryId == categoryId)
                .GroupBy(pav => pav.AttributeValueId)
                .Select(g => new { ValueId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ValueId, x => x.Count);

            var groups = new List<AttributeFilterGroupDto>();
            foreach (var attr in attributes)
            {
                var options = attr.Values
                    .Where(v => valueCounts.ContainsKey(v.Id))
                    .OrderBy(v => v.Id)
                    .Select(v => new AttributeFilterOptionDto
                    {
                        ValueId = v.Id,
                        ValueAr = v.ValueAr,
                        ValueEn = v.ValueEn,
                        Count = valueCounts[v.Id]
                    })
                    .ToList();

                if (options.Count == 0) continue;

                groups.Add(new AttributeFilterGroupDto
                {
                    AttributeId = attr.Id,
                    NameAr = attr.NameAr,
                    NameEn = attr.NameEn,
                    Options = options
                });
            }

            return groups;
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(string query, int take, string lang)
        {
            // Below two characters the result set is meaningless and expensive.
            if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
                return new List<string>();

            if (take <= 0) take = 8;
            if (take > 20) take = 20;

            var q = query.ToLower().Trim();

            // Projects ONLY the two name columns — a suggestion dropdown must
            // never pay the cost of full product rows. Over-fetches slightly so
            // de-duplication still yields a full list.
            var rows = await _db.Products
                .AsNoTracking()
                .Where(p => p.IsActive &&
                            (p.NameAr.ToLower().Contains(q) || p.NameEn.ToLower().Contains(q)))
                .OrderByDescending(p => p.IsFeatured)
                .ThenBy(p => p.Id)
                .Select(p => new { p.NameAr, p.NameEn })
                .Take(take * 2)
                .ToListAsync();

            return rows
                .Select(r => LocalizationHelper.Pick(r.NameAr, r.NameEn, lang))
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .Take(take)
                .ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  AGGREGATED HOME
        // ══════════════════════════════════════════════════════════════════════

        public async Task<HomeDataDto> GetHomeDataAsync(string lang, string? mediaBaseUrl)
        {
            var settings = await GetPublicSettingsAsync();

            // NOTE: these are awaited sequentially on purpose. A DbContext is
            // NOT thread-safe and all of these share the same scoped instance;
            // running them via Task.WhenAll throws "A second operation was
            // started on this context". The endpoint is cached instead.
            var categories = await GetCategoriesAsync(lang, mediaBaseUrl);
            var brands = await GetBrandsAsync(lang, mediaBaseUrl);
            var featured = await GetFeaturedProductsAsync(8, lang, mediaBaseUrl);

            // The admin curates the hero slider as an ordered id list in site
            // settings; fall back to featured products when it is unset.
            var heroIds = ParseIdList(await _content.GetSettingAsync("homepage.hero.products", ""));
            var heroProducts = heroIds.Count > 0
                ? await GetProductsByIdsAsync(heroIds, lang, mediaBaseUrl)
                : featured.Take(3).ToList();

            var newArrivals = await GetProductsAsync(
                new CatalogFilter { Sort = CatalogSort.Newest }, 1, 8, lang, mediaBaseUrl);

            var onSale = await GetProductsAsync(
                new CatalogFilter { OnSaleOnly = true }, 1, 8, lang, mediaBaseUrl);

            return new HomeDataDto
            {
                Categories = categories,
                Brands = brands,
                HeroProducts = heroProducts,
                FeaturedProducts = featured,
                NewArrivals = newArrivals.Items,
                OnSale = onSale.Items,
                Settings = settings
            };
        }

        /// <summary>
        /// Loads products by explicit id list, preserving the caller's ordering
        /// (the hero slider order is curated by the admin).
        /// </summary>
        private async Task<List<ProductListItemDto>> GetProductsByIdsAsync(
            List<int> ids, string lang, string? mediaBaseUrl)
        {
            if (ids.Count == 0) return new List<ProductListItemDto>();

            var entities = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive && ids.Contains(p.Id))
                .ToListAsync();

            var map = entities.ToDictionary(p => p.Id);
            var ordered = new List<ProductListItemDto>();
            foreach (var id in ids)
            {
                if (map.TryGetValue(id, out var entity))
                    ordered.Add(entity.ToListItem(lang, mediaBaseUrl));
            }
            return ordered;
        }

        /// <summary>Parses a "1,2,3" site-setting value, skipping malformed entries.</summary>
        private static List<int> ParseIdList(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return new List<int>();

            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(part => int.TryParse(part, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        public async Task<PublicSettingsDto> GetPublicSettingsAsync()
        {
            var settings = await _content.GetSettingsAsync();

            string Get(string key, string fallback)
                => settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                    ? value
                    : fallback;

            decimal GetDecimal(string key, decimal fallback)
                => decimal.TryParse(Get(key, string.Empty), out var value) ? value : fallback;

            return new PublicSettingsDto
            {
                SiteName = Get("site.name", "Uni-Shop"),
                TaglineAr = Get("site.tagline.ar", string.Empty),
                TaglineEn = Get("site.tagline.en", string.Empty),
                PrimaryColor = Get("site.primary.color", "#1A6BFF"),
                // Defaults mirror the seeded SiteSettings so a wiped settings
                // table cannot silently make shipping free.
                ShippingFee = GetDecimal("shipping.fee", 150m),
                FreeShippingAbove = GetDecimal("shipping.free.above", 5000m),
                Currency = "EGP",
                CurrencySymbolAr = "ج.م",
                CurrencySymbolEn = "EGP"
            };
        }
    }
}
