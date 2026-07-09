using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FTD.Domain.Entities
{
    // ── SALES ORDER ───────────────────────────────────────────────────────────
    public class SalesOrder
    {
        public int Id { get; set; }
        [Required, MaxLength(30)] public string OrderNumber { get; set; } = "";
        public int StatusId { get; set; }

        // Customer Info
        [Required, MaxLength(150)] public string CustomerName { get; set; } = "";
        [Required, MaxLength(20)] public string CustomerPhone { get; set; } = "";
        [MaxLength(200)] public string? CustomerEmail { get; set; }
        [MaxLength(300)] public string? Address { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        [MaxLength(100)] public string? Governorate { get; set; }

        // Financials
        [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }
        [Column(TypeName = "decimal(18,2)")] public decimal ShippingFee { get; set; } = 0;
        [Column(TypeName = "decimal(18,2)")] public decimal TotalAmount { get; set; }

        public string? Notes { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Nav
        public OrderStatus Status { get; set; } = null!;
        public ICollection<SalesOrderDetail> Details { get; set; } = new List<SalesOrderDetail>();
    }
}
