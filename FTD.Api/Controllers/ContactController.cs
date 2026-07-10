using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IMessageService _messageService;
        public ContactController(IMessageService messageService) => _messageService = messageService;

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Name) || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Message))
                return BadRequest("جميع الحقول الإلزامية مطلوبة (الاسم، البريد الإلكتروني، الرسالة)");

            await _messageService.SaveMessageAsync(dto);
            return Ok(new { success = true, message = "تم إرسال الرسالة بنجاح" });
        }
    }
}
