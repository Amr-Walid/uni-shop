using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using FTD.Web.Helpers;
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
using Microsoft.Extensions.Caching.Memory;

namespace FTD.Web.Controllers.Admin
{
    // ── HOME CACHE INVALIDATION ───────────────────────────────────────────
    // HomeController caches near-static reference data (settings / blocks /
    // contact / categories) for 60s. Admin saves must evict those keys so
    // dashboard edits are visible on the storefront immediately — otherwise
    // "حفظ" in the admin looks broken for up to a minute.
    internal static class HomeCache
    {
        public static readonly string[] Keys = { "home:settings", "home:categories", "home:blocks", "home:contact" };
        public static void Evict(IMemoryCache cache)
        {
            foreach (var k in Keys) cache.Remove(k);
        }
    }
    // ── DASHBOARD ─────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

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
                OrdersByStatus = data.OrdersByStatus
            };
            return View("~/Views/Admin/Dashboard/Index.cshtml", vm);
        }
    }

    // ── ADMIN PRODUCTS ────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;
        public AdminProductsController(IProductService productService, IWebHostEnvironment env)
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

            if (!ModelState.IsValid)
                return await RedisplayFormAsync(vm, null);

            try
            {
                if (vm.MainImage != null && vm.MainImage.Length > 0)
                    vm.Product.ImagePath = await ImageUploadHelper.SaveAsync(vm.MainImage, _env, "products");

                var additionalImages = new List<ProductImageDto>();
                if (vm.ProductImages != null)
                    foreach (var img in vm.ProductImages.Take(3))
                        if (img.Length > 0)
                        {
                            var path = await ImageUploadHelper.SaveAsync(img, _env, "products");
                            additionalImages.Add(new ProductImageDto { ImagePath = path });
                        }

                var attributeSelections = await ResolveAttributeSelectionsAsync(vm);
                await _productService.CreateProductAsync(vm.Product, additionalImages, attributeSelections);
            }
            // ArgumentException covers invalid references (e.g. stale CategoryId);
            // InvalidOperationException covers slug conflicts + upload validation.
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                ModelState.AddModelError("", ex.Message);
                return await RedisplayFormAsync(vm, null);
            }

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

            if (!ModelState.IsValid)
                return await RedisplayFormAsync(vm, vm.Product.CategoryId);

            try
            {
                if (vm.MainImage != null && vm.MainImage.Length > 0)
                    vm.Product.ImagePath = await ImageUploadHelper.SaveAsync(vm.MainImage, _env, "products");

                var additionalImages = new List<ProductImageDto>();
                if (vm.ProductImages != null)
                {
                    foreach (var img in vm.ProductImages.Take(3))
                        if (img.Length > 0)
                        {
                            var path = await ImageUploadHelper.SaveAsync(img, _env, "products");
                            additionalImages.Add(new ProductImageDto { ImagePath = path });
                        }
                }

                var attributeSelections = await ResolveAttributeSelectionsAsync(vm);
                await _productService.UpdateProductAsync(id, vm.Product, additionalImages, attributeSelections);
            }
            // ArgumentException covers "product not found" / invalid references;
            // InvalidOperationException covers slug conflicts + upload validation.
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                ModelState.AddModelError("", ex.Message);
                return await RedisplayFormAsync(vm, vm.Product.CategoryId);
            }

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

        // ── DUPLICATE PRODUCT ─────────────────────────────────────────────────
        // Clones the product (details + brand + category + price + stock + images
        // + all assigned attribute values) then redirects the admin straight to
        // the Edit form of the new copy to change the name/slug and save.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(int id)
        {
            try
            {
                var newId = await _productService.DuplicateProductAsync(id);
                TempData["Success"] = "تم نسخ المنتج بنجاح — عدّل الاسم والـ Slug ثم فعّله واحفظ";
                return RedirectToAction(nameof(Edit), new { id = newId });
            }
            catch (ArgumentException)
            {
                TempData["Error"] = "المنتج غير موجود";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── QUICK ADD ATTRIBUTE VALUE (inline AJAX from the product form) ────
        // Lets the admin create a missing spec value (e.g. a new color) without
        // leaving the Add/Edit Product screen. Returns the created value as JSON
        // so the client inserts + selects it in the dropdown instantly.
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickAddAttributeValue(int attributeId, string valueAr, string? valueEn)
        {
            if (attributeId <= 0 || string.IsNullOrWhiteSpace(valueAr))
                return BadRequest(new { success = false, message = "القيمة مطلوبة" });

            try
            {
                var created = await _productService.AddAttributeValueAsync(attributeId, valueAr, valueEn ?? "");
                return Json(new
                {
                    success = true,
                    id = created.Id,
                    attributeId = created.AttributeId,
                    valueAr = created.ValueAr,
                    valueEn = created.ValueEn
                });
            }
            catch (Exception)
            {
                return BadRequest(new { success = false, message = "تعذر إضافة القيمة" });
            }
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

        /// <summary>
        /// Merges dropdown selections with free-text spec inputs.
        /// Free-text values (attributes without predefined values) are created
        /// in the DB on the fly — AddAttributeValueAsync is idempotent, so
        /// retyping an existing value re-uses it instead of duplicating.
        /// A dropdown selection for the same attribute wins over free text.
        /// </summary>
        private async Task<Dictionary<int, int>> ResolveAttributeSelectionsAsync(ProductFormViewModel vm)
        {
            var selections = new Dictionary<int, int>(vm.SelectedAttributeValues);

            foreach (var kv in vm.FreeTextAttributeValues)
            {
                if (string.IsNullOrWhiteSpace(kv.Value)) continue;
                if (selections.TryGetValue(kv.Key, out var chosen) && chosen > 0) continue;

                try
                {
                    var created = await _productService.AddAttributeValueAsync(kv.Key, kv.Value.Trim(), kv.Value.Trim());
                    selections[kv.Key] = created.Id;
                }
                catch (ArgumentException)
                {
                    // Attribute was deleted while the form was open — skip silently
                }
            }

            return selections;
        }

        /// <summary>Re-populates the dropdown/list data and re-renders the product form.</summary>
        private async Task<IActionResult> RedisplayFormAsync(ProductFormViewModel vm, int? categoryId)
        {
            vm.Categories = await _productService.GetActiveCategoriesAsync();
            var brands = await _productService.GetAllBrandsAsync();
            vm.Brands = brands.Where(b => b.IsActive).ToList();
            vm.Attributes = await _productService.GetAttributesWithDetailsAsync(categoryId);
            return View("~/Views/Admin/Products/Form.cshtml", vm);
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
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _env;
        public AdminCategoriesController(IProductService productService, IWebHostEnvironment env)
        { _productService = productService; _env = env; }

        public async Task<IActionResult> Index()
        {
            var dtos = await _productService.GetAllCategoriesAsync();
            return View("~/Views/Admin/Categories/Index.cshtml", dtos);
        }

        public IActionResult Create()
            => View("~/Views/Admin/Categories/Form.cshtml", new CategoryDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto model, IFormFile? ImageFile)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");

            if (!ModelState.IsValid)
                return View("~/Views/Admin/Categories/Form.cshtml", model);

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                    model.ImagePath = await ImageUploadHelper.SaveAsync(ImageFile, _env, "categories");

                // The service throws InvalidOperationException on slug conflicts.
                await _productService.CreateCategoryAsync(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/Categories/Form.cshtml", model);
            }

            TempData["Success"] = "تم إضافة التصنيف";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            // Single-row lookup instead of loading every category into memory.
            var item = await _productService.GetCategoryByIdAsync(id);
            if (item == null) return NotFound();
            return View("~/Views/Admin/Categories/Form.cshtml", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryDto model, IFormFile? ImageFile)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");

            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View("~/Views/Admin/Categories/Form.cshtml", model);
            }

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                    model.ImagePath = await ImageUploadHelper.SaveAsync(ImageFile, _env, "categories");
                else
                    model.ImagePath = null; // null ⇒ الخدمة تحتفظ بالصورة الحالية

                // The service throws InvalidOperationException on slug conflicts
                // and ArgumentException if the category no longer exists.
                await _productService.UpdateCategoryAsync(id, model);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                ModelState.AddModelError("", ex.Message);
                model.Id = id;
                return View("~/Views/Admin/Categories/Form.cshtml", model);
            }

            TempData["Success"] = "تم تحديث التصنيف";
            return RedirectToAction(nameof(Index));
        }
    }

    // ── ADMIN ORDERS ──────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly IOrderService _orders;
        public AdminOrdersController(IOrderService orders) => _orders = orders;

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
            try
            {
                await _orders.UpdateStatusAsync(id, statusId);
                TempData["Success"] = "تم تحديث حالة الطلب";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Detail), new { id });
        }
    }

    // ── ADMIN CONTENT ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminContentController : Controller
    {
        private readonly IContentService _content;
        private readonly IProductService _products;
        private readonly IMemoryCache _cache;
        public AdminContentController(IContentService content, IProductService products, IMemoryCache cache)
        { _content = content; _products = products; _cache = cache; }

        public async Task<IActionResult> Blocks(string? tab)
        {
            var dtos = await _content.GetBlocksListAsync();
            ViewBag.Products = await _products.GetAllActiveAsync();
            ViewBag.Categories = await _products.GetAllCategoriesAsync();
            ViewBag.Settings = await _content.GetSettingsAsync();
            ViewBag.ContactInfo = await _content.GetContactInfoAsync();
            ViewBag.ActiveTab = tab ?? "hero";
            return View("~/Views/Admin/Content/Blocks.cshtml", dtos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBlock(int id, string? bodyAr, string? bodyEn, string? titleAr, string? tab)
        {
            await _content.SaveBlockAsync(id, bodyAr, bodyEn, titleAr);
            HomeCache.Evict(_cache);
            TempData["Success"] = "تم الحفظ";
            return RedirectToAction(nameof(Blocks), new { tab });
        }

        // حفظ مجموعة بلوكات دفعة واحدة بالـ Key (ينشئ المفقود تلقائياً)
        // أسماء الحقول: blocks[<key>].ar / blocks[<key>].en / blocks[<key>].icon
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBlocksBulk(Dictionary<string, BlockInput> blocks, string? tab)
        {
            if (blocks != null)
            {
                foreach (var kv in blocks)
                {
                    var key = kv.Key?.Trim();
                    if (string.IsNullOrEmpty(key)) continue;
                    await _content.SaveBlockByKeyAsync(key, kv.Value?.Ar, kv.Value?.En, kv.Value?.Icon);
                }
            }
            HomeCache.Evict(_cache);
            TempData["Success"] = "تم حفظ المحتوى";
            return RedirectToAction(nameof(Blocks), new { tab });
        }

        // حفظ اختيارات منتجات السلايدر والكتالوج (الترتيب يتبع ترتيب الاختيار)
        // scope = "hero" أو "featured" لتحديث مفتاح واحد فقط دون مسح الآخر
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveProductSelections(List<int>? heroProductIds, List<int>? featuredProductIds, string? scope, string? tab)
        {
            if (string.IsNullOrEmpty(scope) || scope == "hero")
            {
                var hero = heroProductIds != null ? string.Join(",", heroProductIds.Distinct()) : "";
                await _content.SaveSettingByKeyAsync("homepage.hero.products", hero, "منتجات سلايدر الهيرو (IDs مرتبة)");
            }
            if (string.IsNullOrEmpty(scope) || scope == "featured")
            {
                var featured = featuredProductIds != null ? string.Join(",", featuredProductIds.Distinct()) : "";
                await _content.SaveSettingByKeyAsync("homepage.featured.products", featured, "منتجات الكتالوج المميز (IDs مرتبة)");
            }

            HomeCache.Evict(_cache);
            TempData["Success"] = "تم حفظ اختيارات المنتجات";
            return RedirectToAction(nameof(Blocks), new { tab });
        }

        // حفظ مفاتيح إظهار/إخفاء الأقسام + ترتيب الأقسام + عدد بلاطات الفئات
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveHomepageSettings(List<string>? visibleSections, string? sectionsOrder, string? categoriesCount, string? tab)
        {
            var all = new[] { "hero", "values", "categories", "featured", "about", "mission", "cta", "contact", "newsletter", "payments" };
            var visible = visibleSections ?? new List<string>();
            foreach (var section in all)
            {
                await _content.SaveSettingByKeyAsync(
                    $"homepage.show.{section}",
                    visible.Contains(section) ? "1" : "0",
                    null, "bool");
            }

            if (!string.IsNullOrWhiteSpace(sectionsOrder))
                await _content.SaveSettingByKeyAsync("homepage.sections.order", sectionsOrder.Trim(), "ترتيب أقسام الرئيسية");

            if (!string.IsNullOrWhiteSpace(categoriesCount) && int.TryParse(categoriesCount, out var cc) && cc > 0)
                await _content.SaveSettingByKeyAsync("homepage.categories.count", cc.ToString(), "عدد بلاطات الفئات بالرئيسية");

            HomeCache.Evict(_cache);
            TempData["Success"] = "تم حفظ إعدادات الأقسام";
            return RedirectToAction(nameof(Blocks), new { tab });
        }

        // تبديل ظهور فئة بالرئيسية بنقرة واحدة من تبويب الفئات
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryHomepage(int id, string? tab)
        {
            var cat = await _products.GetCategoryByIdAsync(id);
            if (cat != null)
            {
                cat.ShowOnHomepage = !cat.ShowOnHomepage;
                cat.ImagePath = null; // لا تغير الصورة
                await _products.UpdateCategoryAsync(id, cat);
                HomeCache.Evict(_cache);
                TempData["Success"] = cat.ShowOnHomepage ? "ستظهر الفئة بالرئيسية" : "تم إخفاء الفئة من الرئيسية";
            }
            return RedirectToAction(nameof(Blocks), new { tab = tab ?? "sections" });
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
            
            var created = await _content.CreatePageAsync(model);
            TempData["Success"] = "تم إنشاء الصفحة — أضف الأقسام من هنا";
            return RedirectToAction(nameof(EditPage), new { id = created.Id });
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

            // AJAX save from the visual builder → JSON, no reload
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            TempData["Success"] = "تم تحديث الصفحة";
            return RedirectToAction(nameof(EditPage), new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePage(int id)
        {
            await _content.DeletePageAsync(id);
            TempData["Success"] = "تم حذف الصفحة بنجاح";
            return RedirectToAction(nameof(Pages));
        }
    }

    // ── ADMIN SETTINGS ────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminSettingsController : Controller
    {
        private readonly IContentService _content;
        private readonly IMemoryCache _cache;
        public AdminSettingsController(IContentService content, IMemoryCache cache)
        { _content = content; _cache = cache; }

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
            HomeCache.Evict(_cache);
            TempData["Success"] = "تم حفظ الإعدادات";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveContact(ContactInfoDto model)
        {
            await _content.SaveContactInfoAsync(model);
            HomeCache.Evict(_cache);
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
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("admin-login-policy")]
        public async Task<IActionResult> Login(string email, string password, string? returnUrl = null)
        {
            // lockoutOnFailure: true — enables Identity's built-in account lockout after
            // repeated failed attempts (configured via options.Lockout in Program.cs),
            // protecting against brute-force attacks on the admin login endpoint.
            var result = await _signIn.PasswordSignInAsync(email, password, isPersistent: true, lockoutOnFailure: true);
            if (result.Succeeded)
                return LocalRedirect(returnUrl ?? "/Admin/Dashboard");

            if (result.IsLockedOut)
                ModelState.AddModelError("", "تم قفل الحساب مؤقتاً بسبب محاولات دخول فاشلة متكررة. حاول مرة أخرى بعد بضع دقائق.");
            else
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
