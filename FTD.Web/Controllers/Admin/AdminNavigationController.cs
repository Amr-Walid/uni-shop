using FTD.Application.DTOs;
using FTD.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminNavigationController : Controller
    {
        private readonly ContentService _contentService;

        public AdminNavigationController(ContentService contentService)
        {
            _contentService = contentService;
        }

        public async Task<IActionResult> Index()
        {
            var pages = await _contentService.GetAllPagesAsync();
            ViewBag.Pages = pages.Where(p => p.IsPublished).OrderBy(p => p.TitleAr).ToList();

            var dtos = await _contentService.GetAllNavigationItemsForAdminAsync();
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

            await _contentService.CreateNavigationItemAsync(model);
            TempData["Success"] = "تم الإضافة";
            return RedirectToAction(nameof(Index));
        }

        // Change location (Navbar / Footer / Both / Hidden) via dropdown
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetLocation(int id, string location)
        {
            await _contentService.SetNavigationItemLocationAsync(id, location);
            TempData["Success"] = "تم تحديث مكان العرض";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _contentService.DeleteNavigationItemAsync(id);
            TempData["Success"] = "تم الحذف";
            return RedirectToAction(nameof(Index));
        }
    }
}
