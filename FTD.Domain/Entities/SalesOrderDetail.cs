using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FTD.Domain.Entities
{
    // ── SALES ORDER DETAIL ────────────────────────────────────────────────────
    public class SalesOrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        [Required, MaxLength(200)] public string ProductName { get; set; } = "";  // snapshot
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }

        public SalesOrder Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
