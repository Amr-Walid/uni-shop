using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _products;
        private readonly IContentService _content;
        private readonly IMessageService _messages;
        private readonly IMemoryCache _cache;

        // Static reference data changes only from the admin panel, so a short TTL
        // keeps the storefront fresh while eliminating most per-request DB traffic.
        private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

        public HomeController(
            IProductService products,
            IContentService content,
            IMessageService messages,
            IMemoryCache cache)
        {
            _products = products;
            _content = content;
            _messages = messages;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            // PERFORMANCE: cache the near-static reference data and run the remaining
            // independent product queries; this replaces 7 sequential DB round-trips
            // (measured ~10s p50 under load) with mostly cache hits.
            // NOTE: all services share one scoped DbContext, and EF Core forbids
            // concurrent operations on the same context — so DB reads stay sequential.
            // The performance win here comes from caching, which turns most of these
            // calls into in-memory hits that never touch the database.
            var settingsList = await _cache.GetOrCreateAsync("home:settings", e =>
            {
                e.AbsoluteExpirationRelativeToNow = CacheTtl;
                return _content.GetSettingsListAsync();
            }) ?? new List<SiteSettingDto>();

            var categoriesList = await _cache.GetOrCreateAsync("home:categories", e =>
            {
                e.AbsoluteExpirationRelativeToNow = CacheTtl;
                return _products.GetActiveCategoriesAsync();
            }) ?? new List<CategoryDto>();

            var contentBlocks = await _cache.GetOrCreateAsync("home:blocks", e =>
            {
                e.AbsoluteExpirationRelativeToNow = CacheTtl;
                return _content.GetBlocksAsync();
            }) ?? new Dictionary<string, string>();

            var contactInfo = await _cache.GetOrCreateAsync("home:contact", e =>
            {
                e.AbsoluteExpirationRelativeToNow = CacheTtl;
                return _content.GetContactInfoAsync();
            });

            var settingsMap = settingsList
                .Where(s => !string.IsNullOrEmpty(s.Key))
                .ToDictionary(s => s.Key, s => s.Value ?? "");

            // ── منتجات السلايدر والكتالوج من إعدادات الداشبورد (بنفس الترتيب) ──
            var heroIds = ParseIds(settingsMap.GetValueOrDefault("homepage.hero.products"));
            var featuredIds = ParseIds(settingsMap.GetValueOrDefault("homepage.featured.products"));

            var heroProducts = await _products.GetByIdsOrderedAsync(heroIds);
            var featured = await _products.GetByIdsOrderedAsync(featuredIds);
            var dbFeatured = await _products.GetFeaturedAsync(50);

            // دمج المنتجات المميزة المحددة يدوياً من لوحة التحكم مع أي منتجات تم تفعيل خيار "مميز" لها في صفحة تعديل المنتج
            foreach (var p in dbFeatured)
            {
                if (!featured.Any(f => f.Id == p.Id))
                {
                    featured.Add(p);
                }
            }

            // Fallback لو كل المنتجات المميزة المختارة غير نشطة
            if (heroProducts.Count == 0)
                heroProducts = featured.Take(5).ToList();

            var vm = new HomeViewModel
            {
                HeroProducts = heroProducts,
                FeaturedProducts = featured,
                Categories = categoriesList,
                ContentBlocks = contentBlocks,
                ContactInfo = contactInfo,
                Settings = settingsList,
                SettingsMap = settingsMap
            };
            return View(vm);
        }

        private static List<int> ParseIds(string? csv)
            => string.IsNullOrWhiteSpace(csv)
                ? new List<int>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Select(s => int.TryParse(s, out var n) ? n : 0)
                     .Where(n => n > 0)
                     .Distinct()
                     .ToList();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string phone, string message)
        {
            // Server-side validation: the entity caps Name/Email at 100 and Phone
            // at 20 chars — without these guards an oversized value would throw a
            // SqlException (500) instead of a friendly redirect.
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ContactError"] = "true";
                return Redirect("/#contact");
            }

            static string? Clamp(string? value, int max)
                => string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value[..max]);

            var dto = new ContactMessageDto
            {
                Name = Clamp(name.Trim(), 100),
                Email = Clamp(email?.Trim(), 100),
                Phone = Clamp(phone?.Trim(), 20),
                Message = Clamp(message.Trim(), 4000)
            };

            await _messages.SaveMessageAsync(dto);

            TempData["ContactSuccess"] = "true";
            return Redirect("/#contact");
        }

        public IActionResult Error() => View();
    }
}
