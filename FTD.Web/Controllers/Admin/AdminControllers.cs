using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using FTD.Application.Services;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace FTD.Web.Controllers.Admin
{
    // ── DASHBOARD ─────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;
        public DashboardController(DashboardService dashboardService) => _dashboardService = dashboardService;

        public async Task<IActionResult> Index()
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            
            var vm = new DashboardViewModel
            {
                TotalProducts = data.TotalProducts,
                TotalOrders = data.TotalOrders,
                NewOrders = data.NewOrders,
                PendingOrders = data.PendingOrders,
                TodayRevenue = data.TodayRevenue,
                MonthRevenue = data.MonthRevenue,
                RecentOrders = data.RecentOrders,
                OrdersByStatus = data.OrdersByStatus.Select(s => new OrderStatusCount
                {
                    StatusName = s.StatusName,
                    ColorHex = s.ColorHex,
                    Count = s.Count
                }).ToList()
            };
            return View("~/Views/Admin/Dashboard/Index.cshtml", vm);
        }
    }

    // ── ADMIN PRODUCTS ────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly ProductService _productService;
        private readonly IWebHostEnvironment _env;
        public AdminProductsController(ProductService productService, IWebHostEnvironment env)
        { _productService = productService; _env = env; }

        public async Task<IActionResult> Index()
        {
            var dtos = await _productService.GetAllProductsForAdminAsync();
            return View("~/Views/Admin/Products/Index.cshtml", dtos);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _productService.GetActiveCategoriesAsync();
            var brands = await _productService.GetAllBrandsAsync();
            var activeBrands = brands.Where(b => b.IsActive).ToList();
            var attributes = await _productService.GetAttributesWithDetailsAsync(null);

            var vm = new ProductFormViewModel
            {
                Categories = categories,
                Brands = activeBrands,
                Attributes = attributes
            };
            return View("~/Views/Admin/Products/Form.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel vm)
        {
            ModelState.Remove("Product.Category");
            ModelState.Remove("Product.AttributeValues");
            ModelState.Remove("Product.Images");

            if (string.IsNullOrEmpty(vm.Product.Slug))
                vm.Product.Slug = GenerateSlug(vm.Product.NameEn);

            if (vm.MainImage != null && vm.MainImage.Length > 0)
                vm.Product.ImagePath = await SaveImageAsync(vm.MainImage, "products");

            var additionalImages = new List<ProductImageDto>();
            if (vm.ProductImages != null)
                foreach (var img in vm.ProductImages.Take(3))
                    if (img.Length > 0)
                    {
                        var path = await SaveImageAsync(img, "products");
                        additionalImages.Add(new ProductImageDto { ImagePath = path });
                    }

            await _productService.CreateProductAsync(vm.Product, additionalImages, vm.SelectedAttributeValues);

            TempData["Success"] = "تم إضافة المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            var categories = await _productService.GetActiveCategoriesAsync();
            var brands = await _productService.GetAllBrandsAsync();
            var activeBrands = brands.Where(b => b.IsActive).ToList();
            var attributes = await _productService.GetAttributesWithDetailsAsync(product.CategoryId);

            var vm = new ProductFormViewModel
            {
                Product = product,
                Categories = categories,
                Brands = activeBrands,
                Attributes = attributes,
                ExistingImages = product.Images.OrderBy(i => i.SortOrder).ToList(),
                SelectedAttributeValues = product.AttributeValues
                    .Where(av => av.AttributeValue != null)
                    .ToDictionary(av => av.AttributeId, av => av.AttributeValueId)
            };
            return View("~/Views/Admin/Products/Form.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
        {
            ModelState.Remove("Product.Category");
            ModelState.Remove("Product.AttributeValues");
            ModelState.Remove("Product.Images");

            if (string.IsNullOrEmpty(vm.Product.Slug))
                vm.Product.Slug = GenerateSlug(vm.Product.NameEn);

            if (vm.MainImage != null && vm.MainImage.Length > 0)
                vm.Product.ImagePath = await SaveImageAsync(vm.MainImage, "products");

            var additionalImages = new List<ProductImageDto>();
            if (vm.ProductImages != null)
            {
                foreach (var img in vm.ProductImages.Take(3))
                    if (img.Length > 0)
                    {
                        var path = await SaveImageAsync(img, "products");
                        additionalImages.Add(new ProductImageDto { ImagePath = path });
                    }
            }

            await _productService.UpdateProductAsync(id, vm.Product, additionalImages, vm.SelectedAttributeValues);

            TempData["Success"] = "تم تحديث المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AttributesByCategory(int categoryId)
        {
            var attrs = await _productService.GetAttributesWithDetailsAsync(categoryId);
            var result = attrs.Select(a => new {
                id = a.Id,
                nameAr = a.NameAr,
                nameEn = a.NameEn,
                values = a.Values.Select(v => new { id = v.Id, valueAr = v.ValueAr, valueEn = v.ValueEn }).ToList()
            }).ToList();
            return Json(result);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteProductAsync(id);
            TempData["Success"] = "تم حذف المنتج";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var deletedImage = await _productService.DeleteProductImageAsync(id);
            if (deletedImage != null)
            {
                var filePath = Path.Combine(_env.WebRootPath, deletedImage.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                TempData["Success"] = "تم حذف الصورة";
                return RedirectToAction(nameof(Edit), new { id = deletedImage.ProductId });
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile file, string folder)
        {
            var path = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(path);
            var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using var s = new FileStream(Path.Combine(path, name), FileMode.Create);
            await file.CopyToAsync(s);
            return $"/images/{folder}/{name}";
        }

        private static string GenerateSlug(string text)
            => string.IsNullOrEmpty(text)
                ? Guid.NewGuid().ToString("N")[..8]
                : text.ToLower().Replace(" ", "-").Trim('-');
    }

    // ── ADMIN CATEGORIES ──────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly ProductService _productService;
        public AdminCategoriesController(ProductService productService) => _productService = productService;

        public async Task<IActionResult> Index()
        {
            var dtos = await _productService.GetAllCategoriesAsync();
            return View("~/Views/Admin/Categories/Index.cshtml", dtos);
        }

        public IActionResult Create()
            => View("~/Views/Admin/Categories/Form.cshtml", new CategoryDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto model)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");
            
            await _productService.CreateCategoryAsync(model);
            TempData["Success"] = "تم إضافة التصنيف";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _productService.GetAllCategoriesAsync();
            var item = cat.FirstOrDefault(c => c.Id == id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/Categories/Form.cshtml", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryDto model)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");
            await _productService.UpdateCategoryAsync(id, model);
            TempData["Success"] = "تم تحديث التصنيف";
            return RedirectToAction(nameof(Index));
        }
    }

    // ── ADMIN ORDERS ──────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly OrderService _orders;
        public AdminOrdersController(OrderService orders) => _orders = orders;

        public async Task<IActionResult> Index(int? statusId)
        {
            var dtos = await _orders.GetOrdersAsync(statusId);
            var statuses = await _orders.GetAllStatusesAsync();

            ViewBag.Statuses = statuses;
            ViewBag.CurrentStatus = statusId;
            return View("~/Views/Admin/Orders/Index.cshtml", dtos);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var orderDto = await _orders.GetOrderByIdAsync(id);
            if (orderDto == null) return NotFound();

            var statuses = await _orders.GetAllStatusesAsync();

            var vm = new OrderDetailViewModel
            {
                Order = orderDto,
                AllStatuses = statuses
            };
            return View("~/Views/Admin/Orders/Detail.cshtml", vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, int statusId)
        {
            await _orders.UpdateStatusAsync(id, statusId);
            TempData["Success"] = "تم تحديث حالة الطلب";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }

    // ── ADMIN CONTENT ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminContentController : Controller
    {
        private readonly ContentService _content;
        public AdminContentController(ContentService content) => _content = content;

        public async Task<IActionResult> Blocks()
        {
            var dtos = await _content.GetBlocksListAsync();
            return View("~/Views/Admin/Content/Blocks.cshtml", dtos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBlock(int id, string? bodyAr, string? bodyEn, string? titleAr)
        {
            await _content.SaveBlockAsync(id, bodyAr, bodyEn, titleAr);
            TempData["Success"] = "تم الحفظ";
            return RedirectToAction(nameof(Blocks));
        }

        public async Task<IActionResult> Pages()
        {
            var dtos = await _content.GetAllPagesAsync();
            return View("~/Views/Admin/Content/Pages.cshtml", dtos);
        }

        public IActionResult CreatePage()
            => View("~/Views/Admin/Content/PageForm.cshtml", new ContentPageDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePage(ContentPageDto model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Content/PageForm.cshtml", model);
            
            await _content.CreatePageAsync(model);
            TempData["Success"] = "تم إنشاء الصفحة";
            return RedirectToAction(nameof(Pages));
        }

        public async Task<IActionResult> EditPage(int id)
        {
            var page = await _content.GetPageWithSectionsAsync(id);
            if (page == null) return NotFound();
            return View("~/Views/Admin/Content/PageForm.cshtml", page);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPage(int id, ContentPageDto model)
        {
            await _content.UpdatePageAsync(id, model);
            TempData["Success"] = "تم تحديث الصفحة";
            return RedirectToAction(nameof(Pages));
        }
    }

    // ── ADMIN SETTINGS ────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminSettingsController : Controller
    {
        private readonly ContentService _content;
        public AdminSettingsController(ContentService content) => _content = content;

        public async Task<IActionResult> Index()
        {
            var contactInfo = await _content.GetContactInfoAsync();
            ViewBag.ContactInfo = contactInfo;
            
            var dtos = await _content.GetSettingsListAsync();
            return View("~/Views/Admin/Settings/Index.cshtml", dtos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(Dictionary<int, string> values)
        {
            await _content.SaveSiteSettingsAsync(values);
            TempData["Success"] = "تم حفظ الإعدادات";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveContact(ContactInfoDto model)
        {
            await _content.SaveContactInfoAsync(model);
            TempData["Success"] = "تم حفظ بيانات التواصل";
            return RedirectToAction(nameof(Index));
        }
    }

    // ── ADMIN ACCOUNT ─────────────────────────────────────────────────────────
    public class AdminAccountController : Controller
    {
        private readonly SignInManager<IdentityUser> _signIn;
        public AdminAccountController(SignInManager<IdentityUser> signIn) => _signIn = signIn;

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Admin/Account/Login.cshtml");
        }

        [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            var result = await _signIn.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: false);
            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/Admin/Dashboard");
            ModelState.AddModelError("", "بيانات الدخول غير صحيحة");
            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Admin/Account/Login.cshtml");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
