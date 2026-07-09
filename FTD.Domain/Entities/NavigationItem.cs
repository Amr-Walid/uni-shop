using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FTD.Domain.Entities
{
    // ── NAVIGATION ITEM ───────────────────────────────────────────────────────
    public class NavigationItem
    {
        public int Id { get; set; }
        [Required, MaxLength(100)] public string LabelAr { get; set; } = "";
        [Required, MaxLength(100)] public string LabelEn { get; set; } = "";

        // Type: Static | ContentPage | External
        [Required, MaxLength(20)] public string LinkType { get; set; } = "Static";
        public string? StaticRoute { get; set; }
        public string? PageSlug { get; set; }
        public string? ExternalUrl { get; set; }
        public bool OpenInNewTab { get; set; } = false;

        // Location: Navbar | Footer | Both | Hidden
        [Required, MaxLength(20)] public string Location { get; set; } = "Both";

        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        public NavigationItem? Parent { get; set; }
        public ICollection<NavigationItem> Children { get; set; } = new List<NavigationItem>();
    }
}
