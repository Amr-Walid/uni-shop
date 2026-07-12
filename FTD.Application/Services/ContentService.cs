using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class ContentService : IContentService
    {
        private readonly IAppDbContext _db;
        public ContentService(IAppDbContext db) => _db = db;

        public async Task<Dictionary<string, string>> GetBlocksAsync()
        {
            var blocks = await _db.ContentBlocks.ToListAsync();
            var dict = new Dictionary<string, string>();
            foreach (var b in blocks)
            {
                if (!string.IsNullOrEmpty(b.BodyAr)) dict[b.Key + ".ar"] = b.BodyAr;
                if (!string.IsNullOrEmpty(b.BodyEn)) dict[b.Key + ".en"] = b.BodyEn;
                if (!string.IsNullOrEmpty(b.TitleAr)) dict[b.Key + ".icon"] = b.TitleAr;
                if (!string.IsNullOrEmpty(b.BodyAr)) dict[b.Key] = b.BodyAr;
            }
            return dict;
        }

        public async Task<string> GetBlockAsync(string key, string lang = "ar")
        {
            var block = await _db.ContentBlocks.FirstOrDefaultAsync(b => b.Key == key);
            if (block == null) return "";
            return lang == "en" ? (block.BodyEn ?? block.BodyAr ?? "") : (block.BodyAr ?? block.BodyEn ?? "");
        }

        public async Task<ContentPageDto?> GetPageBySlugAsync(string slug)
        {
            var entity = await _db.ContentPages
                .Include(p => p.Sections.OrderBy(s => s.SortOrder))
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

            return entity.ToDto();
        }

        public async Task<ContactInfoDto?> GetContactInfoAsync()
        {
            var entity = await _db.ContactInfos.FirstOrDefaultAsync();
            return entity.ToDto();
        }

        public async Task<Dictionary<string, string>> GetSettingsAsync()
        {
            var settings = await _db.SiteSettings.ToListAsync();
            return settings.ToDictionary(s => s.Key, s => s.Value ?? "");
        }

        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
            return s?.Value ?? defaultValue;
        }

        public async Task<List<SiteSettingDto>> GetSettingsListAsync()
        {
            var settings = await _db.SiteSettings.ToListAsync();
            return settings.Select(s => s.ToDto()).Where(s => s != null).Select(s => s!).ToList();
        }

        public async Task<List<NavigationItemDto>> GetNavigationItemsAsync()
        {
            var entities = await _db.NavigationItems
                .Where(n => n.IsActive)
                .OrderBy(n => n.SortOrder)
                .ToListAsync();

            return entities.Select(n => n.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<List<BrandDto>> GetActiveBrandsAsync()
        {
            var entities = await _db.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ToListAsync();

            return entities.Select(b => b.ToDto()).Where(dto => dto != null).Select(dto => dto!).ToList();
        }

        public async Task<List<ContentBlockDto>> GetBlocksListAsync()
        {
            var blocks = await _db.ContentBlocks.OrderBy(b => b.Key).ToListAsync();
            return blocks.Select(b => b.ToDto()).Where(b => b != null).Select(b => b!).ToList();
        }

        public async Task SaveBlockAsync(int id, string? bodyAr, string? bodyEn, string? titleAr)
        {
            var block = await _db.ContentBlocks.FindAsync(id);
            if (block != null)
            {
                block.BodyAr = bodyAr;
                block.BodyEn = bodyEn;
                if (titleAr != null) block.TitleAr = titleAr;
                block.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<ContentPageDto>> GetAllPagesAsync()
        {
            var pages = await _db.ContentPages.OrderBy(p => p.Slug).ToListAsync();
            return pages.Select(p => p.ToDto()).Where(p => p != null).Select(p => p!).ToList();
        }

        public async Task<ContentPageDto> CreatePageAsync(ContentPageDto dto)
        {
            var page = new ContentPage
            {
                Slug = dto.Slug,
                TitleAr = dto.TitleAr,
                TitleEn = dto.TitleEn,
                BodyAr = dto.BodyAr,
                BodyEn = dto.BodyEn,
                IsPublished = dto.IsPublished,
                MetaTitle = dto.MetaTitle,
                MetaDesc = dto.MetaDesc,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.ContentPages.Add(page);
            await _db.SaveChangesAsync();
            return page.ToDto()!;
        }

        public async Task<ContentPageDto> UpdatePageAsync(int id, ContentPageDto dto)
        {
            var page = await _db.ContentPages.FindAsync(id);
            if (page == null) throw new ArgumentException("Page not found");

            page.Slug = string.IsNullOrEmpty(dto.Slug) ? page.Slug : dto.Slug;
            page.TitleAr = dto.TitleAr;
            page.TitleEn = dto.TitleEn;
            page.BodyAr = dto.BodyAr;
            page.BodyEn = dto.BodyEn;
            page.IsPublished = dto.IsPublished;
            page.MetaTitle = dto.MetaTitle;
            page.MetaDesc = dto.MetaDesc;
            page.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return page.ToDto()!;
        }

        public async Task DeletePageAsync(int id)
        {
            var page = await _db.ContentPages.FindAsync(id);
            if (page != null)
            {
                _db.ContentPages.Remove(page);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<ContentPageDto?> GetPageWithSectionsAsync(int id)
        {
            var page = await _db.ContentPages
                .Include(p => p.Sections)
                .FirstOrDefaultAsync(p => p.Id == id);
            return page.ToDto();
        }

        public async Task AddPageSectionAsync(int pageId, string type, string defaultJson)
        {
            var page = await _db.ContentPages.Include(p => p.Sections).FirstOrDefaultAsync(p => p.Id == pageId);
            if (page == null) throw new ArgumentException("Page not found");

            var maxOrder = page.Sections.Any() ? page.Sections.Max(s => s.SortOrder) : 0;

            _db.PageSections.Add(new PageSection
            {
                PageId = pageId,
                Type = type,
                ContentJson = defaultJson,
                SortOrder = maxOrder + 1,
                IsVisible = true
            });
            await _db.SaveChangesAsync();
        }

        public async Task<PageSectionDto?> GetPageSectionAsync(int sectionId)
        {
            var section = await _db.PageSections.FindAsync(sectionId);
            return section.ToDto();
        }

        public async Task SavePageSectionContentAsync(int sectionId, string contentJson)
        {
            var section = await _db.PageSections.FindAsync(sectionId);
            if (section == null) throw new ArgumentException("Section not found");

            section.ContentJson = contentJson;
            await _db.SaveChangesAsync();
        }

        public async Task DeletePageSectionAsync(int sectionId)
        {
            var section = await _db.PageSections.FindAsync(sectionId);
            if (section != null)
            {
                _db.PageSections.Remove(section);
                await _db.SaveChangesAsync();
            }
        }

        public async Task TogglePageSectionVisibilityAsync(int sectionId)
        {
            var section = await _db.PageSections.FindAsync(sectionId);
            if (section == null) throw new ArgumentException("Section not found");

            section.IsVisible = !section.IsVisible;
            await _db.SaveChangesAsync();
        }

        public async Task MovePageSectionUpAsync(int sectionId)
        {
            var section = await _db.PageSections.FindAsync(sectionId);
            if (section == null) throw new ArgumentException("Section not found");

            var prev = await _db.PageSections
                .Where(s => s.PageId == section.PageId && s.SortOrder < section.SortOrder)
                .OrderByDescending(s => s.SortOrder).FirstOrDefaultAsync();

            if (prev != null)
            {
                (section.SortOrder, prev.SortOrder) = (prev.SortOrder, section.SortOrder);
                await _db.SaveChangesAsync();
            }
        }

        public async Task MovePageSectionDownAsync(int sectionId)
        {
            var section = await _db.PageSections.FindAsync(sectionId);
            if (section == null) throw new ArgumentException("Section not found");

            var next = await _db.PageSections
                .Where(s => s.PageId == section.PageId && s.SortOrder > section.SortOrder)
                .OrderBy(s => s.SortOrder).FirstOrDefaultAsync();

            if (next != null)
            {
                (section.SortOrder, next.SortOrder) = (next.SortOrder, section.SortOrder);
                await _db.SaveChangesAsync();
            }
        }

        // Bulk reorder — sets SortOrder to match the exact order of the received IDs.
        // All IDs must belong to the same page (validated) to prevent cross-page tampering.
        public async Task UpdatePageSectionsOrderAsync(List<int> orderedSectionIds)
        {
            if (orderedSectionIds == null || orderedSectionIds.Count == 0) return;

            var sections = await _db.PageSections
                .Where(s => orderedSectionIds.Contains(s.Id))
                .ToListAsync();

            if (sections.Count == 0) return;

            // Safety: all sections must belong to a single page
            var pageId = sections[0].PageId;
            if (sections.Any(s => s.PageId != pageId))
                throw new ArgumentException("All sections must belong to the same page");

            for (int i = 0; i < orderedSectionIds.Count; i++)
            {
                var sec = sections.FirstOrDefault(s => s.Id == orderedSectionIds[i]);
                if (sec != null) sec.SortOrder = i + 1;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<List<NavigationItemDto>> GetAllNavigationItemsForAdminAsync()
        {
            var items = await _db.NavigationItems
                .Where(n => n.ParentId == null)
                .OrderBy(n => n.SortOrder)
                .ToListAsync();

            return items.Select(n => n.ToDto()).Where(n => n != null).Select(n => n!).ToList();
        }

        public async Task CreateNavigationItemAsync(NavigationItemDto dto)
        {
            var item = new NavigationItem
            {
                LabelAr = dto.LabelAr,
                LabelEn = dto.LabelEn,
                LinkType = dto.LinkType,
                StaticRoute = dto.StaticRoute,
                PageSlug = dto.PageSlug,
                ExternalUrl = dto.ExternalUrl,
                OpenInNewTab = dto.OpenInNewTab,
                Location = dto.Location,
                ParentId = dto.ParentId,
                SortOrder = dto.SortOrder,
                IsActive = true
            };

            _db.NavigationItems.Add(item);
            await _db.SaveChangesAsync();
        }

        public async Task SetNavigationItemLocationAsync(int id, string location)
        {
            var item = await _db.NavigationItems.FindAsync(id);
            if (item != null)
            {
                item.Location = location;
                item.IsActive = location != "Hidden";
                await _db.SaveChangesAsync();
            }
        }

        public async Task DeleteNavigationItemAsync(int id)
        {
            var item = await _db.NavigationItems.FindAsync(id);
            if (item != null)
            {
                _db.NavigationItems.Remove(item);
                await _db.SaveChangesAsync();
            }
        }

        public async Task SaveSiteSettingsAsync(Dictionary<int, string> values)
        {
            foreach (var kv in values)
            {
                var s = await _db.SiteSettings.FindAsync(kv.Key);
                if (s != null)
                {
                    s.Value = kv.Value;
                    s.UpdatedAt = DateTime.UtcNow;
                }
            }
            await _db.SaveChangesAsync();
        }

        public async Task SaveSettingByKeyAsync(string key, string? value, string? description = null, string type = "text")
        {
            var s = await _db.SiteSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (s == null)
            {
                _db.SiteSettings.Add(new SiteSetting
                {
                    Key = key,
                    Value = value,
                    Description = description,
                    Type = type,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                s.Value = value;
                if (description != null) s.Description = description;
                s.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }

        public async Task SaveBlockByKeyAsync(string key, string? bodyAr, string? bodyEn, string? titleAr = null)
        {
            var block = await _db.ContentBlocks.FirstOrDefaultAsync(b => b.Key == key);
            if (block == null)
            {
                _db.ContentBlocks.Add(new ContentBlock
                {
                    Key = key,
                    BodyAr = bodyAr,
                    BodyEn = bodyEn,
                    TitleAr = titleAr,
                    Type = "text",
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                block.BodyAr = bodyAr;
                block.BodyEn = bodyEn;
                if (titleAr != null) block.TitleAr = titleAr;
                block.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }

        public async Task SaveContactInfoAsync(ContactInfoDto dto)
        {
            var contact = await _db.ContactInfos.FirstOrDefaultAsync();
            if (contact == null)
            {
                var newContact = new ContactInfo
                {
                    Phone = dto.Phone,
                    Phone2 = dto.Phone2,
                    Email = dto.Email,
                    AddressAr = dto.AddressAr,
                    AddressEn = dto.AddressEn,
                    City = dto.City,
                    Facebook = dto.Facebook,
                    Instagram = dto.Instagram,
                    WhatsApp = dto.WhatsApp,
                    TikTok = dto.TikTok,
                    WorkingHoursAr = dto.WorkingHoursAr,
                    WorkingHoursEn = dto.WorkingHoursEn,
                    MapEmbedUrl = dto.MapEmbedUrl,
                    ShowPhone = dto.ShowPhone,
                    ShowPhone2 = dto.ShowPhone2,
                    ShowEmail = dto.ShowEmail,
                    ShowAddress = dto.ShowAddress,
                    ShowMap = dto.ShowMap,
                    ShowWorkingHours = dto.ShowWorkingHours,
                    ShowFacebook = dto.ShowFacebook,
                    ShowInstagram = dto.ShowInstagram,
                    ShowWhatsApp = dto.ShowWhatsApp,
                    ShowTikTok = dto.ShowTikTok,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.ContactInfos.Add(newContact);
            }
            else
            {
                contact.Phone = dto.Phone;
                contact.Phone2 = dto.Phone2;
                contact.Email = dto.Email;
                contact.AddressAr = dto.AddressAr;
                contact.AddressEn = dto.AddressEn;
                contact.City = dto.City;
                contact.Facebook = dto.Facebook;
                contact.Instagram = dto.Instagram;
                contact.WhatsApp = dto.WhatsApp;
                contact.TikTok = dto.TikTok;
                contact.WorkingHoursAr = dto.WorkingHoursAr;
                contact.WorkingHoursEn = dto.WorkingHoursEn;
                contact.MapEmbedUrl = dto.MapEmbedUrl;
                contact.ShowPhone = dto.ShowPhone;
                contact.ShowPhone2 = dto.ShowPhone2;
                contact.ShowEmail = dto.ShowEmail;
                contact.ShowAddress = dto.ShowAddress;
                contact.ShowMap = dto.ShowMap;
                contact.ShowWorkingHours = dto.ShowWorkingHours;
                contact.ShowFacebook = dto.ShowFacebook;
                contact.ShowInstagram = dto.ShowInstagram;
                contact.ShowWhatsApp = dto.ShowWhatsApp;
                contact.ShowTikTok = dto.ShowTikTok;
                contact.UpdatedAt = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
        }
    }
}
