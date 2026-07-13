using FTD.Application.Interfaces;
using FTD.Application.Services;
using FTD.Application.DTOs;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _products;
        private readonly IContentService _content;
        private readonly IMessageService _messages;

        public HomeController(
            IProductService products,
            IContentService content,
            IMessageService messages)
        {
            _products = products;
            _content = content;
            _messages = messages;
        }

        public async Task<IActionResult> Index()
        {
            var categoriesList = await _products.GetActiveCategoriesAsync();
            var settingsList = await _content.GetSettingsListAsync();
            var settingsMap = settingsList
                .Where(s => !string.IsNullOrEmpty(s.Key))
                .ToDictionary(s => s.Key, s => s.Value ?? "");

            // ── منتجات السلايدر والكتالوج من إعدادات الداشبورد (بنفس الترتيب) ──
            var heroIds = ParseIds(settingsMap.GetValueOrDefault("homepage.hero.products"));
            var featuredIds = ParseIds(settingsMap.GetValueOrDefault("homepage.featured.products"));

            var heroProducts = await _products.GetByIdsOrderedAsync(heroIds);
            var featured = await _products.GetByIdsOrderedAsync(featuredIds);

            // دمج المنتجات المميزة المحددة يدوياً من لوحة التحكم مع أي منتجات تم تفعيل خيار "مميز" لها في صفحة تعديل المنتج
            var dbFeatured = await _products.GetFeaturedAsync(50);
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
                ContentBlocks = await _content.GetBlocksAsync(),
                ContactInfo = await _content.GetContactInfoAsync(),
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
            var dto = new ContactMessageDto
            {
                Name = name,
                Email = email,
                Phone = phone,
                Message = message
            };

            await _messages.SaveMessageAsync(dto);

            TempData["ContactSuccess"] = "true";
            return Redirect("/#contact");
        }

        public IActionResult Error() => View();
    }
}
