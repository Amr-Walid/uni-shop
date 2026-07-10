using System.Threading.Tasks;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Web.ViewComponents
{
    public class AdminMessageCountViewComponent : ViewComponent
    {
        private readonly IMessageService _messageService;

        public AdminMessageCountViewComponent(IMessageService messageService)
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
