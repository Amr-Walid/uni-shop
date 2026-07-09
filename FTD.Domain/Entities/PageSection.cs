using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── PAGE SECTION ──────────────────────────────────────────────────────────
    public class PageSection
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        [Required, MaxLength(50)] public string Type { get; set; } = "RichText";
        // RichText | FAQ | Cards | Hero | ContactForm
        public string? ContentJson { get; set; }   // JSON payload depends on Type
        public int SortOrder { get; set; } = 0;
        public bool IsVisible { get; set; } = true;

        public ContentPage Page { get; set; } = null!;
    }
}
