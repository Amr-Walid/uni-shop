using FTD.Application.Services;
using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _products;

        public ProductsController(IProductService products)
        {
            _products = products;
        }

        // GET /Products  or  /Products?brand=doogee  or  /Products?category=tablets
        public async Task<IActionResult> Index(
            string? brand, string? category, string? q, string? sort,
            [FromQuery(Name = "av")] List<int>? attrValues)
        {
            var brandsList = await _products.GetAllBrandsAsync();
            var categoriesList = await _products.GetActiveCategoriesAsync();

            var brands = brandsList.Where(b => b.IsActive).ToList();
            var categories = categoriesList;

            BrandDto? currentBrand = null;
            CategoryDto? currentCategory = null;

            if (!string.IsNullOrEmpty(brand))
            {
                currentBrand = brands.FirstOrDefault(b => b.Slug == brand.ToLower());
            }

            if (!string.IsNullOrEmpty(category))
            {
                currentCategory = categories.FirstOrDefault(c => c.Slug == category);
            }

            List<ProductDto> products;
            if (!string.IsNullOrEmpty(q))
                products = await _products.SearchAsync(q);
            else
                products = await _products.GetFilteredBySlugAsync(brand, category, attrValues, sort);

            // Build attribute filter groups from visible products
            var attrGroups = await _products.BuildAttributeGroupsAsync(products);

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
            var products = await _products.GetFilteredBySlugAsync(brand, category, attrValues, sort);
            var rawGroups = await _products.BuildAttributeGroupsAsync(products);

            var result = new
            {
                count = products.Count,
                products = products.Select(p => new
                {
                    id = p.Id,
                    slug = p.Slug,
                    nameAr = p.NameAr,
                    nameEn = p.NameEn,
                    shortDescAr = p.ShortDescAr,
                    shortDescEn = p.ShortDescEn,
                    brandName = p.Brand?.NameEn ?? p.BrandName,
                    price = p.Price,
                    oldPrice = p.OldPrice,
                    badge = p.Badge,
                    imagePath = p.ImagePath,
                    emoji = p.Emoji,
                    url = "/products/" + p.Slug
                }),
                attrGroups = rawGroups.Select(g => new
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
            var brandsList = await _products.GetAllBrandsAsync();
            var brand = brandsList.FirstOrDefault(b => b.Slug == brandSlug.ToLower() && b.IsActive);

            if (brand == null) return NotFound();

            var categories = await _products.GetActiveCategoriesAsync();

            var products = await _products.GetFilteredBySlugAsync(brand.Slug, null, null, "featured");
            var attrGroups = await _products.BuildAttributeGroupsAsync(products);

            var vm = new ProductsViewModel
            {
                Products = products,
                Brands = brandsList.Where(b => b.IsActive).ToList(),
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
    }
}
