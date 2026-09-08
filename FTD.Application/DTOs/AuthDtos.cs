using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Application.DTOs
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  AUTHENTICATION CONTRACTS
    //  Validation attributes mirror the AppUser / SalesOrder column limits so
    //  oversized input is rejected as a clean 400 by [ApiController] automatic
    //  model validation, instead of surfacing as a SqlException at save time.
    // ═══════════════════════════════════════════════════════════════════════════

    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(200, ErrorMessage = "البريد الإلكتروني طويل جداً")]
        public string Email { get; set; } = "";

        // MinimumLength mirrors the Identity password policy configured in
        // Program.cs (RequiredLength = 8) so the client gets a field-level error
        // rather than a generic Identity failure list.
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(150, ErrorMessage = "الاسم طويل جداً")]
        public string FullName { get; set; } = "";

        [StringLength(20, ErrorMessage = "رقم الهاتف طويل جداً")]
        public string? Phone { get; set; }

        [StringLength(5)]
        public string? PreferredLanguage { get; set; }

        [StringLength(200, ErrorMessage = "بيانات الجهاز طويلة جداً")]
        public string? DeviceInfo { get; set; }
    }

    public class LoginRequestDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(200, ErrorMessage = "البريد الإلكتروني طويل جداً")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "كلمة المرور غير صالحة")]
        public string Password { get; set; } = "";

        [StringLength(200, ErrorMessage = "بيانات الجهاز طويلة جداً")]
        public string? DeviceInfo { get; set; }
    }

    public class RefreshRequestDto
    {
        [Required(ErrorMessage = "رمز التجديد مطلوب")]
        [StringLength(500)]
        public string RefreshToken { get; set; } = "";

        [StringLength(200)]
        public string? DeviceInfo { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
        [StringLength(200)]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
        public string NewPassword { get; set; } = "";
    }

    public class ForgotPasswordRequestDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(200)]
        public string Email { get; set; } = "";
    }

    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(200)]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "رمز الاستعادة مطلوب")]
        public string Token { get; set; } = "";

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب أن تكون 8 أحرف على الأقل")]
        public string NewPassword { get; set; } = "";
    }

    /// <summary>Successful sign-in / refresh payload.</summary>
    public class AuthResultDto
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";

        /// <summary>Access-token lifetime in SECONDS (mobile clients schedule refresh from this).</summary>
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";

        public UserProfileDto User { get; set; } = new();
    }

    public class UserProfileDto
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? DefaultAddress { get; set; }
        public string? City { get; set; }
        public string? Governorate { get; set; }
        public string PreferredLanguage { get; set; } = "ar";
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();

        /// <summary>Lets the app show a badge/entry point for the admin area.</summary>
        public bool IsAdmin { get; set; }
    }

    public class UpdateProfileRequestDto
    {
        [StringLength(150, ErrorMessage = "الاسم طويل جداً")]
        public string? FullName { get; set; }

        [StringLength(20, ErrorMessage = "رقم الهاتف طويل جداً")]
        public string? Phone { get; set; }

        [StringLength(300, ErrorMessage = "العنوان طويل جداً")]
        public string? DefaultAddress { get; set; }

        [StringLength(100, ErrorMessage = "اسم المدينة طويل جداً")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "اسم المحافظة طويل جداً")]
        public string? Governorate { get; set; }

        [StringLength(5)]
        public string? PreferredLanguage { get; set; }
    }

    /// <summary>
    /// Outcome of an auth operation. Deliberately NOT an exception-based flow:
    /// failed logins are an expected, high-volume path and the API must map each
    /// distinct cause to a specific stable errorCode without paying the cost of
    /// throwing.
    /// </summary>
    public class AuthOperationResult
    {
        public bool Succeeded { get; set; }
        public AuthResultDto? Data { get; set; }

        /// <summary>Stable machine-readable code — see ApiErrorCodes.</summary>
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public static AuthOperationResult Success(AuthResultDto data)
            => new() { Succeeded = true, Data = data };

        public static AuthOperationResult Fail(string code, string message)
            => new() { Succeeded = false, ErrorCode = code, ErrorMessage = message };
    }
}
