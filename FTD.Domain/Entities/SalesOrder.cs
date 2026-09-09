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

        /// <summary>
        /// Owning customer account, or NULL for a guest checkout.
        ///
        /// Nullable by design: the storefront has always allowed guest orders and
        /// every historical row predates accounts. The FK uses
        /// DeleteBehavior.SetNull so deleting an account never cascades into the
        /// financial history (same principle as the SalesOrderDetail → Product
        /// Restrict rule from AUDIT_REPORT I-01).
        /// </summary>
        [MaxLength(450)] public string? UserId { get; set; }

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
        public AppUser? User { get; set; }
        public ICollection<SalesOrderDetail> Details { get; set; } = new List<SalesOrderDetail>();
    }
}
