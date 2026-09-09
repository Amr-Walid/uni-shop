using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    /// <summary>
    /// Saved-for-later products for a signed-in customer.
    /// All operations are scoped by userId supplied by the caller from the
    /// validated JWT — never from the request body — so one customer can never
    /// read or mutate another's wishlist.
    /// </summary>
    public interface IWishlistService
    {
        Task<PagedResult<WishlistItemDto>> GetAsync(string userId, int page, int pageSize, string lang, string? mediaBaseUrl);

        /// <summary>
        /// Adds a product. Idempotent: adding an already-saved product succeeds
        /// without creating a duplicate, so a retried mobile request is safe.
        /// Returns false when the product does not exist or is inactive.
        /// </summary>
        Task<bool> AddAsync(string userId, int productId);

        Task<bool> RemoveAsync(string userId, int productId);

        Task<bool> ContainsAsync(string userId, int productId);

        Task<int> GetCountAsync(string userId);
    }
}
