using System;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── CONTACT MESSAGE ───────────────────────────────────────────────────────
    public class ContactMessage
    {
        public int Id { get; set; }
        [MaxLength(100)] public string? Name { get; set; }
        [MaxLength(100)] public string? Email { get; set; }
        [MaxLength(20)] public string? Phone { get; set; }
        public string? Message { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
