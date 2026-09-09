using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FTD.Domain.Entities
{
    // ── APPLICATION USER ──────────────────────────────────────────────────────
    /// <summary>
    /// Extends <see cref="IdentityUser"/> with the storefront profile fields a
    /// mobile customer needs (display name, default shipping address, language).
    ///
    /// Why this exists: the original solution used the bare <c>IdentityUser</c>,
    /// which has no room for a customer profile and no link to orders. Mobile
    /// screens such as "my orders", "my addresses" and personalised push
    /// notifications are impossible without it.
    ///
    /// All added columns are nullable (or have defaults) so the migration is
    /// non-destructive for the existing admin account.
    /// </summary>
    public class AppUser : IdentityUser
    {
        [MaxLength(150)] public string? FullName { get; set; }

        // Default shipping address — pre-fills the checkout form on mobile.
        // Lengths mirror SalesOrder so a saved profile can never overflow the
        // order columns when copied into a new order.
        [MaxLength(300)] public string? DefaultAddress { get; set; }
        [MaxLength(100)] public string? City { get; set; }
        [MaxLength(100)] public string? Governorate { get; set; }

        /// <summary>UI language for API responses and push notifications ("ar" | "en").</summary>
        [MaxLength(5)] public string PreferredLanguage { get; set; } = "ar";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Set when the customer deletes their own account (Apple App Store
        /// requirement). We anonymise instead of hard-deleting so historical
        /// orders — and therefore revenue reports — stay intact.
        /// </summary>
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Nav
        public ICollection<SalesOrder> Orders { get; set; } = new List<SalesOrder>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    }
}
