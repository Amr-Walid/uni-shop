namespace FTD.Application.DTOs
{
    public class ContentBlockDto
    {
        public int Id { get; set; }
        public string Key { get; set; } = "";
        public string? TitleAr { get; set; }
        public string? TitleEn { get; set; }
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public string? ImagePath { get; set; }
        public string Type { get; set; } = "text";
        public DateTime UpdatedAt { get; set; }
    }

    public class ContentPageDto
    {
        public int Id { get; set; }
        public string Slug { get; set; } = "";
        public string TitleAr { get; set; } = "";
        public string TitleEn { get; set; } = "";
        public string? BodyAr { get; set; }
        public string? BodyEn { get; set; }
        public bool IsPublished { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<PageSectionDto> Sections { get; set; } = new();
    }

    public class PageSectionDto
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public string Type { get; set; } = "";
        public string? ContentJson { get; set; }
        public int SortOrder { get; set; }
        public bool IsVisible { get; set; }
    }

    public class NavigationItemDto
    {
        public int Id { get; set; }
        public string LabelAr { get; set; } = "";
        public string LabelEn { get; set; } = "";
        public string LinkType { get; set; } = "";
        public string? StaticRoute { get; set; }
        public string? PageSlug { get; set; }
        public string? ExternalUrl { get; set; }
        public bool OpenInNewTab { get; set; }
        public string Location { get; set; } = "";
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public NavigationItemDto? Parent { get; set; }
        public List<NavigationItemDto> Children { get; set; } = new();
    }
}
