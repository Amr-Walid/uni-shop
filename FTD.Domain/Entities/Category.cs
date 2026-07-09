using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── CATEGORY ──────────────────────────────────────────────────────────────────
    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(150)] public string NameAr { get; set; } = "";
        [Required, MaxLength(150)] public string NameEn { get; set; } = "";
        [Required, MaxLength(100)] public string Slug { get; set; } = "";
        public string? ImagePath { get; set; }
        public string? Emoji { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
    }
}
