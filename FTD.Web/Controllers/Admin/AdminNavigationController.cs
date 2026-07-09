using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminNavigationController : Controller
    {
        private readonly IAppDbContext _db;
        public AdminNavigationController(IAppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var pages = await _db.ContentPages
                .Where(p => p.IsPublished)
                .OrderBy(p => p.TitleAr)
                .ToListAsync();
            ViewBag.Pages = pages.Select(p => p.ToDto()).Where(p => p != null).Select(p => p!).ToList();

            var items = await _db.NavigationItems
                .Where(n => n.ParentId == null)
                .OrderBy(n => n.SortOrder)
                .ToListAsync();
            var dtos = items.Select(n => n.ToDto()).Where(n => n != null).Select(n => n!).ToList();

            return View("~/Views/Admin/Navigation/Index.cshtml", dtos);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NavigationItemDto model)
        {
            if (string.IsNullOrEmpty(model.LabelAr) || string.IsNullOrEmpty(model.LabelEn))
            {
                TempData["Error"] = "الاسم مطلوب";
                return RedirectToAction(nameof(Index));
            }
            if (string.IsNullOrEmpty(model.Location)) model.Location = "Both";
            
            var item = new NavigationItem
            {
                LabelAr = model.LabelAr,
                LabelEn = model.LabelEn,
                LinkType = model.LinkType,
                StaticRoute = model.StaticRoute,
                PageSlug = model.PageSlug,
                ExternalUrl = model.ExternalUrl,
                OpenInNewTab = model.OpenInNewTab,
                Location = model.Location,
                ParentId = model.ParentId,
                SortOrder = model.SortOrder,
                IsActive = true
            };

            _db.NavigationItems.Add(item);
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
