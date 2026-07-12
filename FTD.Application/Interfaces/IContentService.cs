using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    public interface IContentService
    {
        Task<Dictionary<string, string>> GetBlocksAsync();
        Task<string> GetBlockAsync(string key, string lang = "ar");
        Task<ContentPageDto?> GetPageBySlugAsync(string slug);
        Task<ContactInfoDto?> GetContactInfoAsync();
        Task<Dictionary<string, string>> GetSettingsAsync();
        Task<string> GetSettingAsync(string key, string defaultValue = "");
        Task<List<SiteSettingDto>> GetSettingsListAsync();
        Task<List<NavigationItemDto>> GetNavigationItemsAsync();
        Task<List<BrandDto>> GetActiveBrandsAsync();
        Task<List<ContentBlockDto>> GetBlocksListAsync();
        Task SaveBlockAsync(int id, string? bodyAr, string? bodyEn, string? titleAr);
        Task<List<ContentPageDto>> GetAllPagesAsync();
        Task<ContentPageDto> CreatePageAsync(ContentPageDto dto);
        Task<ContentPageDto> UpdatePageAsync(int id, ContentPageDto dto);
        Task<ContentPageDto?> GetPageWithSectionsAsync(int id);
        Task AddPageSectionAsync(int pageId, string type, string defaultJson);
        Task<PageSectionDto?> GetPageSectionAsync(int sectionId);
        Task SavePageSectionContentAsync(int sectionId, string contentJson);
        Task DeletePageSectionAsync(int sectionId);
        Task TogglePageSectionVisibilityAsync(int sectionId);
        Task MovePageSectionUpAsync(int sectionId);
        Task MovePageSectionDownAsync(int sectionId);
        Task<List<NavigationItemDto>> GetAllNavigationItemsForAdminAsync();
        Task CreateNavigationItemAsync(NavigationItemDto dto);
        Task SetNavigationItemLocationAsync(int id, string location);
        Task DeleteNavigationItemAsync(int id);
        Task SaveSiteSettingsAsync(Dictionary<int, string> values);
        Task SaveSettingByKeyAsync(string key, string? value, string? description = null, string type = "text");
        Task SaveBlockByKeyAsync(string key, string? bodyAr, string? bodyEn, string? titleAr = null);
        Task SaveContactInfoAsync(ContactInfoDto dto);
    }
}
