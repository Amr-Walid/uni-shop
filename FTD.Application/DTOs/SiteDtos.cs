using System.ComponentModel.DataAnnotations;

namespace FTD.Application.DTOs
{
    public class ContactInfoDto
    {
        public int Id { get; set; }
        public string? Phone { get; set; }
        public string? Phone2 { get; set; }
        public string? Email { get; set; }
        public string? AddressAr { get; set; }
        public string? AddressEn { get; set; }
        public string? City { get; set; }
        public string? MapEmbedUrl { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? WhatsApp { get; set; }
        public string? TikTok { get; set; }
        public string? WorkingHoursAr { get; set; }
        public string? WorkingHoursEn { get; set; }
        public bool ShowPhone { get; set; }
        public bool ShowPhone2 { get; set; }
        public bool ShowEmail { get; set; }
        public bool ShowAddress { get; set; }
        public bool ShowMap { get; set; }
        public bool ShowWorkingHours { get; set; }
        public bool ShowFacebook { get; set; }
        public bool ShowInstagram { get; set; }
        public bool ShowWhatsApp { get; set; }
        public bool ShowTikTok { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SiteSettingDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string? Value { get; set; }
        public string? Description { get; set; }
        public string Type { get; set; } = "";
        public DateTime UpdatedAt { get; set; }
    }

    public class ContactMessageDto
    {
        public int Id { get; set; }

        // Length caps mirror the ContactMessage entity columns so oversized input
        // is rejected by validation (API [ApiController] auto-400) instead of
        // throwing a DB-level exception. The Web controller also clamps manually.
        [Required(ErrorMessage = "الاسم مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم طويل جداً")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
        [StringLength(100, ErrorMessage = "البريد الإلكتروني طويل جداً")]
        public string? Email { get; set; }

        [StringLength(20, ErrorMessage = "رقم الهاتف طويل جداً")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "الرسالة مطلوبة")]
        [StringLength(4000, ErrorMessage = "الرسالة طويلة جداً")]
        public string? Message { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
