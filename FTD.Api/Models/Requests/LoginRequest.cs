using System.ComponentModel.DataAnnotations;

namespace FTD.Api.Models.Requests
{
    /// <summary>
    /// طلب تسجيل الدخول للحصول على رمز JWT.
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(200, ErrorMessage = "البريد الإلكتروني طويل جداً")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "كلمة المرور غير صالحة")]
        public string Password { get; set; } = "";
    }
}
