using FTD.Application.Interfaces;
using FTD.Application.Services;
using FTD.Application.Mappers;
using FTD.Application.DTOs;
using FTD.Domain.Entities;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ProductService _products;
        private readonly IAppDbContext _db;

        public ProductsController(ProductService products, IAppDbContext db)
        {
            _products = products;
            _db = db;
        }

        // GET /Products  or  /Products?brand=doogee  or  /Products?category=tablets
        public async Task<IActionResult> Index(
            string? brand, string? category, string? q, string? sort,
            [FromQuery(Name = "av")] List<int>? attrValues)
        {
            var brandsList = await _db.Brands.Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync();
            var categoriesList = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();

            var brands = brandsList.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList();
            var categories = categoriesList.Select(c => c.ToDto()).Where(c => c != null).Select(c => c!).ToList();

            BrandDto? currentBrand = null;
            CategoryDto? currentCategory = null;

            if (!string.IsNullOrEmpty(brand))
            {
                var bEntity = await _db.Brands.FirstOrDefaultAsync(b => b.Slug == brand.ToLower());
                currentBrand = bEntity.ToDto();
            }

            if (!string.IsNullOrEmpty(category))
            {
                var cEntity = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == category);
                currentCategory = cEntity.ToDto();
            }

            List<ProductDto> products;
            if (!string.IsNullOrEmpty(q))
                products = await _products.SearchAsync(q);
            else
                products = await _products.GetFilteredAsync(brand, category, attrValues, sort);

            // Build attribute filter groups from visible products
            var attrGroups = await BuildAttributeGroupsAsync(products);

            var vm = new ProductsViewModel
            {
                Products = products,
                Brands = brands,
                Categories = categories,
                CurrentBrand = currentBrand,
                CurrentCategory = currentCategory,
                BrandFilter = brand,
                CategoryFilter = category,
                SearchQuery = q,
                SortBy = sort,
                SelectedAttrValues = attrValues ?? new(),
                AttributeGroups = attrGroups,
                TotalCount = products.Count
            };

            return View(vm);
        }

        // GET /Products/Filter (AJAX)
        [HttpGet]
        public async Task<IActionResult> Filter(
            string? brand, string? category, string? sort,
            [FromQuery(Name = "av")] List<int>? attrValues)
        {
            var products = await _products.GetFilteredAsync(brand, category, attrValues, sort);
            var attrGroups = await BuildAttributeGroupsAsync(products);

            var result = new
            {
                count = products.Count,
                products = products.Select(p => new
                {
                    id = p.Id,
                    slug = p.Slug,
                    nameAr = p.NameAr,
                    nameEn = p.NameEn,
                    brandName = p.Brand?.NameEn ?? p.BrandName,
                    price = p.Price,
                    oldPrice = p.OldPrice,
                    badge = p.Badge,
                    imagePath = p.ImagePath,
                    emoji = p.Emoji,
                    url = "/products/" + p.Slug
                }),
                attrGroups = attrGroups.Select(g => new
                {
                    attributeId = g.AttributeId,
                    nameAr = g.NameAr,
                    nameEn = g.NameEn,
                    options = g.Options.Select(o => new
                    {
                        valueId = o.ValueId,
                        valueAr = o.ValueAr,
                        valueEn = o.ValueEn,
                        count = o.Count
                    })
                })
            };

            return Json(result);
        }

        // GET /brand/{brandSlug}  e.g. /brand/doogee
        public async Task<IActionResult> BrandPage(string brandSlug)
        {
            var brandEntity = await _db.Brands
                .FirstOrDefaultAsync(b => b.Slug == brandSlug.ToLower() && b.IsActive);

            if (brandEntity == null) return NotFound();

            var brand = brandEntity.ToDto()!;

            // Build same ViewModel as Index but keep URL as /brand/{slug}
            var brandsList = await _db.Brands.Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync();
            var categoriesList = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();

            var brands = brandsList.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList();
            var categories = categoriesList.Select(c => c.ToDto()).Where(c => c != null).Select(c => c!).ToList();

            var products = await _products.GetFilteredAsync(brand.Slug, null, null, "featured");
            var attrGroups = await BuildAttributeGroupsAsync(products);

            var vm = new ProductsViewModel
            {
                Products = products,
                Brands = brands,
                Categories = categories,
                CurrentBrand = brand,
                BrandFilter = brand.Slug,
                AttributeGroups = attrGroups,
                TotalCount = products.Count
            };

            return View("~/Views/Products/Index.cshtml", vm);
        }

        // GET /Products/{slug}
        public async Task<IActionResult> Detail(string slug)
        {
            var product = await _products.GetBySlugAsync(slug);
            if (product == null) return NotFound();

            var related = await _products.GetRelatedAsync(product.Id, product.CategoryId, 4);
            var vm = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = related,
                Attributes = product.AttributeValues.ToList()
            };
            return View(vm);
        }

        // GET /Products/Search?q=... (AJAX live search)
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new { results = new object[] { } });

            var products = await _products.SearchAsync(q);
            var results = products.Take(8).Select(p => new
            {
                id = p.Id,
                slug = p.Slug,
                nameAr = p.NameAr,
                nameEn = p.NameEn,
                brand = p.Brand?.NameEn ?? p.BrandName,
                price = p.Price.ToString("N0") + " EGP",
                emoji = p.Emoji,
                url = "/products/" + p.Slug
            });
            return Json(new { results });
        }

        // Helper
        private async Task<List<AttributeFilterGroup>> BuildAttributeGroupsAsync(List<ProductDto> products)
        {
            if (!products.Any()) return new();

            var productIds = products.Select(p => p.Id).ToHashSet();

            // Fetch and filter in database - avoids fetching all records
            var pavs = await _db.ProductAttributeValues.Where(av => productIds.Contains(av.ProductId)).ToListAsync();

            if (!pavs.Any()) return new();

            var attrIds = pavs.Select(av => av.AttributeId).ToHashSet();
            var valIds = pavs.Select(av => av.AttributeValueId).ToHashSet();

            var attrs = await _db.ProductAttributes.ToListAsync();
            attrs = attrs.Where(a => attrIds.Contains(a.Id)).ToList();

            var vals = await _db.AttributeValues.ToListAsync();
            vals = vals.Where(v => valIds.Contains(v.Id)).ToList();

            // Build in memory
            var groups = new List<AttributeFilterGroup>();

            foreach (var attr in attrs.OrderBy(a => a.SortOrder))
            {
                var attrPavs = pavs.Where(av => av.AttributeId == attr.Id).ToList();
                if (!attrPavs.Any()) continue;

                var options = attrPavs
                    .GroupBy(av => av.AttributeValueId)
                    .Select(g => {
                        var val = vals.FirstOrDefault(v => v.Id == g.Key);
                        return val == null ? null : new AttributeFilterOption
                        {
                            ValueId = g.Key,
                            ValueAr = val.ValueAr,
                            ValueEn = val.ValueEn,
                            Count = g.Select(av => av.ProductId).Distinct().Count()
                        };
                    })
                    .Where(o => o != null)
                    .Cast<AttributeFilterOption>()
                    .OrderBy(o => o.ValueAr)
                    .ToList();

                if (options.Any())
                    groups.Add(new AttributeFilterGroup
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
