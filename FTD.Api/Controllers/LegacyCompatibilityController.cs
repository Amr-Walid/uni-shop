using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Api.Controllers
{
    /// <summary>
    /// DEPRECATED unversioned routes from the first API release, kept working so
    /// any integration built against them does not break when v1 ships.
    ///
    /// These forward to the same services as <c>/api/v1/*</c> but return the
    /// ORIGINAL heavyweight, unpaginated shapes, because changing the response
    /// of an existing route is exactly the breaking change versioning exists to
    /// avoid. New clients must use <c>/api/v1</c>.
    ///
    /// Every response carries <c>Deprecation</c> and <c>Link</c> headers
    /// pointing at the replacement route.
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)] // hidden from Swagger: not for new work
    public class LegacyCompatibilityController : ControllerBase
    {
        private readonly IProductService _products;

        public LegacyCompatibilityController(IProductService products) => _products = products;

        /// <summary>Legacy: full product list with no pagination.</summary>
        [HttpGet("api/products")]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int? categoryId, [FromQuery] int? brandId, [FromQuery] string? query)
        {
            MarkDeprecated("/api/v1/products");
            return Ok(await _products.GetFilteredByIdAsync(categoryId, brandId, query));
        }

        /// <summary>Legacy: product detail by slug.</summary>
        [HttpGet("api/products/{slug}")]
        public async Task<IActionResult> GetProductDetail(string slug)
        {
            MarkDeprecated("/api/v1/products/{slug}");

            var product = await _products.GetBySlugAsync(slug);
            return product == null
                ? ApiProblemFactory.NotFound(HttpContext, "المنتج غير موجود", ApiErrorCodes.ProductNotFound)
                : Ok(product);
        }

        /// <summary>Legacy: categories (now /api/v1/categories).</summary>
        [HttpGet("api/products/categories")]
        public async Task<IActionResult> GetCategories()
        {
            MarkDeprecated("/api/v1/categories");
            return Ok(await _products.GetActiveCategoriesAsync());
        }

        /// <summary>Legacy: brands (now /api/v1/brands).</summary>
        [HttpGet("api/products/brands")]
        public async Task<IActionResult> GetBrands()
        {
            MarkDeprecated("/api/v1/brands");

            var brands = await _products.GetAllBrandsAsync();
            return Ok(brands.FindAll(b => b.IsActive));
        }

        /// <summary>
        /// Advertises the deprecation using the standard HTTP headers, so a
        /// client's logs reveal the migration path without reading these docs.
        /// </summary>
        private void MarkDeprecated(string replacement)
        {
            Response.Headers["Deprecation"] = "true";
            Response.Headers["Link"] = $"<{replacement}>; rel=\"successor-version\"";
            Response.Headers["Warning"] =
                $"299 - \"This endpoint is deprecated. Use {replacement} instead.\"";
        }
    }
}
