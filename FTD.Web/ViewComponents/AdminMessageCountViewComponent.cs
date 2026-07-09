using System.Threading.Tasks;
using FTD.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Web.ViewComponents
{
    public class AdminMessageCountViewComponent : ViewComponent
    {
        private readonly MessageService _messageService;

        public AdminMessageCountViewComponent(MessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var unreadCount = await _messageService.GetUnreadCountAsync();
            return View(unreadCount);
        }
    }
}
