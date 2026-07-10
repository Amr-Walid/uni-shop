### Task 4: Implement Public Catalog Endpoints (Products, Categories, Brands)
**Files:**
- Create: `FTD.Api/Controllers/ProductsController.cs`

**Interfaces:**
- Consumes: `IProductService`, `IContentService`
- Produces: API endpoints `GET /api/products`, `GET /api/products/{slug}`, `GET /api/categories`, `GET /api/brands`.

- [ ] **Step 1: Create ProductsController**
  Create `FTD.Api/Controllers/ProductsController.cs`:
  ```csharp
  using FTD.Application.Interfaces;
  using Microsoft.AspNetCore.Mvc;

  namespace FTD.Api.Controllers
  {
      [ApiController]
      [Route("api/[controller]")]
      public class ProductsController : ControllerBase
      {
          private readonly IProductService _productService;
          public ProductsController(IProductService productService) => _productService = productService;

          [HttpGet]
          public async Task<IActionResult> GetProducts([FromQuery] int? categoryId, [FromQuery] int? brandId, [FromQuery] string? query)
          {
              var products = await _productService.GetFilteredAsync(categoryId, brandId, query);
              return Ok(products);
          }

          [HttpGet("{slug}")]
          public async Task<IActionResult> GetProductDetail(string slug)
          {
              var product = await _productService.GetBySlugAsync(slug);
              if (product == null)
                  return NotFound("المنتج غير موجود");
              return Ok(product);
          }

          [HttpGet("categories")]
          public async Task<IActionResult> GetCategories()
          {
              var categories = await _productService.GetActiveCategoriesAsync();
              return Ok(categories);
          }

          [HttpGet("brands")]
          public async Task<IActionResult> GetBrands()
          {
              var brands = await _productService.GetAllBrandsAsync();
              var activeBrands = brands.Where(b => b.IsActive).ToList();
              return Ok(activeBrands);
          }
      }
  }
  ```

- [ ] **Step 2: Verify Build**
  Run: `dotnet build FTD.Api/FTD.Api.csproj`
  Expected: Build succeeds with 0 errors.

- [ ] **Step 3: Commit Changes**
  Run:
  ```bash
  git add FTD.Api/Controllers/ProductsController.cs
  git commit -m "feat: implement public ProductsController api endpoints for catalog browsing"
  ```
