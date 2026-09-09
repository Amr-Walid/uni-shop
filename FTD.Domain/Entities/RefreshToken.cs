using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── REFRESH TOKEN ─────────────────────────────────────────────────────────
    /// <summary>
    /// A long-lived, rotating credential that lets a mobile app exchange an
    /// expired 15-minute access token for a fresh one without asking the user
    /// to sign in again.
    ///
    /// SECURITY NOTES
    /// • Only the SHA-256 <see cref="TokenHash"/> is persisted — never the raw
    ///   token. A database leak therefore cannot be replayed against the API.
    /// • Tokens ROTATE: every successful refresh revokes the presented token and
    ///   issues a new one, recording the successor in <see cref="ReplacedByHash"/>.
    /// • Presenting an already-revoked token means the credential was stolen
    ///   (the legitimate client has since rotated it), so the whole token family
    ///   is revoked — see IAuthService.RefreshAsync.
    /// </summary>
    public class RefreshToken
    {
        public int Id { get; set; }

        [Required, MaxLength(450)] public string UserId { get; set; } = "";

        /// <summary>SHA-256 (Base64) of the raw token. Never store the original.</summary>
        [Required, MaxLength(200)] public string TokenHash { get; set; } = "";

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }
        [MaxLength(200)] public string? ReplacedByHash { get; set; }

        /// <summary>
        /// Groups all tokens descended from one login so a reuse attack can
        /// revoke the entire chain in a single UPDATE.
        /// </summary>
        [Required, MaxLength(64)] public string FamilyId { get; set; } = "";

        // Diagnostics for a future "active devices" screen.
        [MaxLength(200)] public string? DeviceInfo { get; set; }
        [MaxLength(64)] public string? CreatedByIp { get; set; }

        // Nav
        public AppUser? User { get; set; }

        // Computed (not mapped — expression-bodied properties are ignored by EF)
        public bool IsRevoked => RevokedAt != null;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
