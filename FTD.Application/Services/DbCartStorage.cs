using System;
using System.Threading.Tasks;
using FTD.Application.Interfaces;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    /// <summary>
    /// Database-backed <see cref="ICartStorage"/> for signed-in API clients.
    ///
    /// This is the payoff of the project's existing <c>ICartStorage</c>
    /// abstraction: <c>CartService</c> holds all the cart business logic and
    /// only ever asks for "raw storage", so persisting a cart in SQL instead of
    /// the browser session required NO change to that logic at all. The web app
    /// keeps using SessionCartStorage; the API uses this.
    ///
    /// LIFETIME: one instance per user per request scope. The API resolves it
    /// through a factory once the JWT has been validated, so a caller can never
    /// point it at another customer's cart.
    /// </summary>
    public class DbCartStorage : ICartStorage
    {
        private const string EmptyCart = "[]";

        private readonly IAppDbContext _db;
        private readonly string _userId;

        /// <summary>
        /// Cached row for this scope. The interface is synchronous (it was
        /// designed around ISession), so the row is loaded eagerly via
        /// <see cref="LoadAsync"/> before CartService touches it.
        /// </summary>
        private UserCart? _cart;
        private bool _loaded;

        public DbCartStorage(IAppDbContext db, string userId)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User id is required for a database-backed cart.", nameof(userId));
            _userId = userId;
        }

        /// <summary>
        /// Loads (or creates) the cart row for this user.
        ///
        /// Must be awaited before the storage is handed to CartService, because
        /// ICartStorage is synchronous and cannot await inside GetRaw(). Calling
        /// it twice is harmless.
        /// </summary>
        public async Task LoadAsync()
        {
            if (_loaded) return;

            _cart = await _db.UserCarts.FirstOrDefaultAsync(c => c.UserId == _userId);
            _loaded = true;
        }

        public string? GetRaw()
        {
            EnsureLoaded();
            return _cart?.CartJson;
        }

        public void SetRaw(string json)
        {
            EnsureLoaded();

            var payload = string.IsNullOrWhiteSpace(json) ? EmptyCart : json;

            if (_cart == null)
            {
                _cart = new UserCart
                {
                    UserId = _userId,
                    CartJson = payload,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.UserCarts.Add(_cart);
            }
            else
            {
                _cart.CartJson = payload;
                _cart.UpdatedAt = DateTime.UtcNow;
            }
        }

        public void Clear()
        {
            EnsureLoaded();

            // Reset to an empty array rather than deleting the row: keeping it
            // preserves CreatedAt (useful for abandoned-cart analytics) and
            // avoids a delete/insert cycle on every "clear cart".
            if (_cart != null)
            {
                _cart.CartJson = EmptyCart;
                _cart.UpdatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Persists pending changes. Kept explicit (rather than saving inside
        /// SetRaw) so several cart mutations can share one transaction.
        /// </summary>
        public Task SaveAsync() => _db.SaveChangesAsync();

        private void EnsureLoaded()
        {
            if (!_loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(DbCartStorage)} was used before {nameof(LoadAsync)} completed. " +
                    "Await LoadAsync() before passing this storage to CartService.");
            }
        }
    }
}
