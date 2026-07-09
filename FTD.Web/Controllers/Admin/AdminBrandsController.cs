using FTD.Web.Data;
using FTD.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminBrandsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminBrandsController(AppDbContext db, IWebHostEnvironment env)
        { _db = db; _env = env; }

        public async Task<IActionResult> Index()
            => View("~/Views/Admin/Brands/Index.cshtml",
                await _db.Brands.Include(b => b.Products).OrderBy(b => b.SortOrder).ToListAsync());

        public IActionResult Create()
            => View("~/Views/Admin/Brands/Form.cshtml", new Brand());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand model,
            IFormFile? LogoFile, IFormFile? BannerFile)
        {
            ModelState.Remove("Products");
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");
            if (LogoFile != null) model.LogoPath = await SaveAsync(LogoFile, "brands");
            if (BannerFile != null) model.BannerPath = await SaveAsync(BannerFile, "brands/banners");
            model.CreatedAt = DateTime.UtcNow;
            _db.Brands.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة البراند";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _db.Brands.FindAsync(id);
            if (brand == null) return NotFound();
            return View("~/Views/Admin/Brands/Form.cshtml", brand);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Brand model,
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
