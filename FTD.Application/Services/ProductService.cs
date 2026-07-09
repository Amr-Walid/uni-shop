using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class ProductService
    {
        private readonly IAppDbContext _db;
        public ProductService(IAppDbContext db) => _db = db;

        public async Task<List<ProductDto>> GetFeaturedAsync(int take = 6)
        {
            var entities = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderBy(p => p.SortOrder)
                .Take(take)
                .ToListAsync();

            return entities.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<List<ProductDto>> GetAllActiveAsync()
        {
            var entities = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category.SortOrder).ThenBy(p => p.SortOrder)
                .ToListAsync();

            return entities.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<List<ProductDto>> GetByCategoryAsync(int categoryId)
        {
            var entities = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == categoryId)
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            return entities.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<List<ProductDto>> GetByBrandAsync(string brandSlug)
        {
            var entities = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive && (
                    (p.Brand != null && p.Brand.Slug.ToLower() == brandSlug.ToLower()) ||
                    (p.BrandName != null && p.BrandName.ToLower() == brandSlug.ToLower())
                ))
                .OrderBy(p => p.SortOrder)
                .ToListAsync();

            return entities.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        // Filtered query for products page with AJAX filters
        public async Task<List<ProductDto>> GetFilteredAsync(
            string? brandSlug, string? categorySlug,
            List<int>? attributeValueIds, string? sortBy)
        {
            // Step 1: simple query WITHOUT Include(AttributeValues) to avoid CTE
            var query = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(brandSlug))
                query = query.Where(p =>
                    (p.Brand != null && p.Brand.Slug.ToLower() == brandSlug.ToLower()) ||
                    (p.BrandName != null && p.BrandName.ToLower() == brandSlug.ToLower()));

            if (!string.IsNullOrEmpty(categorySlug))
                query = query.Where(p => p.Category.Slug == categorySlug);

            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.SortOrder)
            };

            var products = await query.ToListAsync();

            // Step 2: filter by attributes if needed (fetch separately to avoid CTE)
            if (attributeValueIds != null && attributeValueIds.Any())
            {
                var productIds = products.Select(p => p.Id).ToHashSet();
                // Fetch all then filter in memory - avoids CTE error
                var allPavs = await _db.ProductAttributeValues.ToListAsync();
                var pavs = allPavs.Where(av => productIds.Contains(av.ProductId)).ToList();

                products = products.Where(p =>
                    attributeValueIds.All(vid =>
                        pavs.Any(av => av.ProductId == p.Id && av.AttributeValueId == vid))
                ).ToList();
            }

            return products.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<ProductDto?> GetBySlugAsync(string slug)
        {
            var entity = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.AttributeValue)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            return entity.ToDto();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var entity = await _db.Products
                .Include(p => p.Category)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.Attribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.AttributeValue)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            return entity.ToDto();
        }

        public async Task<List<ProductDto>> GetRelatedAsync(int productId, int categoryId, int take = 4)
        {
            var entities = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CategoryId == categoryId && p.Id != productId)
                .Take(take)
                .ToListAsync();

            return entities.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<List<ProductDto>> SearchAsync(string query)
        {
            var q = query.ToLower().Trim();
            var entities = await _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && (
                    p.NameAr.ToLower().Contains(q) ||
                    p.NameEn.ToLower().Contains(q) ||
                    (p.BrandName != null && p.BrandName.ToLower().Contains(q)) ||
                    (p.ShortDescAr != null && p.ShortDescAr.ToLower().Contains(q)) ||
                    (p.ShortDescEn != null && p.ShortDescEn.ToLower().Contains(q)) ||
                    (p.DescAr != null && p.DescAr.ToLower().Contains(q)) ||
                    (p.DescEn != null && p.DescEn.ToLower().Contains(q)) ||
                    (p.Badge != null && p.Badge.ToLower().Contains(q)) ||
                    (p.Category != null && p.Category.NameAr.ToLower().Contains(q)) ||
                    (p.Category != null && p.Category.NameEn.ToLower().Contains(q))
                ))
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();

            return entities.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }
    }
}
