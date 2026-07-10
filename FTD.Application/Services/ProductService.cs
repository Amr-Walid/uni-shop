using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class ProductService : IProductService
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

        public async Task<List<CategoryDto>> GetActiveCategoriesAsync()
        {
            var entities = await _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            return entities.Select(c => c.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
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
                // Fetch and filter in database - avoids fetching all records
                var pavs = await _db.ProductAttributeValues.Where(av => productIds.Contains(av.ProductId)).ToListAsync();

                products = products.Where(p =>
                    attributeValueIds.All(vid =>
                        pavs.Any(av => av.ProductId == p.Id && av.AttributeValueId == vid))
                ).ToList();
            }

            return products.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<List<ProductDto>> GetFilteredAsync(int? categoryId, int? brandId, string? query)
        {
            var queryable = _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                queryable = queryable.Where(p => p.CategoryId == categoryId.Value);
            }

            if (brandId.HasValue)
            {
                queryable = queryable.Where(p => p.BrandId == brandId.Value);
            }

            if (!string.IsNullOrEmpty(query))
            {
                var q = query.ToLower().Trim();
                queryable = queryable.Where(p =>
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
                );
            }

            queryable = queryable.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.SortOrder);

            var entities = await queryable.ToListAsync();
            return entities.Select(p => p.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
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

        public async Task<List<ProductDto>> GetAllProductsForAdminAsync()
        {
            var entities = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return entities.Select(p => p.ToDto()).Where(p => p != null).Select(p => p!).ToList();
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto dto, List<ProductImageDto> additionalImages, Dictionary<int, int> attributes)
        {
            var product = new Product
            {
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Slug = dto.Slug,
                ShortDescAr = dto.ShortDescAr,
                ShortDescEn = dto.ShortDescEn,
                DescAr = dto.DescAr,
                DescEn = dto.DescEn,
                Price = dto.Price,
                OldPrice = dto.OldPrice,
                Badge = dto.Badge,
                ImagePath = dto.ImagePath,
                BrandName = dto.BrandName,
                Emoji = dto.Emoji,
                IsActive = dto.IsActive,
                IsFeatured = dto.IsFeatured,
                SortOrder = dto.SortOrder,
                Stock = dto.Stock,
                MetaTitle = dto.MetaTitle,
                MetaDesc = dto.MetaDesc,
                CreatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            var addedImages = 0;
            foreach (var img in additionalImages)
            {
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImagePath = img.ImagePath,
                    IsMain = addedImages == 0 && string.IsNullOrEmpty(product.ImagePath),
                    SortOrder = addedImages
                });
                addedImages++;
            }

            foreach (var kv in attributes)
            {
                if (kv.Value > 0)
                {
                    _db.ProductAttributeValues.Add(new ProductAttributeValue
                    {
                        ProductId = product.Id,
                        AttributeId = kv.Key,
                        AttributeValueId = kv.Value
                    });
                }
            }

            await _db.SaveChangesAsync();

            return (await GetByIdAsync(product.Id))!;
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductDto dto, List<ProductImageDto> additionalImages, Dictionary<int, int> attributes)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) throw new ArgumentException("Product not found");

            product.NameAr = dto.NameAr;
            product.NameEn = dto.NameEn;
            product.Slug = dto.Slug;
            product.ShortDescAr = dto.ShortDescAr;
            product.ShortDescEn = dto.ShortDescEn;
            product.DescAr = dto.DescAr;
            product.DescEn = dto.DescEn;
            product.Price = dto.Price;
            product.OldPrice = dto.OldPrice;
            product.Badge = dto.Badge;
            product.BrandId = dto.BrandId;
            product.Emoji = dto.Emoji;
            product.CategoryId = dto.CategoryId;
            product.IsActive = dto.IsActive;
            product.IsFeatured = dto.IsFeatured;
            product.Stock = dto.Stock;
            product.SortOrder = dto.SortOrder;
            product.MetaTitle = dto.MetaTitle;
            product.MetaDesc = dto.MetaDesc;

            if (!string.IsNullOrEmpty(dto.ImagePath))
            {
                product.ImagePath = dto.ImagePath;
            }

            if (additionalImages.Any())
            {
                var currentCount = await _db.ProductImages.CountAsync(i => i.ProductId == id);
                var addedImages = 0;
                foreach (var img in additionalImages)
                {
                    _db.ProductImages.Add(new ProductImage
                    {
                        ProductId = id,
                        ImagePath = img.ImagePath,
                        IsMain = false,
                        SortOrder = currentCount + addedImages
                    });
                    addedImages++;
                }
            }

            // Update attributes
            var existing = _db.ProductAttributeValues.Where(av => av.ProductId == id);
            _db.ProductAttributeValues.RemoveRange(existing);

            foreach (var kv in attributes)
            {
                if (kv.Value > 0)
                {
                    _db.ProductAttributeValues.Add(new ProductAttributeValue
                    {
                        ProductId = id,
                        AttributeId = kv.Key,
                        AttributeValueId = kv.Value
                    });
                }
            }

            await _db.SaveChangesAsync();

            return (await GetByIdAsync(id))!;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return false;
            product.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ProductImageDto?> DeleteProductImageAsync(int imageId)
        {
            var img = await _db.ProductImages.FindAsync(imageId);
            if (img == null) return null;
            var dto = img.ToDto();
            _db.ProductImages.Remove(img);
            await _db.SaveChangesAsync();
            return dto;
        }

        public async Task<List<BrandDto>> GetAllBrandsAsync()
        {
            var entities = await _db.Brands.OrderBy(b => b.SortOrder).ToListAsync();
            return entities.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList();
        }

        public async Task<BrandDto> CreateBrandAsync(BrandDto dto)
        {
            var brand = new Brand
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Slug = dto.Slug,
                LogoPath = dto.LogoPath,
                BannerPath = dto.BannerPath,
                DescAr = dto.DescAr,
                DescEn = dto.DescEn,
                IsActive = dto.IsActive,
                SortOrder = dto.SortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
            return brand.ToDto()!;
        }

        public async Task<BrandDto> UpdateBrandAsync(int id, BrandDto dto)
        {
            var brand = await _db.Brands.FindAsync(id);
            if (brand == null) throw new ArgumentException("Brand not found");

            brand.NameAr = dto.NameAr;
            brand.NameEn = dto.NameEn;
            brand.Slug = dto.Slug;
            brand.DescAr = dto.DescAr;
            brand.DescEn = dto.DescEn;
            brand.SortOrder = dto.SortOrder;
            brand.IsActive = dto.IsActive;

            if (!string.IsNullOrEmpty(dto.LogoPath)) brand.LogoPath = dto.LogoPath;
            if (!string.IsNullOrEmpty(dto.BannerPath)) brand.BannerPath = dto.BannerPath;

            await _db.SaveChangesAsync();
            return brand.ToDto()!;
        }

        public async Task<List<CategoryDto>> GetAllCategoriesAsync()
        {
            var entities = await _db.Categories.Include(c => c.Products).OrderBy(c => c.SortOrder).ToListAsync();
            return entities.Select(c => c.ToDto()).Where(c => c != null).Select(c => c!).ToList();
        }

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto dto)
        {
            var category = new Category
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Slug = dto.Slug,
                Emoji = dto.Emoji,
                Description = dto.Description,
                ImagePath = dto.ImagePath,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            return category.ToDto()!;
        }

        public async Task<CategoryDto> UpdateCategoryAsync(int id, CategoryDto dto)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) throw new ArgumentException("Category not found");

            category.NameAr = dto.NameAr;
            category.NameEn = dto.NameEn;
            category.Slug = dto.Slug;
            category.Emoji = dto.Emoji;
            category.Description = dto.Description;
            category.ImagePath = dto.ImagePath;
            category.SortOrder = dto.SortOrder;
            category.IsActive = dto.IsActive;

            await _db.SaveChangesAsync();
            return category.ToDto()!;
        }

        public async Task<List<ProductAttributeDto>> GetAttributesWithDetailsAsync(int? categoryId)
        {
            var query = _db.ProductAttributes
                .Include(a => a.Values)
                .Include(a => a.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            var entities = await query.OrderBy(a => a.CategoryId).ThenBy(a => a.SortOrder).ToListAsync();
            return entities.Select(a => a.ToDto()).Where(a => a != null).Select(a => a!).ToList();
        }

        public async Task<ProductAttributeDto> CreateAttributeAsync(ProductAttributeDto dto)
        {
            var attr = new ProductAttribute
            {
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                CategoryId = dto.CategoryId,
                SortOrder = dto.SortOrder
            };

            _db.ProductAttributes.Add(attr);
            await _db.SaveChangesAsync();
            return (await _db.ProductAttributes.Include(a => a.Category).FirstOrDefaultAsync(a => a.Id == attr.Id)).ToDto()!;
        }

        public async Task<ProductAttributeDto> UpdateAttributeAsync(int id, ProductAttributeDto dto)
        {
            var attr = await _db.ProductAttributes.FindAsync(id);
            if (attr == null) throw new ArgumentException("Attribute not found");

            attr.NameAr = dto.NameAr;
            attr.NameEn = dto.NameEn;
            attr.CategoryId = dto.CategoryId;
            attr.SortOrder = dto.SortOrder;

            await _db.SaveChangesAsync();
            return (await _db.ProductAttributes.Include(a => a.Category).FirstOrDefaultAsync(a => a.Id == id)).ToDto()!;
        }

        public async Task<bool> DeleteAttributeAsync(int id)
        {
            var attr = await _db.ProductAttributes.FindAsync(id);
            if (attr == null) return false;
            _db.ProductAttributes.Remove(attr);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<AttributeValueDto> AddAttributeValueAsync(int attributeId, string valueAr, string valueEn)
        {
            var val = new AttributeValue
            {
                AttributeId = attributeId,
                ValueAr = valueAr.Trim(),
                ValueEn = string.IsNullOrWhiteSpace(valueEn) ? valueAr.Trim() : valueEn.Trim()
            };

            _db.AttributeValues.Add(val);
            await _db.SaveChangesAsync();
            return val.ToDto()!;
        }

        public async Task<bool> DeleteAttributeValueAsync(int valueId)
        {
            var val = await _db.AttributeValues.FindAsync(valueId);
            if (val == null) return false;
            _db.AttributeValues.Remove(val);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<AttributeFilterGroupDto>> BuildAttributeGroupsAsync(List<ProductDto> products)
        {
            if (products == null || !products.Any()) return new();

            var productIds = products.Select(p => p.Id).ToHashSet();

            var pavs = await _db.ProductAttributeValues.Where(av => productIds.Contains(av.ProductId)).ToListAsync();

            if (!pavs.Any()) return new();

            var attrIds = pavs.Select(av => av.AttributeId).ToHashSet();
            var valIds = pavs.Select(av => av.AttributeValueId).ToHashSet();

            var attrs = await _db.ProductAttributes.Where(a => attrIds.Contains(a.Id)).ToListAsync();
            var vals = await _db.AttributeValues.Where(v => valIds.Contains(v.Id)).ToListAsync();

            var groups = new List<AttributeFilterGroupDto>();

            foreach (var attr in attrs.OrderBy(a => a.SortOrder))
            {
                var attrPavs = pavs.Where(av => av.AttributeId == attr.Id).ToList();
                if (!attrPavs.Any()) continue;

                var options = attrPavs
                    .GroupBy(av => av.AttributeValueId)
                    .Select(g => {
                        var val = vals.FirstOrDefault(v => v.Id == g.Key);
                        return val == null ? null : new AttributeFilterOptionDto
                        {
                            ValueId = g.Key,
                            ValueAr = val.ValueAr,
                            ValueEn = val.ValueEn,
                            Count = g.Select(av => av.ProductId).Distinct().Count()
                        };
                    })
                    .Where(o => o != null)
                    .Cast<AttributeFilterOptionDto>()
                    .OrderBy(o => o.ValueAr)
                    .ToList();

                if (options.Any())
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
    }
}
