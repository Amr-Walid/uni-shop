namespace FTD.Application.DTOs
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? LogoPath { get; set; }
        public string? BannerPath { get; set; }
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public string Slug { get; set; } = "";
        public string? ImagePath { get; set; }
        public string? Emoji { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
    }
}
