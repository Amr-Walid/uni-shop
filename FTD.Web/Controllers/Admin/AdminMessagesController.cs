using FTD.Application.Services;
using FTD.Application.Interfaces;
using FTD.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FTD.Web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminMessagesController : Controller
    {
        private readonly IMessageService _messages;
        public AdminMessagesController(IMessageService messages) => _messages = messages;

        public async Task<IActionResult> Index()
        {
            var messages = await _messages.GetAllMessagesAsync();
            return View("~/Views/Admin/Messages/Index.cshtml", messages);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _messages.MarkReadAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _messages.DeleteAsync(id);
            TempData["Success"] = "تم حذف الرسالة";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRead()
        {
            await _messages.DeleteReadAsync();
            TempData["Success"] = "تم حذف الرسائل المقروءة";
            return RedirectToAction(nameof(Index));
        }
    }
}
