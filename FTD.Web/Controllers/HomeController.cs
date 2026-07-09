using FTD.Web.Data;
using FTD.Web.Models;
using FTD.Web.Services;
using FTD.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTD.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _products;
        private readonly ContentService _content;
        private readonly AppDbContext _db;
        private readonly EmailService _email;

        public HomeController(
            ProductService products,
            ContentService content,
            AppDbContext db,
            EmailService email)
        {
            _products = products;
            _content = content;
            _db = db;
            _email = email;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new HomeViewModel
            {
                FeaturedProducts = await _products.GetFeaturedAsync(6),
                Categories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.SortOrder).ToListAsync(),
                ContentBlocks = await _content.GetBlocksAsync(),
                ContactInfo = await _content.GetContactInfoAsync(),
                Settings = await _db.SiteSettings.ToListAsync()
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
