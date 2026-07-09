using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── CONTENT PAGE ─────────────────────────────────────────────────────────
    // Free pages created from Admin (privacy, shipping, terms …)
    public class ContentPage
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string Slug { get; set; } = "";
        [Required, MaxLength(200)] public string TitleAr { get; set; } = "";
        [Required, MaxLength(200)] public string TitleEn { get; set; } = "";
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public bool IsPublished { get; set; } = true;

        // SEO
        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Sections (JSON stored per page)
        public ICollection<PageSection> Sections { get; set; } = new List<PageSection>();
    }
}
