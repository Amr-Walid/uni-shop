using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── PRODUCT IMAGES ────────────────────────────────────────────────────────
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required] public string ImagePath { get; set; } = "";
        public bool IsMain { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        public Product Product { get; set; } = null!;
    }
}
