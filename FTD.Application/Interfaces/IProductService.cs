using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetFeaturedAsync(int take = 6);
        Task<List<CategoryDto>> GetActiveCategoriesAsync();
        Task<List<CategoryDto>> GetAllCategoriesAsync();
        Task<List<ProductDto>> GetAllActiveAsync();
        Task<List<ProductDto>> GetByCategoryAsync(int categoryId);
        Task<List<ProductDto>> GetByBrandAsync(string brandSlug);
        Task<List<ProductDto>> GetFilteredBySlugAsync(List<string>? brandSlugs, List<string>? categorySlugs, List<int>? attributeValueIds, string? sortBy);
        Task<(List<CategoryDto> AvailableCategories, List<BrandDto> AvailableBrands)> GetAvailableFacetsAsync(List<string>? brandSlugs, List<string>? categorySlugs);
        Task<List<ProductDto>> GetFilteredByIdAsync(int? categoryId, int? brandId, string? query);
        Task<ProductDto?> GetBySlugAsync(string slug);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<List<ProductDto>> GetRelatedAsync(int productId, int categoryId, int take = 4);
        Task<List<ProductDto>> SearchAsync(string query);
        Task<List<ProductDto>> GetAllProductsForAdminAsync();
        Task<ProductDto> CreateProductAsync(ProductDto dto, List<ProductImageDto> additionalImages, Dictionary<int, int> attributes);
        Task<ProductDto> UpdateProductAsync(int id, ProductDto dto, List<ProductImageDto> additionalImages, Dictionary<int, int> attributes);
        Task<bool> DeleteProductAsync(int id);
        Task<ProductImageDto?> DeleteProductImageAsync(int imageId);
        Task<List<BrandDto>> GetAllBrandsAsync();
        Task<BrandDto> CreateBrandAsync(BrandDto dto);
        Task<BrandDto> UpdateBrandAsync(int id, BrandDto dto);
        Task<List<ProductAttributeDto>> GetAttributesWithDetailsAsync(int? categoryId);
        Task<ProductAttributeDto> CreateAttributeAsync(ProductAttributeDto dto);
        Task<ProductAttributeDto> UpdateAttributeAsync(int id, ProductAttributeDto dto);
        Task<bool> DeleteAttributeAsync(int id);
        Task<AttributeValueDto> AddAttributeValueAsync(int attributeId, string valueAr, string valueEn);
        Task<bool> DeleteAttributeValueAsync(int valueId);
        Task<List<AttributeFilterGroupDto>> BuildAttributeGroupsAsync(List<ProductDto> products);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto dto);
        Task<CategoryDto> UpdateCategoryAsync(int id, CategoryDto dto);
    }
}
