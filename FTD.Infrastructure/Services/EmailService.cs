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

        public async Task SendPasswordResetAsync(
            string toEmail, string userName, string resetToken, string language = "ar")
        {
            if (string.IsNullOrEmpty(_settings.SenderEmail) ||
                _settings.SenderEmail == "your-email@gmail.com")
            {
                // Never log the token itself — an application log is not a safe
                // place for a live credential.
                _logger.LogWarning(
                    "Email not configured — password reset for {Email} could not be delivered.", toEmail);
                return;
            }

            var isArabic = !string.Equals(language, "en", StringComparison.OrdinalIgnoreCase);

            try
            {
                var mail = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = isArabic
                        ? "إعادة تعيين كلمة المرور — Uni-Shop"
                        : "Password Reset — Uni-Shop",
                    IsBodyHtml = true,
                    Body = isArabic
                        ? BuildArabicResetBody(userName, resetToken)
                        : BuildEnglishResetBody(userName, resetToken)
                };

                mail.To.Add(toEmail);

                using var smtp = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.Password),
                    EnableSsl = true
                };

                await smtp.SendMailAsync(mail);
                _logger.LogInformation("Password reset email sent to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                // Swallowed deliberately: AuthService.ForgotPasswordAsync always
                // reports success to the caller so the endpoint cannot be used to
                // enumerate which addresses have accounts. Surfacing an error
                // here would leak exactly that.
                _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            }
        }

        private static string BuildArabicResetBody(string userName, string token) => $@"
<div style='font-family:Tahoma,sans-serif;direction:rtl;max-width:600px;margin:0 auto'>
  <div style='background:#1A6BFF;padding:20px 30px;border-radius:12px 12px 0 0'>
    <h2 style='color:white;margin:0;font-size:20px'>🔐 إعادة تعيين كلمة المرور</h2>
  </div>
  <div style='background:#f9f9f9;padding:24px 30px;border:1px solid #e5e5e5;border-top:none;border-radius:0 0 12px 12px'>
    <p style='font-size:15px;color:#333'>مرحباً {userName}،</p>
    <p style='font-size:14px;color:#555;line-height:1.8'>
      وصلنا طلب لإعادة تعيين كلمة مرور حسابك. استخدم الرمز التالي في التطبيق لإكمال العملية:
    </p>
    <div style='margin:20px 0;padding:16px;background:#fff;border:2px dashed #1A6BFF;border-radius:8px;text-align:center'>
      <code style='font-size:15px;font-weight:700;color:#1A6BFF;word-break:break-all;direction:ltr;display:inline-block'>{token}</code>
    </div>
    <div style='padding:14px;background:#fff3cd;border-radius:8px;font-size:13px;color:#856404'>
      ⚠️ إذا لم تطلب إعادة التعيين، تجاهل هذه الرسالة — لن يتغير أي شيء في حسابك.
    </div>
  </div>
</div>";

        private static string BuildEnglishResetBody(string userName, string token) => $@"
<div style='font-family:Arial,sans-serif;max-width:600px;margin:0 auto'>
  <div style='background:#1A6BFF;padding:20px 30px;border-radius:12px 12px 0 0'>
    <h2 style='color:white;margin:0;font-size:20px'>🔐 Password Reset</h2>
  </div>
  <div style='background:#f9f9f9;padding:24px 30px;border:1px solid #e5e5e5;border-top:none;border-radius:0 0 12px 12px'>
    <p style='font-size:15px;color:#333'>Hello {userName},</p>
    <p style='font-size:14px;color:#555;line-height:1.7'>
      We received a request to reset your account password. Use the code below in the app to continue:
    </p>
    <div style='margin:20px 0;padding:16px;background:#fff;border:2px dashed #1A6BFF;border-radius:8px;text-align:center'>
      <code style='font-size:15px;font-weight:700;color:#1A6BFF;word-break:break-all'>{token}</code>
    </div>
    <div style='padding:14px;background:#fff3cd;border-radius:8px;font-size:13px;color:#856404'>
      ⚠️ If you did not request a reset, ignore this email — nothing will change.
    </div>
  </div>
</div>";
    }
}
