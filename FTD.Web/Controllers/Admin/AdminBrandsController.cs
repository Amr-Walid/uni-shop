using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
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

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Brands/Form.cshtml", model);

            try
            {
                if (LogoFile != null && LogoFile.Length > 0)
                    model.LogoPath = await ImageUploadHelper.SaveAsync(LogoFile, _env, "brands");
                if (BannerFile != null && BannerFile.Length > 0)
                    model.BannerPath = await ImageUploadHelper.SaveAsync(BannerFile, _env, "brands/banners");

                await _productService.CreateBrandAsync(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/Brands/Form.cshtml", model);
            }

            TempData["Success"] = "تم إضافة البراند";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Single-row lookup instead of loading every brand into memory.
            var brand = await _productService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();
            return View("~/Views/Admin/Brands/Form.cshtml", brand);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandDto model,
            IFormFile? LogoFile, IFormFile? BannerFile)
        {
            // Single-row existence check instead of loading every brand.
            var brand = await _productService.GetBrandByIdAsync(id);
            if (brand == null) return NotFound();

            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");

            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("~/Views/Admin/Brands/Form.cshtml", model);
            }

            try
            {
                if (LogoFile != null && LogoFile.Length > 0)
                    model.LogoPath = await ImageUploadHelper.SaveAsync(LogoFile, _env, "brands");
                if (BannerFile != null && BannerFile.Length > 0)
                    model.BannerPath = await ImageUploadHelper.SaveAsync(BannerFile, _env, "brands/banners");

                await _productService.UpdateBrandAsync(id, model);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                ModelState.AddModelError("", ex.Message);
                model.Id = id;
                return View("~/Views/Admin/Brands/Form.cshtml", model);
            }

            TempData["Success"] = "تم تحديث البراند";
            return RedirectToAction(nameof(Index));
        }
    }
}
