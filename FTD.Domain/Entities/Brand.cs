using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── BRAND ─────────────────────────────────────────────────────────────────
    public class Brand
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string NameAr { get; set; } = "";
        [Required, MaxLength(100)] public string NameEn { get; set; } = "";
        [Required, MaxLength(100)] public string Slug { get; set; } = "";
        public string? LogoPath { get; set; }  // colored logo
        public string? BannerPath { get; set; }  // hero banner image
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Nav
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
