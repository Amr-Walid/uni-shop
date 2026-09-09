using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Domain.Entities;

namespace FTD.Application.Interfaces
{
    /// <summary>
    /// Registry of push-notification targets (FCM/APNs tokens).
    ///
    /// Registration accepts a null userId so guests still receive order-status
    /// notifications; the token is later attached to the account when that same
    /// device signs in.
    /// </summary>
    public interface IDeviceTokenService
    {
        /// <summary>
        /// Registers or refreshes a device. Idempotent on the token value: the
        /// same installation re-registering updates its row (owner, language,
        /// app version, last-seen) instead of inserting a duplicate.
        /// </summary>
        Task RegisterAsync(RegisterDeviceRequestDto request, string? userId);

        /// <summary>Deactivates a token — used on sign-out so the next account
        /// on this device does not inherit the previous user's notifications.</summary>
        Task<bool> UnregisterAsync(string token);

        /// <summary>Active tokens for a user, for fan-out when their order changes.</summary>
        Task<List<DeviceToken>> GetActiveTokensForUserAsync(string userId);
    }
}
