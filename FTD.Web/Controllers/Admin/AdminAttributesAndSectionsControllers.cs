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
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        public AdminPageSectionsController(IContentService contentService,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        { _contentService = contentService; _env = env; }

        // GET /Admin/AdminPageSections/Manage/{pageId}
        // Legacy URL — now redirects to the unified visual builder (EditPage)
        public IActionResult Manage(int id)
            => Redirect($"/Admin/AdminContent/EditPage/{id}");

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
                "Video" => "{}",
                "Gallery" => "{}",
                "Testimonials" => "{}",
                _ => "{\"ar\":\"\",\"en\":\"\"}"
            };

            await _contentService.AddPageSectionAsync(pageId, type, defaultJson);
            TempData["Success"] = "تم إضافة الـ Section";
            return Redirect($"/Admin/AdminContent/EditPage/{pageId}#sections");
        }

        // ── AJAX: bulk drag-and-drop reorder ──────────────────────────────────
        [HttpPost]
        [Route("Admin/AdminPageSections/UpdateSortOrders")]
        [ValidateAntiForgeryToken] // token sent via RequestVerificationToken header from JS
        public async Task<IActionResult> UpdateSortOrders([FromBody] List<int> orderedSectionIds)
        {
            if (orderedSectionIds == null || orderedSectionIds.Count == 0)
                return BadRequest(new { success = false, message = "Empty list" });

            try
            {
                await _contentService.UpdatePageSectionsOrderAsync(orderedSectionIds);
                return Json(new { success = true });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── AJAX: image upload (Quill inline images + section images) ─────────
        // SECURITY: .svg deliberately excluded — SVG files can embed <script> and
        // on-load handlers, so an uploaded SVG served from our own origin becomes a
        // stored-XSS vector. Only true raster formats are accepted.
        private static readonly string[] AllowedImageExts = { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        // Magic-byte signatures used to verify the real file type instead of trusting
        // the (client-supplied, spoofable) extension alone.
        private static readonly Dictionary<string, byte[][]> ImageSignatures = new()
        {
            [".png"]  = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
            [".gif"]  = new[] { System.Text.Encoding.ASCII.GetBytes("GIF87a"), System.Text.Encoding.ASCII.GetBytes("GIF89a") },
            [".jpg"]  = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".webp"] = new[] { System.Text.Encoding.ASCII.GetBytes("RIFF") },
        };
        private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB

        [HttpPost]
        [Route("Admin/AdminPageSections/UploadImage")]
        [ValidateAntiForgeryToken] // token sent as form field or header from JS
        public async Task<IActionResult> UploadImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "لم يتم اختيار ملف" });

            if (file.Length > MaxImageBytes)
                return BadRequest(new { error = "حجم الصورة أكبر من 5 ميجا" });

            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExts.Contains(ext))
                return BadRequest(new { error = "صيغة غير مدعومة — المسموح: jpg, png, webp, gif" });

            // Verify the file's real content (magic bytes) matches the claimed extension,
            // so a script disguised with an image extension is rejected before it is stored.
            if (ImageSignatures.TryGetValue(ext, out var sigs))
            {
                var header = new byte[8];
                await using (var probe = file.OpenReadStream())
                {
                    var read = await probe.ReadAsync(header.AsMemory(0, header.Length));
                    if (read < 4 || !sigs.Any(sig => header.Take(sig.Length).SequenceEqual(sig)))
                        return BadRequest(new { error = "محتوى الملف لا يطابق صيغة الصورة المعلنة" });
                }
            }

            var dir = System.IO.Path.Combine(_env.WebRootPath, "images", "uploads");
            System.IO.Directory.CreateDirectory(dir);

            var filename = Guid.NewGuid().ToString("N") + ext;
            var fullPath = System.IO.Path.Combine(dir, filename);

            using (var stream = new System.IO.FileStream(fullPath, System.IO.FileMode.Create))
                await file.CopyToAsync(stream);

            return Json(new { url = $"/images/uploads/{filename}" });
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
                "Video" => BuildVideoJson(form),
                "Gallery" => BuildGalleryJson(form, id),
                "Testimonials" => BuildTestimonialsJson(form, id),
                _ => section.ContentJson
            };

            await _contentService.SavePageSectionContentAsync(id, contentJson ?? "");

            // AJAX save (from the visual builder) → JSON response, no reload
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            TempData["Success"] = "تم حفظ الـ Section";
            return Redirect($"/Admin/AdminContent/EditPage/{section.PageId}#sections");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();
            var pageId = section.PageId;

            await _contentService.DeletePageSectionAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            TempData["Success"] = "تم الحذف";
            return Redirect($"/Admin/AdminContent/EditPage/{pageId}#sections");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSectionVisible(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();

            await _contentService.TogglePageSectionVisibilityAsync(id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, visible = !section.IsVisible });

            return Redirect($"/Admin/AdminContent/EditPage/{section.PageId}#sections");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSectionUp(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();

            await _contentService.MovePageSectionUpAsync(id);
            return Redirect($"/Admin/AdminContent/EditPage/{section.PageId}#sections");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSectionDown(int id)
        {
            var section = await _contentService.GetPageSectionAsync(id);
            if (section == null) return NotFound();

            await _contentService.MovePageSectionDownAsync(id);
            return Redirect($"/Admin/AdminContent/EditPage/{section.PageId}#sections");
        }

        // ── Style helper — shared appearance options for every section type ──
        // bg: "light" | "dark" | "brand" | "none"
        // pad: "sm" | "md" | "lg"
        // align: "start" | "center" | "end"
        private static Dictionary<string, object> WithStyle(IFormCollection f, Dictionary<string, object> data)
        {
            data["style"] = new
            {
                bg = f["style_bg"].ToString(),
                pad = f["style_pad"].ToString(),
                align = f["style_align"].ToString()
            };
            return data;
        }

        // ── JSON Builders ──────────────────────────────────────────────────────
        private static string BuildRichTextJson(IFormCollection f)
            => JsonSerializer.Serialize(WithStyle(f, new Dictionary<string, object>
            {
                ["ar"] = f["ar"].ToString(),
                ["en"] = f["en"].ToString()
            }));

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
            return JsonSerializer.Serialize(WithStyle(f, new Dictionary<string, object> { ["items"] = items }));
        }

        private static string BuildCardsJson(IFormCollection f, int sectionId)
        {
            var icons = f[$"card_icon_{sectionId}"].ToArray();
            var images = f[$"card_image_{sectionId}"].ToArray();
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
                    card_image_path = i < images.Length ? images[i] : "",
                    title_ar = titleArs[i],
                    title_en = i < titleEns.Length ? titleEns[i] : "",
                    body_ar = i < bodyArs.Length ? bodyArs[i] : "",
                    body_en = i < bodyEns.Length ? bodyEns[i] : ""
                });
            }
            return JsonSerializer.Serialize(WithStyle(f, new Dictionary<string, object> { ["items"] = items }));
        }

        private static string BuildHeroJson(IFormCollection f)
            => JsonSerializer.Serialize(WithStyle(f, new Dictionary<string, object>
            {
                ["title_ar"] = f["title_ar"].ToString(),
                ["title_en"] = f["title_en"].ToString(),
                ["desc_ar"] = f["desc_ar"].ToString(),
                ["desc_en"] = f["desc_en"].ToString(),
                ["btn_ar"] = f["btn_ar"].ToString(),
                ["btn_en"] = f["btn_en"].ToString(),
                ["btn_url"] = f["btn_url"].ToString(),
                ["image_path"] = f["image_path"].ToString(),
                ["image_side"] = f["image_side"].ToString() // "start" | "end"
            }));

        private static string BuildVideoJson(IFormCollection f)
            => JsonSerializer.Serialize(WithStyle(f, new Dictionary<string, object>
            {
                ["title_ar"] = f["title_ar"].ToString(),
                ["title_en"] = f["title_en"].ToString(),
                ["desc_ar"] = f["desc_ar"].ToString(),
                ["desc_en"] = f["desc_en"].ToString(),
                ["video_url"] = f["video_url"].ToString() // YouTube / Vimeo / direct mp4
            }));

        private static string BuildGalleryJson(IFormCollection f, int sectionId)
        {
            var urls = f[$"gal_url_{sectionId}"].ToArray();
            var captionsAr = f[$"gal_cap_ar_{sectionId}"].ToArray();
            var captionsEn = f[$"gal_cap_en_{sectionId}"].ToArray();

            var items = new List<object>();
            for (int i = 0; i < urls.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(urls[i])) continue;
                items.Add(new
                {
                    url = urls[i],
                    cap_ar = i < captionsAr.Length ? captionsAr[i] : "",
                    cap_en = i < captionsEn.Length ? captionsEn[i] : ""
                });
            }
            return JsonSerializer.Serialize(WithStyle(f, new Dictionary<string, object>
            {
                ["title_ar"] = f["title_ar"].ToString(),
                ["title_en"] = f["title_en"].ToString(),
                ["items"] = items
            }));
        }

        private static string BuildTestimonialsJson(IFormCollection f, int sectionId)
        {
            var names = f[$"tst_name_{sectionId}"].ToArray();
            var roles = f[$"tst_role_{sectionId}"].ToArray();
            var avatars = f[$"tst_avatar_{sectionId}"].ToArray();
            var ratings = f[$"tst_rating_{sectionId}"].ToArray();
            var textArs = f[$"tst_text_ar_{sectionId}"].ToArray();
            var textEns = f[$"tst_text_en_{sectionId}"].ToArray();

            var items = new List<object>();
            for (int i = 0; i < names.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(names[i]) &&
                    (i >= textArs.Length || string.IsNullOrWhiteSpace(textArs[i]))) continue;
                items.Add(new
                {
                    name = names[i],
                    role = i < roles.Length ? roles[i] : "",
                    avatar = i < avatars.Length ? avatars[i] : "",
                    rating = i < ratings.Length && int.TryParse(ratings[i], out var r) ? Math.Clamp(r, 1, 5) : 5,
                    text_ar = i < textArs.Length ? textArs[i] : "",
                    text_en = i < textEns.Length ? textEns[i] : ""
                });
            }
            return JsonSerializer.Serialize(WithStyle(f, new Dictionary<string, object>
            {
                ["title_ar"] = f["title_ar"].ToString(),
                ["title_en"] = f["title_en"].ToString(),
                ["items"] = items
            }));
        }
    }
}
