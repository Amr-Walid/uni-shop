using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── IDEMPOTENCY RECORD ────────────────────────────────────────────────────
    /// <summary>
    /// Remembers the outcome of a mutating request that carried an
    /// <c>Idempotency-Key</c> header, so replaying the same key returns the
    /// original result instead of performing the action twice.
    ///
    /// WHY THIS MATTERS ON MOBILE: cellular connections drop mid-request all the
    /// time. The client cannot tell "the order was never created" from "the
    /// order was created but the response was lost", so it retries — and without
    /// this table the customer is charged for two identical orders and stock is
    /// deducted twice.
    /// </summary>
    public class IdempotencyRecord
    {
        public int Id { get; set; }

        /// <summary>Client-supplied key (unique index).</summary>
        [Required, MaxLength(120)] public string Key { get; set; } = "";

        /// <summary>Endpoint the key was used against — prevents cross-endpoint replay.</summary>
        [Required, MaxLength(120)] public string Endpoint { get; set; } = "";

        /// <summary>
        /// Hash of the request body. A key replayed with a DIFFERENT payload is a
        /// client bug (or an attack) and must be rejected with 422 rather than
        /// silently returning an unrelated result.
        /// </summary>
        [MaxLength(200)] public string? RequestHash { get; set; }

        /// <summary>Serialized successful response, replayed verbatim on retry.</summary>
        public string? ResponseJson { get; set; }

        public int StatusCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Records are pruned after this instant (default: 24 hours).</summary>
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);
    }
}
