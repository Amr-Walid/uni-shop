using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── ATTRIBUTE ─────────────────────────────────────────────────────────────
    public class ProductAttribute
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }

        [Required, MaxLength(100)] public string NameAr { get; set; } = "";
        [Required, MaxLength(100)] public string NameEn { get; set; } = "";
        public int SortOrder { get; set; } = 0;

        public Category Category { get; set; } = null!;
        public ICollection<AttributeValue> Values { get; set; } = new List<AttributeValue>();
        public ICollection<ProductAttributeValue> ProductValues { get; set; } = new List<ProductAttributeValue>();
    }
}
