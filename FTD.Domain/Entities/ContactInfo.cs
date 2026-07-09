using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── CONTACT INFO ──────────────────────────────────────────────────────────
    public class ContactInfo
    {
        public int Id { get; set; } = 1;

        // Values
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

        // Visibility toggles — false = hidden from public site
        public bool ShowPhone { get; set; } = true;
        public bool ShowPhone2 { get; set; } = false;
        public bool ShowEmail { get; set; } = true;
        public bool ShowAddress { get; set; } = true;
        public bool ShowMap { get; set; } = true;
        public bool ShowWorkingHours { get; set; } = true;
        public bool ShowFacebook { get; set; } = true;
        public bool ShowInstagram { get; set; } = true;
        public bool ShowWhatsApp { get; set; } = true;
        public bool ShowTikTok { get; set; } = false;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
