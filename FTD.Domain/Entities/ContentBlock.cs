using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── CONTENT BLOCK ─────────────────────────────────────────────────────────
    // Key examples: about.title, about.body, hero.slide1.title, mission.text
    public class ContentBlock
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Key { get; set; } = "";
        [MaxLength(300)] public string? TitleAr { get; set; }
        [MaxLength(300)] public string? TitleEn { get; set; }
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public string? ImagePath { get; set; }
        [MaxLength(50)] public string Type { get; set; } = "text";   // text | html | image
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
