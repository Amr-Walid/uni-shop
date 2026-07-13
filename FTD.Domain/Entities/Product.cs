using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FTD.Domain.Entities
{
    // ── PRODUCT ───────────────────────────────────────────────────────────────
    public class Product
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }   // FK → Brand

        [Required, MaxLength(200)] public string NameAr { get; set; } = "";
        [Required, MaxLength(200)] public string NameEn { get; set; } = "";
        [Required, MaxLength(100)] public string Slug { get; set; } = "";

        public string? ShortDescAr { get; set; }
        public string? ShortDescEn { get; set; }
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }

        [Column(TypeName = "decimal(18,2)")] public decimal Price { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal? OldPrice { get; set; }

        public string? Badge { get; set; }   // NEW, HOT, RUGGED, 4K …
        public string? ImagePath { get; set; }
        public string? BrandName { get; set; }   // display / legacy fallback
        public string? Emoji { get; set; }   // fallback when no image

        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; } = false;
        public int SortOrder { get; set; } = 0;
        public int Stock { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }
        public string? FeaturesJson { get; set; }

        // Nav
        public Category Category { get; set; } = null!;
        public Brand? Brand { get; set; }
        public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<SalesOrderDetail> OrderDetails { get; set; } = new List<SalesOrderDetail>();
    }
}
