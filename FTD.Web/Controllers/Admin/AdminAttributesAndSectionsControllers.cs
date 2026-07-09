using FTD.Web.Data;
using FTD.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FTD.Web.Controllers.Admin
{
    // ── ATTRIBUTES ────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminAttributesController : Controller
    {
        private readonly AppDbContext _db;
        public AdminAttributesController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _db.ProductAttributes
                .Include(a => a.Values)
                .Include(a => a.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync();
            ViewBag.CurrentCategoryId = categoryId;

            return View("~/Views/Admin/Attributes/Index.cshtml", await query.OrderBy(a => a.CategoryId).ThenBy(a => a.SortOrder).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            return View("~/Views/Admin/Attributes/Form.cshtml", new ProductAttribute());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductAttribute model)
        {
            // Remove validation errors for nav properties
            ModelState.Remove("Category");
            ModelState.Remove("Values");
            ModelState.Remove("ProductValues");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
                // Show validation errors
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["Error"] = "خطأ في البيانات: " + errors;
                return View("~/Views/Admin/Attributes/Form.cshtml", model);
            }
            _db.ProductAttributes.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة الـ Attribute";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var attr = await _db.ProductAttributes.FindAsync(id);
            if (attr == null) return NotFound();
            ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).ToListAsync();
            return View("~/Views/Admin/Attributes/Form.cshtml", attr);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductAttribute model)
        {
            var attr = await _db.ProductAttributes.FindAsync(id);
            if (attr == null) return NotFound();
            attr.NameAr = model.NameAr;
            attr.NameEn = model.NameEn;
            attr.CategoryId = model.CategoryId;
            attr.SortOrder = model.SortOrder;
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم تحديث الـ Attribute";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var attr = await _db.ProductAttributes.FindAsync(id);
            if (attr != null) { _db.ProductAttributes.Remove(attr); await _db.SaveChangesAsync(); }
            TempData["Success"] = "تم الحذف";
            return RedirectToAction(nameof(Index));
        }

        // ── Attribute Values ──────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddValue(int attributeId, string valueAr, string valueEn)
        {
            if (!string.IsNullOrWhiteSpace(valueAr))
            {
                _db.AttributeValues.Add(new AttributeValue
                {
                    AttributeId = attributeId,
                    ValueAr = valueAr.Trim(),
                    ValueEn = valueEn?.Trim() ?? valueAr.Trim()
                });
                await _db.SaveChangesAsync();
                TempData["Success"] = "تم إضافة القيمة";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteValue(int id)
        {
            var val = await _db.AttributeValues.FindAsync(id);
            if (val != null) { _db.AttributeValues.Remove(val); await _db.SaveChangesAsync(); }
            TempData["Success"] = "تم حذف القيمة";
            return RedirectToAction(nameof(Index));
        }
    }

    // ── PAGE SECTIONS (extends AdminContentController) ────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminPageSectionsController : Controller
    {
        private readonly AppDbContext _db;
        public AdminPageSectionsController(AppDbContext db) => _db = db;

        // GET /Admin/AdminPageSections/Manage/{pageId}
        public async Task<IActionResult> Manage(int id)
        {
            var page = await _db.ContentPages
                .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                .FirstOrDefaultAsync(p => p.Id == id);
            if (page == null) return NotFound();
            return View("~/Views/Admin/Content/PageSections.cshtml", page);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(int id, string type)
        {
            var pageId = id;
            var page = await _db.ContentPages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null) return NotFound();

            var maxOrder = page.Sections.Any() ? page.Sections.Max(s => s.SortOrder) : 0;

            // Default content per type
            var defaultJson = type switch
            {
                "FAQ" => "[]",
                "Cards" => "[]",
                "Hero" => "{}",
                _ => "{\"ar\":\"\",\"en\":\"\"}"
            };

            _db.PageSections.Add(new PageSection
            {
                PageId = pageId,
                Type = type,
                ContentJson = defaultJson,
                SortOrder = maxOrder + 1,
                IsVisible = true
            });
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم إضافة الـ Section";
            return RedirectToAction(nameof(Manage), new { id = pageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSection(int id, IFormCollection form)
        {
            var section = await _db.PageSections.Include(s => s.Page).FirstOrDefaultAsync(s => s.Id == id);
            if (section == null) return NotFound();

            section.ContentJson = section.Type switch
            {
                "RichText" => BuildRichTextJson(form),
                "FAQ" => BuildFaqJson(form, id),
                "Cards" => BuildCardsJson(form, id),
                "Hero" => BuildHeroJson(form),
                _ => section.ContentJson
            };

            await _db.SaveChangesAsync();
            TempData["Success"] = "تم حفظ الـ Section";
            return RedirectToAction(nameof(Manage), new { id = section.PageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _db.PageSections.FindAsync(id);
            if (section == null) return NotFound();
            var pageId = section.PageId;
            _db.PageSections.Remove(section);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم الحذف";
            return RedirectToAction(nameof(Manage), new { id = pageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSectionVisible(int id)
        {
            var section = await _db.PageSections.FindAsync(id);
            if (section == null) return NotFound();
            section.IsVisible = !section.IsVisible;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id = section.PageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSectionUp(int id)
        {
            var section = await _db.PageSections.FindAsync(id);
            if (section == null) return NotFound();
            var prev = await _db.PageSections
                .Where(s => s.PageId == section.PageId && s.SortOrder < section.SortOrder)
                .OrderByDescending(s => s.SortOrder).FirstOrDefaultAsync();
            if (prev != null) (section.SortOrder, prev.SortOrder) = (prev.SortOrder, section.SortOrder);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id = section.PageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSectionDown(int id)
        {
            var section = await _db.PageSections.FindAsync(id);
            if (section == null) return NotFound();
            var next = await _db.PageSections
                .Where(s => s.PageId == section.PageId && s.SortOrder > section.SortOrder)
                .OrderBy(s => s.SortOrder).FirstOrDefaultAsync();
            if (next != null) (section.SortOrder, next.SortOrder) = (next.SortOrder, section.SortOrder);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Manage), new { id = section.PageId });
        }

        // ── JSON Builders ──────────────────────────────────────────────────────
        private static string BuildRichTextJson(IFormCollection f)
            => JsonSerializer.Serialize(new { ar = f["ar"].ToString(), en = f["en"].ToString() });

        private static string BuildFaqJson(IFormCollection f, int sectionId)
        {
            var qArs = f[$"faq_q_ar_{sectionId}"].ToArray();
            var qEns = f[$"faq_q_en_{sectionId}"].ToArray();
            var aArs = f[$"faq_a_ar_{sectionId}"].ToArray();
            var aEns = f[$"faq_a_en_{sectionId}"].ToArray();

            var items = new List<object>();
            for (int i = 0; i < qArs.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(qArs[i])) continue;
                items.Add(new
                {
                    q_ar = qArs[i],
                    q_en = i < qEns.Length ? qEns[i] : "",
                    a_ar = i < aArs.Length ? aArs[i] : "",
                    a_en = i < aEns.Length ? aEns[i] : ""
                });
            }
            return JsonSerializer.Serialize(items);
        }

        private static string BuildCardsJson(IFormCollection f, int sectionId)
        {
            var icons = f[$"card_icon_@{sectionId}"].ToArray();
            var titleArs = f[$"card_title_ar_{sectionId}"].ToArray();
            var titleEns = f[$"card_title_en_{sectionId}"].ToArray();
            var bodyArs = f[$"card_body_ar_{sectionId}"].ToArray();
            var bodyEns = f[$"card_body_en_{sectionId}"].ToArray();

            var items = new List<object>();
            for (int i = 0; i < titleArs.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(titleArs[i])) continue;
                items.Add(new
                {
                    icon = i < icons.Length ? icons[i] : "",
                    title_ar = titleArs[i],
                    title_en = i < titleEns.Length ? titleEns[i] : "",
                    body_ar = i < bodyArs.Length ? bodyArs[i] : "",
                    body_en = i < bodyEns.Length ? bodyEns[i] : ""
                });
            }
            return JsonSerializer.Serialize(items);
        }

        private static string BuildHeroJson(IFormCollection f)
            => JsonSerializer.Serialize(new
            {
                title_ar = f["title_ar"].ToString(),
                title_en = f["title_en"].ToString(),
                desc_ar = f["desc_ar"].ToString(),
                desc_en = f["desc_en"].ToString(),
                btn_ar = f["btn_ar"].ToString(),
                btn_en = f["btn_en"].ToString(),
                btn_url = f["btn_url"].ToString()
            });
    }
}
