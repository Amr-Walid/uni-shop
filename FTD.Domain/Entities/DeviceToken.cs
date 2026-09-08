using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── DEVICE TOKEN ──────────────────────────────────────────────────────────
    /// <summary>
    /// A push-notification target (FCM / APNs registration token) for one
    /// installation of the mobile app.
    ///
    /// <see cref="UserId"/> is nullable on purpose: guests must still receive
    /// "your order has shipped" notifications, and the token is later linked to
    /// the account when the same device signs in.
    /// </summary>
    public class DeviceToken
    {
        public int Id { get; set; }

        [MaxLength(450)] public string? UserId { get; set; }

        /// <summary>FCM/APNs registration token. Unique per installation.</summary>
        [Required, MaxLength(500)] public string Token { get; set; } = "";

        /// <summary>"android" | "ios" | "web".</summary>
        [Required, MaxLength(20)] public string Platform { get; set; } = "";

        [MaxLength(100)] public string? DeviceModel { get; set; }
        [MaxLength(30)] public string? AppVersion { get; set; }

        /// <summary>Language used when composing notification text for this device.</summary>
        [MaxLength(5)] public string Language { get; set; } = "ar";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

        // Nav
        public AppUser? User { get; set; }
    }
}
