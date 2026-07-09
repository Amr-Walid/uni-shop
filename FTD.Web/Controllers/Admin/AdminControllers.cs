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
        private readonly IAppDbContext _db;
        public DashboardController(IAppDbContext db) => _db = db;
        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            
            var recentOrders = await _db.SalesOrders
                .Include(o => o.Status)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10).ToListAsync();

            var vm = new DashboardViewModel
            {
                TotalProducts = await _db.Products.CountAsync(p => p.IsActive),
                TotalOrders = await _db.SalesOrders.CountAsync(),
                NewOrders = await _db.SalesOrders.CountAsync(o => o.StatusId == 1),
                PendingOrders = await _db.SalesOrders.CountAsync(o => o.StatusId == 3 || o.StatusId == 4),
                TodayRevenue = await _db.SalesOrders
                    .Where(o => o.CreatedAt.Date == today && o.StatusId != 7)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                MonthRevenue = await _db.SalesOrders
                    .Where(o => o.CreatedAt >= monthStart && o.StatusId != 7)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                RecentOrders = recentOrders.Select(o => o.ToDto()).Where(o => o != null).Select(o => o!).ToList(),
                OrdersByStatus = await _db.OrderStatuses
                    .Select(s => new OrderStatusCount
                    {
                        StatusName = s.NameAr,
                        ColorHex = s.ColorHex,
                        Count = s.Orders.Count
                    }).ToListAsync()
            };
            return View("~/Views/Admin/Dashboard/Index.cshtml", vm);
        }
    }

    // ── ADMIN PRODUCTS ────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminProductsController : Controller
    {
        private readonly IAppDbContext _db;
        private readonly IWebHostEnvironment _env;
        public AdminProductsController(IAppDbContext db, IWebHostEnvironment env)
        { _db = db; _env = env; }

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            var dtos = products.Select(p => p.ToDto()).Where(p => p != null).Select(p => p!).ToList();
            return View("~/Views/Admin/Products/Index.cshtml", dtos);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            var brands = await _db.Brands.Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync();
            var attributes = await _db.ProductAttributes.Include(a => a.Values).OrderBy(a => a.SortOrder).ToListAsync();

            var vm = new ProductFormViewModel
            {
                Categories = categories.Select(c => c.ToDto()).Where(c => c != null).Select(c => c!).ToList(),
                Brands = brands.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList(),
                Attributes = attributes.Select(a => a.ToDto()).Where(a => a != null).Select(a => a!).ToList()
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

            var product = new Product
            {
                CategoryId = vm.Product.CategoryId,
                BrandId = vm.Product.BrandId,
                NameAr = vm.Product.NameAr,
                NameEn = vm.Product.NameEn,
                Slug = vm.Product.Slug,
                ShortDescAr = vm.Product.ShortDescAr,
                ShortDescEn = vm.Product.ShortDescEn,
                DescAr = vm.Product.DescAr,
                DescEn = vm.Product.DescEn,
                Price = vm.Product.Price,
                OldPrice = vm.Product.OldPrice,
                Badge = vm.Product.Badge,
                ImagePath = vm.Product.ImagePath,
                BrandName = vm.Product.BrandName,
                Emoji = vm.Product.Emoji,
                IsActive = vm.Product.IsActive,
                IsFeatured = vm.Product.IsFeatured,
                SortOrder = vm.Product.SortOrder,
                Stock = vm.Product.Stock,
                MetaTitle = vm.Product.MetaTitle,
                MetaDesc = vm.Product.MetaDesc,
                CreatedAt = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            var addedImages = 0;
            if (vm.ProductImages != null)
                foreach (var img in vm.ProductImages.Take(3))
                    if (img.Length > 0)
                    {
                        var path = await SaveImageAsync(img, "products");
                        _db.ProductImages.Add(new ProductImage
                        {
                            ProductId = product.Id,
                            ImagePath = path,
                            IsMain = addedImages == 0 && string.IsNullOrEmpty(product.ImagePath),
                            SortOrder = addedImages
                        });
                        addedImages++;
                    }

            foreach (var kv in vm.SelectedAttributeValues)
                if (kv.Value > 0)
                    _db.ProductAttributeValues.Add(new ProductAttributeValue
                    {
                        ProductId = product.Id,
                        AttributeId = kv.Key,
                        AttributeValueId = kv.Value
                    });
            await _db.SaveChangesAsync();

            TempData["Success"] = "تم إضافة المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products
                .Include(p => p.AttributeValues)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();

            var attrQuery = _db.ProductAttributes.Include(a => a.Values).AsQueryable();
            if (product.CategoryId > 0)
                attrQuery = attrQuery.Where(a => a.CategoryId == product.CategoryId);

            var categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            var brands = await _db.Brands.Where(b => b.IsActive).OrderBy(b => b.SortOrder).ToListAsync();
            var attributes = await attrQuery.OrderBy(a => a.SortOrder).ToListAsync();

            var vm = new ProductFormViewModel
            {
                Product = product.ToDto()!,
                Categories = categories.Select(c => c.ToDto()).Where(c => c != null).Select(c => c!).ToList(),
                Brands = brands.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList(),
                Attributes = attributes.Select(a => a.ToDto()).Where(a => a != null).Select(a => a!).ToList(),
                ExistingImages = product.Images.OrderBy(i => i.SortOrder).Select(i => i.ToDto()!).ToList(),
                SelectedAttributeValues = product.AttributeValues
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

            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.NameAr = vm.Product.NameAr;
            product.NameEn = vm.Product.NameEn;
            product.Slug = string.IsNullOrEmpty(vm.Product.Slug) ? GenerateSlug(vm.Product.NameEn) : vm.Product.Slug;
            product.ShortDescAr = vm.Product.ShortDescAr;
            product.ShortDescEn = vm.Product.ShortDescEn;
            product.DescAr = vm.Product.DescAr;
            product.DescEn = vm.Product.DescEn;
            product.Price = vm.Product.Price;
            product.OldPrice = vm.Product.OldPrice;
            product.Badge = vm.Product.Badge;
            product.BrandId = vm.Product.BrandId;
            product.Emoji = vm.Product.Emoji;
            product.CategoryId = vm.Product.CategoryId;
            product.IsActive = vm.Product.IsActive;
            product.IsFeatured = vm.Product.IsFeatured;
            product.Stock = vm.Product.Stock;
            product.SortOrder = vm.Product.SortOrder;
            product.MetaTitle = vm.Product.MetaTitle;
            product.MetaDesc = vm.Product.MetaDesc;

            // Save main image
            if (vm.MainImage != null && vm.MainImage.Length > 0)
                product.ImagePath = await SaveImageAsync(vm.MainImage, "products");

            // Save additional images
            if (vm.ProductImages != null)
            {
                var currentCount = await _db.ProductImages.CountAsync(i => i.ProductId == id);
                var addedImages = 0;
                foreach (var img in vm.ProductImages.Take(3))
                    if (img.Length > 0)
                    {
                        var path = await SaveImageAsync(img, "products");
                        _db.ProductImages.Add(new ProductImage
                        {
                            ProductId = id,
                            ImagePath = path,
                            IsMain = false,
                            SortOrder = currentCount + addedImages
                        });
                        addedImages++;
                    }
            }

            // Update attributes
            var existing = _db.ProductAttributeValues.Where(av => av.ProductId == id);
            _db.ProductAttributeValues.RemoveRange(existing);
            foreach (var kv in vm.SelectedAttributeValues)
                if (kv.Value > 0)
                    _db.ProductAttributeValues.Add(new ProductAttributeValue
                    {
                        ProductId = id,
                        AttributeId = kv.Key,
                        AttributeValueId = kv.Value
                    });

            await _db.SaveChangesAsync();

            TempData["Success"] = "تم تحديث المنتج بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AttributesByCategory(int categoryId)
        {
            var attrs = await _db.ProductAttributes
                .Where(a => a.CategoryId == categoryId)
                .Include(a => a.Values)
                .OrderBy(a => a.SortOrder)
                .Select(a => new {
                    id = a.Id,
                    nameAr = a.NameAr,
                    nameEn = a.NameEn,
                    values = a.Values.Select(v => new { id = v.Id, valueAr = v.ValueAr, valueEn = v.ValueEn }).ToList()
                })
                .ToListAsync();
            return Json(attrs);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null) { product.IsActive = false; await _db.SaveChangesAsync(); }
            TempData["Success"] = "تم حذف المنتج";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var img = await _db.ProductImages.FindAsync(id);
            if (img != null)
            {
                var productId = img.ProductId;
                var filePath = Path.Combine(_env.WebRootPath, img.ImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                _db.ProductImages.Remove(img);
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم حذف الصورة";
                return RedirectToAction(nameof(Edit), new { id = productId });
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
        private readonly IAppDbContext _db;
        public AdminCategoriesController(IAppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories.Include(c => c.Products).OrderBy(c => c.SortOrder).ToListAsync();
            var dtos = categories.Select(c => c.ToDto()).Where(c => c != null).Select(c => c!).ToList();
            return View("~/Views/Admin/Categories/Index.cshtml", dtos);
        }

        public IActionResult Create()
            => View("~/Views/Admin/Categories/Form.cshtml", new CategoryDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryDto model)
        {
            if (string.IsNullOrEmpty(model.Slug))
                model.Slug = model.NameEn.ToLower().Replace(" ", "-");
            
            var category = new Category
            {
                NameAr = model.NameAr,
                NameEn = model.NameEn,
                Slug = model.Slug,
                Emoji = model.Emoji,
                Description = model.Description,
                ImagePath = model.ImagePath,
                SortOrder = model.SortOrder,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة التصنيف";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View("~/Views/Admin/Categories/Form.cshtml", cat.ToDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryDto model)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            cat.NameAr = model.NameAr;
            cat.NameEn = model.NameEn;
            cat.Slug = string.IsNullOrEmpty(model.Slug) ? model.NameEn.ToLower().Replace(" ", "-") : model.Slug;
            cat.Emoji = model.Emoji;
            cat.Description = model.Description;
            cat.ImagePath = model.ImagePath;
            cat.SortOrder = model.SortOrder;
            cat.IsActive = model.IsActive;
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تحديث التصنيف";
            return RedirectToAction(nameof(Index));
        }
    }

    // ── ADMIN ORDERS ──────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : Controller
    {
        private readonly IAppDbContext _db;
        private readonly OrderService _orders;
        public AdminOrdersController(IAppDbContext db, OrderService orders)
        { _db = db; _orders = orders; }

        public async Task<IActionResult> Index(int? statusId)
        {
            var query = _db.SalesOrders.Include(o => o.Status).AsQueryable();
            if (statusId.HasValue) query = query.Where(o => o.StatusId == statusId.Value);
            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            var dtos = orders.Select(o => o.ToDto()).Where(o => o != null).Select(o => o!).ToList();

            var statuses = await _db.OrderStatuses.OrderBy(s => s.SortOrder).ToListAsync();
            ViewBag.Statuses = statuses.Select(s => s.ToDto()).Where(s => s != null).Select(s => s!).ToList();
            ViewBag.CurrentStatus = statusId;
            return View("~/Views/Admin/Orders/Index.cshtml", dtos);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var order = await _db.SalesOrders
                .Include(o => o.Status)
                .Include(o => o.Details).ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();

            var statuses = await _db.OrderStatuses.OrderBy(s => s.SortOrder).ToListAsync();

            var vm = new OrderDetailViewModel
            {
                Order = order.ToDto()!,
                AllStatuses = statuses.Select(s => s.ToDto()).Where(s => s != null).Select(s => s!).ToList()
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
        private readonly IAppDbContext _db;
        public AdminContentController(IAppDbContext db) => _db = db;

        public async Task<IActionResult> Blocks()
        {
            var blocks = await _db.ContentBlocks.OrderBy(b => b.Key).ToListAsync();
            var dtos = blocks.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList();
            return View("~/Views/Admin/Content/Blocks.cshtml", dtos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBlock(int id, string? bodyAr, string? bodyEn, string? titleAr)
        {
            var block = await _db.ContentBlocks.FindAsync(id);
            if (block != null)
            {
                block.BodyAr = bodyAr;
                block.BodyEn = bodyEn;
                if (titleAr != null) block.TitleAr = titleAr;
                block.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "تم الحفظ";
            return RedirectToAction(nameof(Blocks));
        }

        public async Task<IActionResult> Pages()
        {
            var pages = await _db.ContentPages.OrderBy(p => p.Slug).ToListAsync();
            var dtos = pages.Select(p => p.ToDto()).Where(p => p != null).Select(p => p!).ToList();
            return View("~/Views/Admin/Content/Pages.cshtml", dtos);
        }

        public IActionResult CreatePage()
            => View("~/Views/Admin/Content/PageForm.cshtml", new ContentPageDto());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePage(ContentPageDto model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Admin/Content/PageForm.cshtml", model);
            
            var page = new ContentPage
            {
                Slug = model.Slug,
                TitleAr = model.TitleAr,
                TitleEn = model.TitleEn,
                BodyAr = model.BodyAr,
                BodyEn = model.BodyEn,
                IsPublished = model.IsPublished,
                MetaTitle = model.MetaTitle,
                MetaDesc = model.MetaDesc,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.ContentPages.Add(page);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إنشاء الصفحة";
            return RedirectToAction(nameof(Pages));
        }

        public async Task<IActionResult> EditPage(int id)
        {
            var page = await _db.ContentPages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Id == id);
            if (page == null) return NotFound();
            return View("~/Views/Admin/Content/PageForm.cshtml", page.ToDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPage(int id, ContentPageDto model)
        {
            var page = await _db.ContentPages.FindAsync(id);
            if (page == null) return NotFound();
            page.Slug = string.IsNullOrEmpty(model.Slug) ? page.Slug : model.Slug;
            page.TitleAr = model.TitleAr;
            page.TitleEn = model.TitleEn;
            page.BodyAr = model.BodyAr;
            page.BodyEn = model.BodyEn;
            page.IsPublished = model.IsPublished;
            page.MetaTitle = model.MetaTitle;
            page.MetaDesc = model.MetaDesc;
            page.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تحديث الصفحة";
            return RedirectToAction(nameof(Pages));
        }
    }

    // ── ADMIN SETTINGS ────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminSettingsController : Controller
    {
        private readonly IAppDbContext _db;
        public AdminSettingsController(IAppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var contactInfo = await _db.ContactInfos.FirstOrDefaultAsync();
            ViewBag.ContactInfo = contactInfo.ToDto();
            
            var settings = await _db.SiteSettings.OrderBy(s => s.Key).ToListAsync();
            var dtos = settings.Select(s => s.ToDto()).Where(s => s != null).Select(s => s!).ToList();
            
            return View("~/Views/Admin/Settings/Index.cshtml", dtos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSettings(Dictionary<int, string> values)
        {
            foreach (var kv in values)
            {
                var s = await _db.SiteSettings.FindAsync(kv.Key);
                if (s != null) { s.Value = kv.Value; s.UpdatedAt = DateTime.UtcNow; }
            }
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم حفظ الإعدادات";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveContact(ContactInfoDto model)
        {
            var contact = await _db.ContactInfos.FirstOrDefaultAsync();
            if (contact == null)
            {
                var newContact = new ContactInfo
                {
                    Phone = model.Phone,
                    Phone2 = model.Phone2,
                    Email = model.Email,
                    AddressAr = model.AddressAr,
                    AddressEn = model.AddressEn,
                    City = model.City,
                    Facebook = model.Facebook,
                    Instagram = model.Instagram,
                    WhatsApp = model.WhatsApp,
                    TikTok = model.TikTok,
                    WorkingHoursAr = model.WorkingHoursAr,
                    WorkingHoursEn = model.WorkingHoursEn,
                    MapEmbedUrl = model.MapEmbedUrl,
                    ShowPhone = model.ShowPhone,
                    ShowPhone2 = model.ShowPhone2,
                    ShowEmail = model.ShowEmail,
                    ShowAddress = model.ShowAddress,
                    ShowMap = model.ShowMap,
                    ShowWorkingHours = model.ShowWorkingHours,
                    ShowFacebook = model.ShowFacebook,
                    ShowInstagram = model.ShowInstagram,
                    ShowWhatsApp = model.ShowWhatsApp,
                    ShowTikTok = model.ShowTikTok,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ContactInfos.Add(newContact);
            }
            else
            {
                contact.Phone = model.Phone;
                contact.Phone2 = model.Phone2;
                contact.Email = model.Email;
                contact.AddressAr = model.AddressAr;
                contact.AddressEn = model.AddressEn;
                contact.City = model.City;
                contact.Facebook = model.Facebook;
                contact.Instagram = model.Instagram;
                contact.WhatsApp = model.WhatsApp;
                contact.TikTok = model.TikTok;
                contact.WorkingHoursAr = model.WorkingHoursAr;
                contact.WorkingHoursEn = model.WorkingHoursEn;
                contact.MapEmbedUrl = model.MapEmbedUrl;
                contact.ShowPhone = model.ShowPhone;
                contact.ShowPhone2 = model.ShowPhone2;
                contact.ShowEmail = model.ShowEmail;
                contact.ShowAddress = model.ShowAddress;
                contact.ShowMap = model.ShowMap;
                contact.ShowWorkingHours = model.ShowWorkingHours;
                contact.ShowFacebook = model.ShowFacebook;
                contact.ShowInstagram = model.ShowInstagram;
                contact.ShowWhatsApp = model.ShowWhatsApp;
                contact.ShowTikTok = model.ShowTikTok;
                contact.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
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
