using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    /// <summary>
    /// Registry of push-notification targets. Sending is intentionally NOT part
    /// of this service — dispatch belongs to an Infrastructure adapter (FCM),
    /// while this layer only owns which devices exist and who they belong to.
    /// </summary>
    public class DeviceTokenService : IDeviceTokenService
    {
        private readonly IAppDbContext _db;

        public DeviceTokenService(IAppDbContext db) => _db = db;

        public async Task RegisterAsync(RegisterDeviceRequestDto request, string? userId)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Token)) return;

            var token = request.Token.Trim();
            var existing = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == token);

            var language = LocalizationHelper.NormalizeLanguage(request.Language);

            if (existing == null)
            {
                _db.DeviceTokens.Add(new DeviceToken
                {
                    Token = token,
                    UserId = userId,
                    Platform = request.Platform.Trim().ToLowerInvariant(),
                    DeviceModel = request.DeviceModel,
                    AppVersion = request.AppVersion,
                    Language = language,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastSeenAt = DateTime.UtcNow
                });
            }
            else
            {
                // Same installation re-registering (app restart, token refresh,
                // or a different account signing in on this handset). Update in
                // place — re-assigning UserId is what stops a device from
                // continuing to receive the previous user's notifications.
                existing.UserId = userId;
                existing.Platform = request.Platform.Trim().ToLowerInvariant();
                existing.DeviceModel = request.DeviceModel ?? existing.DeviceModel;
                existing.AppVersion = request.AppVersion ?? existing.AppVersion;
                existing.Language = language;
                existing.IsActive = true;
                existing.LastSeenAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<bool> UnregisterAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            var trimmed = token.Trim();
            var existing = await _db.DeviceTokens.FirstOrDefaultAsync(d => d.Token == trimmed);
            if (existing == null) return false;

            // Deactivate rather than delete: the row is a useful record of the
            // installation, and FCM may still report delivery results for it.
            existing.IsActive = false;
            existing.UserId = null;
            existing.LastSeenAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<DeviceToken>> GetActiveTokensForUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return new List<DeviceToken>();

            return await _db.DeviceTokens
                .AsNoTracking()
                .Where(d => d.UserId == userId && d.IsActive)
                .ToListAsync();
        }
    }
}
