using FTD.Application.Interfaces;
using FTD.Application.Services;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _products;
        private readonly ContentService _content;
        private readonly IAppDbContext _db;
        private readonly IEmailService _email;

        public HomeController(
            ProductService products,
            ContentService content,
            IAppDbContext db,
            IEmailService email)
        {
            _products = products;
            _content = content;
            _db = db;
            _email = email;
        }

        public async Task<IActionResult> Index()
        {
            var featured = await _products.GetFeaturedAsync(6);
            var categoriesList = await _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            var settingsList = await _db.SiteSettings.ToListAsync();

            var vm = new HomeViewModel
            {
                FeaturedProducts = featured,
                Categories = categoriesList.Select(c => c.ToDto()).Where(c => c != null).Select(c => c!).ToList(),
                ContentBlocks = await _content.GetBlocksAsync(),
                ContactInfo = await _content.GetContactInfoAsync(),
                Settings = settingsList.Select(s => s.ToDto()).Where(s => s != null).Select(s => s!).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(string name, string email, string phone, string message)
        {
            // 1. Save to DB
            _db.ContactMessages.Add(new ContactMessage
            {
                Name = name?.Trim(),
                Email = email?.Trim(),
                Phone = phone?.Trim(),
                Message = message?.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            // 2. Send email notification (async, won't block if it fails)
            _ = Task.Run(async () =>
            {
                await _email.SendContactNotificationAsync(
                    name ?? "", email ?? "", phone ?? "", message ?? "");
            });

            TempData["ContactSuccess"] = "true";
            return Redirect("/#contact");
        }

        public IActionResult Error() => View();
    }
}
