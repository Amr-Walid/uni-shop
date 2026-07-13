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

            var brandSlugs = string.IsNullOrEmpty(brand) ? new List<string>() : brand.Split(',').Select(s => s.Trim().ToLower()).ToList();
            var categorySlugs = string.IsNullOrEmpty(category) ? new List<string>() : category.Split(',').Select(s => s.Trim().ToLower()).ToList();

            BrandDto? currentBrand = null;
            CategoryDto? currentCategory = null;

            if (brandSlugs.Any())
            {
                currentBrand = brands.FirstOrDefault(b => b.Slug == brandSlugs.First());
            }

            if (categorySlugs.Any())
            {
                currentCategory = categories.FirstOrDefault(c => c.Slug == categorySlugs.First());
            }

            List<ProductDto> products;
            List<AttributeFilterGroupDto> attrGroups;
            List<int> selectedAttrs;

            if (!string.IsNullOrEmpty(q))
            {
                products = await _products.SearchAsync(q);
                attrGroups = await _products.BuildAttributeGroupsAsync(products);
                selectedAttrs = new();
            }
            else
            {
                (products, attrGroups, selectedAttrs) =
                    await GetFacetedProductsAsync(brandSlugs, categorySlugs, attrValues, sort);
            }

            // Calculate dynamic facets (available categories and brands based on other active filters)
            var facets = await _products.GetAvailableFacetsAsync(brandSlugs, categorySlugs);

            // Ensure checked filters are kept in the lists so they don't disappear from the UI
            var displayCategories = facets.AvailableCategories;
            foreach (var checkedSlug in categorySlugs)
            {
                if (!displayCategories.Any(c => c.Slug == checkedSlug))
                {
                    var original = categories.FirstOrDefault(c => c.Slug == checkedSlug);
                    if (original != null) displayCategories.Add(original);
                }
            }

            var displayBrands = facets.AvailableBrands;
            foreach (var checkedSlug in brandSlugs)
            {
                if (!displayBrands.Any(b => b.Slug == checkedSlug))
                {
                    var original = brands.FirstOrDefault(b => b.Slug == checkedSlug);
                    if (original != null) displayBrands.Add(original);
                }
            }

            var vm = new ProductsViewModel
            {
                Products = products,
                Brands = displayBrands,
                Categories = displayCategories,
                CurrentBrand = currentBrand,
                CurrentCategory = currentCategory,
                BrandFilter = brand,
                CategoryFilter = category,
                SearchQuery = q,
                SortBy = sort,
                SelectedAttrValues = selectedAttrs,
                AttributeGroups = attrGroups,
                TotalCount = products.Count
            };

            return View(vm);
        }

        /// <summary>
        /// Faceted-filter pipeline shared by Index (SSR) and Filter (AJAX):
        /// 1. Base set   = products matching brand + category only.
        /// 2. Facets     = attribute groups built from the base set, so
        ///    selecting one option never hides its siblings.
        /// 3. Pruning    = drop selected attribute values that no longer
        ///    exist in the base set (e.g. after switching brand).
        /// 4. Result set = base set narrowed by the pruned attribute values.
        /// </summary>
        private async Task<(List<ProductDto> Products, List<AttributeFilterGroupDto> Groups, List<int> SelectedAttrs)>
            GetFacetedProductsAsync(List<string>? brandSlugs, List<string>? categorySlugs, List<int>? attrValues, string? sort)
        {
            var baseProducts = await _products.GetFilteredBySlugAsync(brandSlugs, categorySlugs, null, sort);
            var attrGroups = await _products.BuildAttributeGroupsAsync(baseProducts);

            var validValueIds = attrGroups
                .SelectMany(g => g.Options.Select(o => o.ValueId))
                .ToHashSet();

            var selected = (attrValues ?? new())
                .Where(v => validValueIds.Contains(v))
                .Distinct()
                .ToList();

            var products = selected.Any()
                ? await _products.GetFilteredBySlugAsync(brandSlugs, categorySlugs, selected, sort)
                : baseProducts;

            return (products, attrGroups, selected);
        }

        // GET /Products/Filter (AJAX)
        [HttpGet]
        public async Task<IActionResult> Filter(
            string? brand, string? category, string? sort,
            [FromQuery(Name = "av")] List<int>? attrValues)
        {
            var brandSlugs = string.IsNullOrEmpty(brand) ? new List<string>() : brand.Split(',').Select(s => s.Trim().ToLower()).ToList();
            var categorySlugs = string.IsNullOrEmpty(category) ? new List<string>() : category.Split(',').Select(s => s.Trim().ToLower()).ToList();

            var (products, rawGroups, selectedAttrs) =
                await GetFacetedProductsAsync(brandSlugs, categorySlugs, attrValues, sort);

            var facets = await _products.GetAvailableFacetsAsync(brandSlugs, categorySlugs);

            var brandsList = await _products.GetAllBrandsAsync();
            var categoriesList = await _products.GetActiveCategoriesAsync();

            // Ensure checked filters are kept in the lists so they don't disappear from the UI
            var displayCategories = facets.AvailableCategories;
            foreach (var checkedSlug in categorySlugs)
            {
                if (!displayCategories.Any(c => c.Slug == checkedSlug))
                {
                    var original = categoriesList.FirstOrDefault(c => c.Slug == checkedSlug);
                    if (original != null) displayCategories.Add(original);
                }
            }

            var displayBrands = facets.AvailableBrands;
            foreach (var checkedSlug in brandSlugs)
            {
                if (!displayBrands.Any(b => b.Slug == checkedSlug))
                {
                    var original = brandsList.FirstOrDefault(b => b.Slug == checkedSlug);
                    if (original != null) displayBrands.Add(original);
                }
            }

            var result = new
            {
                count = products.Count,
                selectedAttrValues = selectedAttrs,
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
                categories = displayCategories.Select(c => new
                {
                    slug = c.Slug,
                    nameAr = c.NameAr,
                    nameEn = c.NameEn
                }),
                brands = displayBrands.Select(b => new
                {
                    slug = b.Slug,
                    nameAr = b.NameAr,
                    nameEn = b.NameEn
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

            var products = await _products.GetFilteredBySlugAsync(new List<string> { brand.Slug }, null, null, "featured");
            var attrGroups = await _products.BuildAttributeGroupsAsync(products);

            var facets = await _products.GetAvailableFacetsAsync(new List<string> { brand.Slug }, null);

            var vm = new ProductsViewModel
            {
                Products = products,
                Brands = brandsList.Where(b => b.IsActive).ToList(),
                Categories = facets.AvailableCategories,
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
