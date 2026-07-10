using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminBrandsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;

        public AdminBrandsController(IProductService productService, IWebHostEnvironment env)
        {
            _productService = productService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var dtos = await _productService.GetAllBrandsAsync();
            return View("~/Views/Admin/Brands/Index.cshtml", dtos);
        }

        public IActionResult Create()
            => View("~/Views/Admin/Brands/Form.cshtml", new BrandDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandDto model, IFormFile? LogoFile, IFormFile? BannerFile)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");
            if (LogoFile != null) model.LogoPath = await SaveAsync(LogoFile, "brands");
            if (BannerFile != null) model.BannerPath = await SaveAsync(BannerFile, "brands/banners");

            await _productService.CreateBrandAsync(model);
            TempData["Success"] = "تم إضافة البراند";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brands = await _productService.GetAllBrandsAsync();
            var brand = brands.FirstOrDefault(b => b.Id == id);
            if (brand == null) return NotFound();
            return View("~/Views/Admin/Brands/Form.cshtml", brand);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandDto model,
            IFormFile? LogoFile, IFormFile? BannerFile)
        {
            var brands = await _productService.GetAllBrandsAsync();
            var brand = brands.FirstOrDefault(b => b.Id == id);
            if (brand == null) return NotFound();

            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");

            if (LogoFile != null) model.LogoPath = await SaveAsync(LogoFile, "brands");
            if (BannerFile != null) model.BannerPath = await SaveAsync(BannerFile, "brands/banners");

            await _productService.UpdateBrandAsync(id, model);
            TempData["Success"] = "تم تحديث البراند";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveAsync(IFormFile file, string folder)
        {
            // Validate extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException($"نوع الملف غير مدعوم. الأنواع المسموح بها: {string.Join(", ", allowedExtensions)}");

            // Validate size (max 5 MB)
            const long maxBytes = 5 * 1024 * 1024;
            if (file.Length > maxBytes)
                throw new InvalidOperationException("حجم الصورة يتجاوز الحد المسموح به (5 ميغابايت)");

            var dir = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(dir);
            var name = $"{Guid.NewGuid()}{ext}";
            using var s = new FileStream(Path.Combine(dir, name), FileMode.Create);
            await file.CopyToAsync(s);
            return $"/images/{folder}/{name}";
        }
    }
}
