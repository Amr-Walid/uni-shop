using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── SITE SETTINGS ─────────────────────────────────────────────────────────
    public class SiteSetting
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Key { get; set; } = "";
        public string? Value { get; set; }
        [MaxLength(200)] public string? Description { get; set; }
        [MaxLength(50)] public string Type { get; set; } = "text";  // text|bool|image|color
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
