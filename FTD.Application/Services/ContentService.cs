using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class ContentService
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
    }
}
