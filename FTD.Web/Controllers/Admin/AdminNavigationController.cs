using FTD.Web.Data;
using FTD.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminNavigationController : Controller
    {
        private readonly AppDbContext _db;
        public AdminNavigationController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            ViewBag.Pages = await _db.ContentPages
                .Where(p => p.IsPublished)
                .OrderBy(p => p.TitleAr)
                .ToListAsync();

            var items = await _db.NavigationItems
                .Where(n => n.ParentId == null)
                .OrderBy(n => n.SortOrder)
                .ToListAsync();

            return View("~/Views/Admin/Navigation/Index.cshtml", items);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NavigationItem model)
        {
            if (string.IsNullOrEmpty(model.LabelAr) || string.IsNullOrEmpty(model.LabelEn))
            {
                TempData["Error"] = "الاسم مطلوب";
                return RedirectToAction(nameof(Index));
            }
            if (string.IsNullOrEmpty(model.Location)) model.Location = "Both";
            model.IsActive = true;
            _db.NavigationItems.Add(model);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم الإضافة";
            return RedirectToAction(nameof(Index));
        }

        // Change location (Navbar / Footer / Both / Hidden) via dropdown
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetLocation(int id, string location)
        {
            var item = await _db.NavigationItems.FindAsync(id);
            if (item != null)
            {
                item.Location = location;
                item.IsActive = location != "Hidden";
                await _db.SaveChangesAsync();
            }
            TempData["Success"] = "تم تحديث مكان العرض";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.NavigationItems.FindAsync(id);
            if (item != null) { _db.NavigationItems.Remove(item); await _db.SaveChangesAsync(); }
            TempData["Success"] = "تم الحذف";
            return RedirectToAction(nameof(Index));
        }
    }
}
