using System.ComponentModel.DataAnnotations;

namespace FTD.Application.DTOs
{
    public class BrandDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "اسم البراند بالعربية مطلوب")]
        [StringLength(100)]
        public string NameAr { get; set; } = "";
        [StringLength(100)]
        public string NameEn { get; set; } = "";
        [StringLength(100)]
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
        [Required(ErrorMessage = "اسم القسم بالعربية مطلوب")]
        [StringLength(150)]
        public string NameAr { get; set; } = "";
        [StringLength(150)]
        public string NameEn { get; set; } = "";
        [StringLength(100)]
        public string Slug { get; set; } = "";
        public string? ImagePath { get; set; }
        public string? Emoji { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool ShowOnHomepage { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public int ProductsCount { get; set; }
    }
}
