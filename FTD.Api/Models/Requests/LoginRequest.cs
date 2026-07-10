namespace FTD.Api.Models.Requests
{
    /// <summary>
    /// طلب تسجيل الدخول للحصول على رمز JWT.
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
