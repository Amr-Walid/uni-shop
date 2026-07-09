using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── ORDER STATUS ──────────────────────────────────────────────────────────
    public class OrderStatus
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string NameAr { get; set; } = "";
        [Required, MaxLength(100)] public string NameEn { get; set; } = "";
        [MaxLength(7)] public string ColorHex { get; set; } = "#6c757d";
        public string? Icon { get; set; }
        public int SortOrder { get; set; } = 0;

        public ICollection<SalesOrder> Orders { get; set; } = new List<SalesOrder>();
    }
}
