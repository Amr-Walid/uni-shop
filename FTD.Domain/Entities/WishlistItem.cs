using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── WISHLIST ITEM ─────────────────────────────────────────────────────────
    /// <summary>
    /// A product a signed-in customer saved for later. A unique index on
    /// (UserId, ProductId) guarantees idempotent "add to wishlist" calls, so the
    /// mobile client can retry safely on a flaky connection without creating
    /// duplicate rows.
    /// </summary>
    public class WishlistItem
    {
        public int Id { get; set; }

        [Required, MaxLength(450)] public string UserId { get; set; } = "";
        public int ProductId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public AppUser? User { get; set; }
        public Product? Product { get; set; }
    }
}
