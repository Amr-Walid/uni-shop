using System;

namespace FTD.Domain.Entities
{
    // ── PRODUCT ATTRIBUTE VALUE (Junction) ────────────────────────────────────
    public class ProductAttributeValue
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }

        public Product Product { get; set; } = null!;
        public ProductAttribute Attribute { get; set; } = null!;
        public AttributeValue AttributeValue { get; set; } = null!;
    }
}
