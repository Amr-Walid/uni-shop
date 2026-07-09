using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminBrandsController : Controller
    {
        private readonly IAppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminBrandsController(IAppDbContext db, IWebHostEnvironment env)
        { _db = db; _env = env; }

        public async Task<IActionResult> Index()
        {
            var entities = await _db.Brands.OrderBy(b => b.SortOrder).ToListAsync();
            var dtos = entities.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList();
            return View("~/Views/Admin/Brands/Index.cshtml", dtos);
        }

        public IActionResult Create()
            => View("~/Views/Admin/Brands/Form.cshtml", new BrandDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandDto model,
            IFormFile? LogoFile, IFormFile? BannerFile)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");
            if (LogoFile != null) model.LogoPath = await SaveAsync(LogoFile, "brands");
            if (BannerFile != null) model.BannerPath = await SaveAsync(BannerFile, "brands/banners");

            var brand = new Brand
            {
                NameAr = model.NameAr,
                NameEn = model.NameEn,
                Slug = model.Slug,
                LogoPath = model.LogoPath,
                BannerPath = model.BannerPath,
                DescAr = model.DescAr,
                DescEn = model.DescEn,
                IsActive = model.IsActive,
                SortOrder = model.SortOrder,
                CreatedAt = DateTime.UtcNow
            };

            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة البراند";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _db.Brands.FindAsync(id);
            if (brand == null) return NotFound();
            return View("~/Views/Admin/Brands/Form.cshtml", brand.ToDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandDto model,
            IFormFile? LogoFile, IFormFile? LogoWhiteFile, IFormFile? BannerFile)
        {
            var brand = await _db.Brands.FindAsync(id);
            if (brand == null) return NotFound();
            brand.NameAr = model.NameAr;
            brand.NameEn = model.NameEn;
            brand.Slug = string.IsNullOrEmpty(model.Slug) ? model.NameEn.ToLower().Replace(" ", "-") : model.Slug;
            brand.DescAr = model.DescAr;
            brand.DescEn = model.DescEn;
            brand.SortOrder = model.SortOrder;
            brand.IsActive = model.IsActive;
            if (LogoFile != null) brand.LogoPath = await SaveAsync(LogoFile, "brands");
            else if (!string.IsNullOrEmpty(model.LogoPath)) brand.LogoPath = model.LogoPath;
          
            if (BannerFile != null) brand.BannerPath = await SaveAsync(BannerFile, "brands/banners");
            else if (!string.IsNullOrEmpty(model.BannerPath)) brand.BannerPath = model.BannerPath;
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تحديث البراند";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveAsync(IFormFile file, string folder)
        {
            var dir = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(dir);
            var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var s = new FileStream(Path.Combine(dir, name), FileMode.Create);
            await file.CopyToAsync(s);
            return $"/images/{folder}/{name}";
        }
    }
}
