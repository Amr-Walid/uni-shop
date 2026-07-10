using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using FTD.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FTD.Infrastructure.Services
{
    public class EmailSettings
    {
        public string SmtpHost    { get; set; } = "smtp.gmail.com";
        public int    SmtpPort    { get; set; } = 587;
        public string SenderEmail { get; set; } = "";
        public string SenderName  { get; set; } = "Uni-Shop";
        public string Password    { get; set; } = "";
        public string NotifyEmail { get; set; } = "";
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailSettings settings, ILogger<EmailService> logger)
        {
            _settings = settings;
            _logger   = logger;
        }

        public async Task SendContactNotificationAsync(
            string name, string email, string phone, string message)
        {
            if (string.IsNullOrEmpty(_settings.SenderEmail) ||
                _settings.SenderEmail == "your-email@gmail.com")
            {
                _logger.LogWarning("Email not configured — skipping send.");
                return;
            }

            try
            {
                var mail = new MailMessage
                {
                    From       = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject    = $"رسالة جديدة من {name} — Uni-Shop",
                    IsBodyHtml = true,
                    Body       = $@"
<div style='font-family:Tahoma,sans-serif;direction:rtl;max-width:600px;margin:0 auto'>
  <div style='background:#1A6BFF;padding:20px 30px;border-radius:12px 12px 0 0'>
    <h2 style='color:white;margin:0;font-size:20px'>📩 رسالة جديدة من الموقع</h2>
  </div>
  <div style='background:#f9f9f9;padding:24px 30px;border:1px solid #e5e5e5;border-top:none;border-radius:0 0 12px 12px'>
    <table style='width:100%;border-collapse:collapse'>
      <tr><td style='padding:10px 0;border-bottom:1px solid #eee;color:#666;width:120px'>الاسم</td>
          <td style='padding:10px 0;border-bottom:1px solid #eee;font-weight:700'>{name}</td></tr>
      <tr><td style='padding:10px 0;border-bottom:1px solid #eee;color:#666'>البريد</td>
          <td style='padding:10px 0;border-bottom:1px solid #eee'><a href='mailto:{email}'>{email}</a></td></tr>
      <tr><td style='padding:10px 0;border-bottom:1px solid #eee;color:#666'>الهاتف</td>
          <td style='padding:10px 0;border-bottom:1px solid #eee'>{phone}</td></tr>
      <tr><td style='padding:10px 0;color:#666;vertical-align:top'>الرسالة</td>
          <td style='padding:10px 0'>{message}</td></tr>
    </table>
    <div style='margin-top:20px;padding:14px;background:#fff3cd;border-radius:8px;font-size:13px;color:#856404'>
      ⏰ تم الاستلام: {DateTime.Now:dd/MM/yyyy hh:mm tt}
    </div>
  </div>
</div>"
                };

                mail.To.Add(_settings.NotifyEmail);

                using var smtp = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password),
                    EnableSsl   = true
                };

                await smtp.SendMailAsync(mail);
                _logger.LogInformation("Contact email sent to {email}", _settings.NotifyEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send contact email");
            }
        }
    }
}
