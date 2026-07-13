using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
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

            try
            {
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

                var attributeSelections = await ResolveAttributeSelectionsAsync(vm);
                await _productService.CreateProductAsync(vm.Product, additionalImages, attributeSelections);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.Categories = await _productService.GetActiveCategoriesAsync();
                var brandsRetry = await _productService.GetAllBrandsAsync();
                vm.Brands = brandsRetry.Where(b => b.IsActive).ToList();
                vm.Attributes = await _productService.GetAttributesWithDetailsAsync(null);
                return View("~/Views/Admin/Products/Form.cshtml", vm);
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

            try
            {
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

                var attributeSelections = await ResolveAttributeSelectionsAsync(vm);
                await _productService.UpdateProductAsync(id, vm.Product, additionalImages, attributeSelections);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                vm.Categories = await _productService.GetActiveCategoriesAsync();
                var brandsRetry = await _productService.GetAllBrandsAsync();
                vm.Brands = brandsRetry.Where(b => b.IsActive).ToList();
                vm.Attributes = await _productService.GetAttributesWithDetailsAsync(vm.Product.CategoryId);
                return View("~/Views/Admin/Products/Form.cshtml", vm);
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

        private async Task<string> SaveImageAsync(IFormFile file, string folder)
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

            var path = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(path);
            var name = $"{Guid.NewGuid()}{ext}";
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

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                    model.ImagePath = await SaveCategoryImageAsync(ImageFile);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("~/Views/Admin/Categories/Form.cshtml", model);
            }

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
        public async Task<IActionResult> Edit(int id, CategoryDto model, IFormFile? ImageFile)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");

            try
            {
                if (ImageFile != null && ImageFile.Length > 0)
                    model.ImagePath = await SaveCategoryImageAsync(ImageFile);
                else
                    model.ImagePath = null; // null ⇒ الخدمة تحتفظ بالصورة الحالية
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Id = id;
                return View("~/Views/Admin/Categories/Form.cshtml", model);
            }

            await _productService.UpdateCategoryAsync(id, model);
            TempData["Success"] = "تم تحديث التصنيف";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveCategoryImageAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException($"نوع الملف غير مدعوم. الأنواع المسموح بها: {string.Join(", ", allowedExtensions)}");

            const long maxBytes = 5 * 1024 * 1024;
            if (file.Length > maxBytes)
                throw new InvalidOperationException("حجم الصورة يتجاوز الحد المسموح به (5 ميغابايت)");

            var dir = Path.Combine(_env.WebRootPath, "images", "categories");
            Directory.CreateDirectory(dir);
            var name = $"{Guid.NewGuid()}{ext}";
            using var s = new FileStream(Path.Combine(dir, name), FileMode.Create);
            await file.CopyToAsync(s);
            return $"/images/categories/{name}";
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
            await _orders.UpdateStatusAsync(id, statusId);
            TempData["Success"] = "تم تحديث حالة الطلب";
            return RedirectToAction(nameof(Detail), new { id });
        }
    }

    // ── ADMIN CONTENT ─────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminContentController : Controller
    {
        private readonly IContentService _content;
        private readonly IProductService _products;
        public AdminContentController(IContentService content, IProductService products)
        { _content = content; _products = products; }

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

            TempData["Success"] = "تم حفظ إعدادات الأقسام";
            return RedirectToAction(nameof(Blocks), new { tab });
        }

        // تبديل ظهور فئة بالرئيسية بنقرة واحدة من تبويب الفئات
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCategoryHomepage(int id, string? tab)
        {
            var cats = await _products.GetAllCategoriesAsync();
            var cat = cats.FirstOrDefault(c => c.Id == id);
            if (cat != null)
            {
                cat.ShowOnHomepage = !cat.ShowOnHomepage;
                cat.ImagePath = null; // لا تغير الصورة
                await _products.UpdateCategoryAsync(id, cat);
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
        public AdminSettingsController(IContentService content) => _content = content;

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
