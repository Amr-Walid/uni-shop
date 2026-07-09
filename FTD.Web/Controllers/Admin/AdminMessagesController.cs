using FTD.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminMessagesController : Controller
    {
        private readonly AppDbContext _db;
        public AdminMessagesController(AppDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var messages = await _db.ContactMessages
                .OrderByDescending(m => m.IsRead)
                .ThenByDescending(m => m.CreatedAt)
                .ToListAsync();
            return View("~/Views/Admin/Messages/Index.cshtml", messages);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg != null) { msg.IsRead = true; await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var msg = await _db.ContactMessages.FindAsync(id);
            if (msg != null) { _db.ContactMessages.Remove(msg); await _db.SaveChangesAsync(); }
            TempData["Success"] = "تم حذف الرسالة";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRead()
        {
            var read = _db.ContactMessages.Where(m => m.IsRead);
            _db.ContactMessages.RemoveRange(read);
            await _db.SaveChangesAsync();
            TempData["Success"] = "تم حذف الرسائل المقروءة";
            return RedirectToAction(nameof(Index));
        }
    }
}
