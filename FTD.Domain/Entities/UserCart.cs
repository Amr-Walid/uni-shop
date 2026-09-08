using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── USER CART ─────────────────────────────────────────────────────────────
    /// <summary>
    /// Server-side shopping cart for a signed-in customer, so a cart started on
    /// the phone survives a reinstall and is visible on the website too.
    ///
    /// DESIGN: the cart payload is stored as the SAME JSON shape the existing
    /// session cart uses (<c>[{ "ProductId": 1, "Qty": 2 }]</c>). That lets a
    /// database-backed <c>ICartStorage</c> implementation reuse
    /// <c>CartService</c> verbatim — no business logic is duplicated or changed.
    /// </summary>
    public class UserCart
    {
        public int Id { get; set; }

        [Required, MaxLength(450)] public string UserId { get; set; } = "";

        /// <summary>Raw cart JSON — same contract as the session cart storage.</summary>
        public string CartJson { get; set; } = "[]";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Nav
        public AppUser? User { get; set; }
    }
}
