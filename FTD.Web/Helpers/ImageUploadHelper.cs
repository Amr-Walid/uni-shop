using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FTD.Web.Helpers
{
    /// <summary>
    /// Single, shared image-upload pipeline for all admin controllers.
    /// Replaces three copy-pasted implementations (products, categories, brands)
    /// so validation rules (extension whitelist + size cap) can never drift apart.
    /// </summary>
    public static class ImageUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

        /// <summary>
        /// Validates and saves an uploaded image under wwwroot/images/{folder}.
        /// Returns the web-relative path ("/images/{folder}/{name}").
        /// Throws <see cref="InvalidOperationException"/> with a localized message
        /// on validation failure (callers surface it via TempData/ModelState).
        /// </summary>
        public static async Task<string> SaveAsync(IFormFile file, IWebHostEnvironment env, string folder)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("لم يتم اختيار ملف صالح.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                throw new InvalidOperationException("صيغة الصورة غير مدعومة. الصيغ المسموحة: JPG, PNG, GIF, WebP");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException("حجم الصورة يتجاوز الحد الأقصى (5 ميجابايت).");

            var dir = Path.Combine(env.WebRootPath, "images", folder);
            Directory.CreateDirectory(dir);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(dir, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{folder}/{fileName}";
        }
    }
}
