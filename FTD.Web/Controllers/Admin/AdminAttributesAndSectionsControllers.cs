using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace FTD.Web.Controllers.Admin
{
    // ── ATTRIBUTES ────────────────────────────────────────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminAttributesController : Controller
    {
        private readonly IProductService _productService;
        public AdminAttributesController(IProductService productService) => _productService = productService;

        public async Task<IActionResult> Index(int? categoryId)
        {
            var categories = await _productService.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.CurrentCategoryId = categoryId;

            var dtos = await _productService.GetAttributesWithDetailsAsync(categoryId);
            return View("~/Views/Admin/Attributes/Index.cshtml", dtos);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _productService.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            return View("~/Views/Admin/Attributes/Form.cshtml", new ProductAttributeDto());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductAttributeDto model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _productService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                TempData["Error"] = "خطأ في البيانات: " + errors;
                return View("~/Views/Admin/Attributes/Form.cshtml", model);
            }

            await _productService.CreateAttributeAsync(model);
            TempData["Success"] = "تم إضافة الـ Attribute";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var attrs = await _productService.GetAttributesWithDetailsAsync(null);
            var attr = attrs.FirstOrDefault(a => a.Id == id);
            if (attr == null) return NotFound();

            var categories = await _productService.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            return View("~/Views/Admin/Attributes/Form.cshtml", attr);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductAttributeDto model)
        {
            await _productService.UpdateAttributeAsync(id, model);
            TempData["Success"] = "تم تحديث الـ Attribute";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAttributeAsync(id);
            TempData["Success"] = "تم الحذف";
            return RedirectToAction(nameof(Index));
        }

        // ── Attribute Values ──────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddValue(int attributeId, string valueAr, string valueEn)
        {
            if (!string.IsNullOrWhiteSpace(valueAr))
            {
                await _productService.AddAttributeValueAsync(attributeId, valueAr, valueEn);
                TempData["Success"] = "تم إضافة القيمة";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteValue(int id)
        {
            await _productService.DeleteAttributeValueAsync(id);
            TempData["Success"] = "تم حذف القيمة";
            return RedirectToAction(nameof(Index));
        }
    }

    // ── PAGE SECTIONS (extends AdminContentController) ────────────────────────
    [Authorize(Roles = "Admin")]
    public class AdminPageSectionsController : Controller
    {
        private readonly IContentService _contentService;
        public AdminPageSectionsController(IContentService contentService) => _contentService = contentService;

        // GET /Admin/AdminPageSections/Manage/{pageId}
        public async Task<IActionResult> Manage(int id)
        {
            var page = await _contentService.GetPageWithSectionsAsync(id);
            if (page == null) return NotFound();
            return View("~/Views/Admin/Content/PageSections.cshtml", page);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSection(int id, string type)
        {
            var pageId = id;
            // Default content per type
            var defaultJson = type switch
            {
                "FAQ" => "[]",
                "Cards" => "[]",
                "Hero" => "{}",
                _ => "{\"ar\":\"\",\"en\":\"\"}"
            };

            await _contentService.AddPageSectionAsync(pageId, type, defaultJson);
            TempData["Success"] = "تم إضافة الـ Section";
            return RedirectToAction(nameof(Manage), new { id = pageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSection(int id, IFormCollection form)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();

            var contentJson = section.Type switch
            {
                "RichText" => BuildRichTextJson(form),
                "FAQ" => BuildFaqJson(form, id),
                "Cards" => BuildCardsJson(form, id),
                "Hero" => BuildHeroJson(form),
                _ => section.ContentJson
            };

            await _contentService.SavePageSectionContentAsync(id, contentJson ?? "");
            TempData["Success"] = "تم حفظ الـ Section";
            return RedirectToAction(nameof(Manage), new { id = section.PageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();
            var pageId = section.PageId;

            await _contentService.DeletePageSectionAsync(id);
            TempData["Success"] = "تم الحذف";
            return RedirectToAction(nameof(Manage), new { id = pageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSectionVisible(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();

            await _contentService.TogglePageSectionVisibilityAsync(id);
            return RedirectToAction(nameof(Manage), new { id = section.PageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSectionUp(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();

            await _contentService.MovePageSectionUpAsync(id);
            return RedirectToAction(nameof(Manage), new { id = section.PageId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSectionDown(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();

            await _contentService.MovePageSectionDownAsync(id);
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
