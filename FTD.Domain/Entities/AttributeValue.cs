using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── ATTRIBUTE VALUE ───────────────────────────────────────────────────────
    public class AttributeValue
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }

        [Required, MaxLength(200)] public string ValueAr { get; set; } = "";
        [Required, MaxLength(200)] public string ValueEn { get; set; } = "";

        public ProductAttribute Attribute { get; set; } = null!;
        public ICollection<ProductAttributeValue> ProductValues { get; set; } = new List<ProductAttributeValue>();
    }
}
