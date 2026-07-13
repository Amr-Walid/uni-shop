using System.ComponentModel.DataAnnotations;

namespace FTD.Application.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        [Required(ErrorMessage = "اسم المنتج بالعربية مطلوب")]
        [StringLength(200)]
        public string NameAr { get; set; } = "";
        [StringLength(200)]
        public string NameEn { get; set; } = "";
        [StringLength(100)]
        public string Slug { get; set; } = "";
        public string? ShortDescAr { get; set; }
        public string? ShortDescEn { get; set; }
        public string? DescAr { get; set; }
        public string? DescEn { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "السعر لا يمكن أن يكون سالباً")]
        public decimal Price { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "السعر القديم لا يمكن أن يكون سالباً")]
        public decimal? OldPrice { get; set; }
        public string? Badge { get; set; }
        public string? ImagePath { get; set; }
        public string? BrandName { get; set; }
        public string? Emoji { get; set; }
        public bool IsActive { get; set; }
        public bool IsFeatured { get; set; }
        public int SortOrder { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "المخزون لا يمكن أن يكون سالباً")]
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? MetaTitle { get; set; }
        public string? MetaDesc { get; set; }
        public string? FeaturesJson { get; set; }

        public CategoryDto? Category { get; set; }
        public BrandDto? Brand { get; set; }
        public List<ProductAttributeValueDto> AttributeValues { get; set; } = new();
        public List<ProductImageDto> Images { get; set; } = new();
    }

    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; } = "";
        public bool IsMain { get; set; }
        public int SortOrder { get; set; }
    }

    public class ProductAttributeDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string NameAr { get; set; } = "";
        public string NameEn { get; set; } = "";
        public int SortOrder { get; set; }
        public CategoryDto? Category { get; set; }
        public List<AttributeValueDto> Values { get; set; } = new();
    }

    public class AttributeValueDto
    {
        public int Id { get; set; }
        public int AttributeId { get; set; }
        public string ValueAr { get; set; } = "";
        public string ValueEn { get; set; } = "";
    }

    public class ProductAttributeValueDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int AttributeId { get; set; }
        public int AttributeValueId { get; set; }
        public ProductAttributeDto? Attribute { get; set; }
        public AttributeValueDto? AttributeValue { get; set; }
    }
}
