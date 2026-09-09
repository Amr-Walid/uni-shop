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
    /// <summary>
    /// Saved-for-later products.
    ///
    /// SECURITY: every method filters on the caller-supplied userId, which the
    /// API always takes from the validated JWT and never from the request body.
    /// There is no code path that lets one customer address another's wishlist.
    /// </summary>
    public class WishlistService : IWishlistService
    {
        private readonly IAppDbContext _db;

        public WishlistService(IAppDbContext db) => _db = db;

        public async Task<PagedResult<WishlistItemDto>> GetAsync(
            string userId, int page, int pageSize, string lang, string? mediaBaseUrl)
        {
            var (normalizedPage, normalizedSize) = PagedResult<WishlistItemDto>.Normalize(page, pageSize);

            if (string.IsNullOrWhiteSpace(userId))
                return PagedResult<WishlistItemDto>.Empty(normalizedPage, normalizedSize);

            // Only surface rows whose product is still active — a product the
            // admin has since deactivated must not appear as buyable.
            var query = _db.WishlistItems
                .AsNoTracking()
                .Where(w => w.UserId == userId && w.Product != null && w.Product.IsActive);

            var totalCount = await query.CountAsync();
            if (totalCount == 0)
                return PagedResult<WishlistItemDto>.Empty(normalizedPage, normalizedSize);

            var rows = await query
                .Include(w => w.Product!).ThenInclude(p => p.Category)
                .Include(w => w.Product!).ThenInclude(p => p.Brand)
                // Newest first, with an Id tie-break for stable paging.
                .OrderByDescending(w => w.CreatedAt)
                .ThenBy(w => w.Id)
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToListAsync();

            var items = rows
                .Where(w => w.Product != null)
                .Select(w => new WishlistItemDto
                {
                    Id = w.Id,
                    CreatedAt = w.CreatedAt,
                    Product = w.Product!.ToListItem(lang, mediaBaseUrl)
                })
                .ToList();

            return new PagedResult<WishlistItemDto>(items, normalizedPage, normalizedSize, totalCount);
        }

        public async Task<bool> AddAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId) || productId <= 0) return false;

            // Reject unknown/inactive products up front so the wishlist cannot
            // accumulate references to things the customer can never buy.
            var productExists = await _db.Products
                .AsNoTracking()
                .AnyAsync(p => p.Id == productId && p.IsActive);

            if (!productExists) return false;

            var alreadySaved = await _db.WishlistItems
                .AsNoTracking()
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);

            // IDEMPOTENT: re-adding reports success without inserting again, so a
            // retried request on a flaky mobile connection is safe. The unique
            // (UserId, ProductId) index is the backstop against a race.
            if (alreadySaved) return true;

            _db.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two concurrent adds raced past the AnyAsync check; the unique
                // index rejected the second. The user's intent is satisfied
                // either way, so this is a success, not an error.
                return true;
            }

            return true;
        }

        public async Task<bool> RemoveAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId) || productId <= 0) return false;

            var item = await _db.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (item == null) return false;

            _db.WishlistItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<bool> ContainsAsync(string userId, int productId)
        {
            if (string.IsNullOrWhiteSpace(userId) || productId <= 0) return Task.FromResult(false);

            return _db.WishlistItems
                .AsNoTracking()
                .AnyAsync(w => w.UserId == userId && w.ProductId == productId);
        }

        public Task<int> GetCountAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return Task.FromResult(0);

            return _db.WishlistItems
                .AsNoTracking()
                .CountAsync(w => w.UserId == userId && w.Product != null && w.Product.IsActive);
        }
    }
}
