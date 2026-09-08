using System;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Mappers;
using FTD.Application.Services;
using Xunit;

namespace FTD.Tests
{
    /// <summary>
    /// Localization, media-URL, discount and feature-parsing helpers. These are
    /// pure functions applied to every catalog response, so a regression here
    /// would be visible on every screen in the app.
    /// </summary>
    public class HelperTests
    {
        [Theory]
        [InlineData("ar", "ar")]
        [InlineData("en", "en")]
        [InlineData("en-US", "en")]
        [InlineData("ar-EG", "ar")]
        // Real HTTP clients send weighted lists; the highest-priority tag wins.
        [InlineData("en-US,en;q=0.9,ar;q=0.8", "en")]
        [InlineData("fr-FR", "ar")]   // unsupported -> Arabic default
        [InlineData("", "ar")]
        [InlineData(null, "ar")]
        public void NormalizeLanguage_ResolvesSupportedCode(string? input, string expected)
            => Assert.Equal(expected, LocalizationHelper.NormalizeLanguage(input));

        [Fact]
        public void Pick_PrefersRequestedLanguage()
        {
            Assert.Equal("تابلت", LocalizationHelper.Pick("تابلت", "Tablet", "ar"));
            Assert.Equal("Tablet", LocalizationHelper.Pick("تابلت", "Tablet", "en"));
        }

        /// <summary>
        /// Admins commonly fill only Arabic. The fallback is what stops English
        /// clients from rendering blank product names.
        /// </summary>
        [Fact]
        public void Pick_FallsBackWhenPreferredIsBlank()
        {
            Assert.Equal("تابلت", LocalizationHelper.Pick("تابلت", "", "en"));
            Assert.Equal("تابلت", LocalizationHelper.Pick("تابلت", "   ", "en"));
            Assert.Equal("Tablet", LocalizationHelper.Pick(null, "Tablet", "ar"));
        }

        [Fact]
        public void Pick_ReturnsEmptyWhenBothMissing()
            => Assert.Equal(string.Empty, LocalizationHelper.Pick(null, null, "ar"));

        /// <summary>
        /// Optional text returns null (not "") so the client can hide the
        /// element instead of rendering an empty line.
        /// </summary>
        [Fact]
        public void PickOptional_ReturnsNullWhenBothMissing()
            => Assert.Null(LocalizationHelper.PickOptional("", "", "ar"));

        // ── Media URLs ────────────────────────────────────────────────────────

        [Fact]
        public void ToAbsolute_PrefixesRelativePath()
            => Assert.Equal("https://cdn.test/images/a.jpg",
                MediaUrlHelper.ToAbsolute("/images/a.jpg", "https://cdn.test"));

        [Fact]
        public void ToAbsolute_HandlesTrailingAndLeadingSlashes()
            => Assert.Equal("https://cdn.test/images/a.jpg",
                MediaUrlHelper.ToAbsolute("images/a.jpg", "https://cdn.test/"));

        /// <summary>
        /// An admin may paste a full CDN URL. Prefixing it again would produce a
        /// broken link, so absolute values must pass through untouched.
        /// </summary>
        [Fact]
        public void ToAbsolute_LeavesAbsoluteUrlsUnchanged()
        {
            Assert.Equal("https://other.cdn/x.jpg",
                MediaUrlHelper.ToAbsolute("https://other.cdn/x.jpg", "https://cdn.test"));
            Assert.Equal("http://other.cdn/x.jpg",
                MediaUrlHelper.ToAbsolute("http://other.cdn/x.jpg", "https://cdn.test"));
        }

        [Fact]
        public void ToAbsolute_ReturnsNullForBlankInput()
        {
            // Null distinguishes "no image" (render the emoji placeholder) from
            // a broken link.
            Assert.Null(MediaUrlHelper.ToAbsolute(null, "https://cdn.test"));
            Assert.Null(MediaUrlHelper.ToAbsolute("   ", "https://cdn.test"));
        }

        [Fact]
        public void ToAbsolute_KeepsRelativePathWhenNoBaseConfigured()
            => Assert.Equal("/images/a.jpg", MediaUrlHelper.ToAbsolute("/images/a.jpg", null));

        // ── Discount calculation ──────────────────────────────────────────────

        [Theory]
        [InlineData(100, 200, 50)]
        [InlineData(8999, 10499, 14)]
        [InlineData(75, 100, 25)]
        public void CalculateDiscountPercent_ComputesCorrectly(
            decimal price, decimal oldPrice, int expected)
            => Assert.Equal(expected,
                MobileMappingExtensions.CalculateDiscountPercent(price, oldPrice));

        /// <summary>
        /// Cases where no discount should be reported.
        ///
        /// Supplied via MemberData rather than InlineData because C# attributes
        /// cannot carry decimal constants — xUnit would hand the method an int
        /// and fail to convert it to decimal?.
        /// </summary>
        public static TheoryData<decimal, decimal?> NonDiscountCases => new()
        {
            { 100m, null },   // no old price
            { 100m, 0m },     // zero old price -> would divide by zero
            { 100m, 80m },    // "old" price lower -> not a discount
            { 100m, 100m }    // equal -> not a discount
        };

        /// <summary>
        /// Guards against divide-by-zero and negative discounts from bad data.
        /// </summary>
        [Theory]
        [MemberData(nameof(NonDiscountCases))]
        public void CalculateDiscountPercent_ReturnsNullForInvalidInput(
            decimal price, decimal? oldPrice)
            => Assert.Null(MobileMappingExtensions.CalculateDiscountPercent(price, oldPrice));

        // ── FeaturesJson parsing ──────────────────────────────────────────────

        [Fact]
        public void ParseFeatures_ReadsStringArray()
        {
            var result = MobileMappingExtensions.ParseFeatures("[\"شاشة 12 بوصة\",\"12GB RAM\"]");

            Assert.Equal(2, result.Count);
            Assert.Equal("شاشة 12 بوصة", result[0]);
        }

        [Fact]
        public void ParseFeatures_ReadsObjectArray()
        {
            var result = MobileMappingExtensions.ParseFeatures(
                "[{\"text\":\"ميزة أولى\"},{\"title\":\"ميزة ثانية\"}]");

            Assert.Equal(2, result.Count);
            Assert.Equal("ميزة أولى", result[0]);
            Assert.Equal("ميزة ثانية", result[1]);
        }

        /// <summary>
        /// FeaturesJson is free-form text hand-edited in the admin panel, so
        /// malformed content is expected. It must degrade to an empty list —
        /// never turn a product page into a 500.
        /// </summary>
        [Theory]
        [InlineData("not json at all")]
        [InlineData("{\"notAnArray\":true}")]
        [InlineData("[")]
        [InlineData("")]
        [InlineData(null)]
        public void ParseFeatures_ReturnsEmptyForMalformedInput(string? json)
            => Assert.Empty(MobileMappingExtensions.ParseFeatures(json));

        // ── Paging normalization ──────────────────────────────────────────────

        [Theory]
        [InlineData(null, null, 1, 20)]     // defaults
        [InlineData(0, 0, 1, 20)]           // zero -> defaults
        [InlineData(-5, -5, 1, 20)]         // negative -> defaults
        [InlineData(3, 10, 3, 10)]          // honoured
        [InlineData(2, 1000, 2, 50)]        // clamped to MaxPageSize
        public void Normalize_ClampsPagingValues(
            int? page, int? pageSize, int expectedPage, int expectedSize)
        {
            var (p, s) = PagedResult<string>.Normalize(page, pageSize);
            Assert.Equal(expectedPage, p);
            Assert.Equal(expectedSize, s);
        }
    }

    /// <summary>
    /// Wishlist idempotency and per-user isolation.
    /// </summary>
    public class WishlistServiceTests
    {
        private const string UserA = "user-a";
        private const string UserB = "user-b";

        [Fact]
        public async Task AddAsync_IsIdempotent()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1);

            var service = new WishlistService(context);

            // A retried mobile request must not create a second row.
            Assert.True(await service.AddAsync(UserA, 1));
            Assert.True(await service.AddAsync(UserA, 1));
            Assert.True(await service.AddAsync(UserA, 1));

            Assert.Equal(1, await service.GetCountAsync(UserA));
        }

        [Fact]
        public async Task AddAsync_RejectsUnknownOrInactiveProduct()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, isActive: false);

            var service = new WishlistService(context);

            Assert.False(await service.AddAsync(UserA, 999)); // does not exist
            Assert.False(await service.AddAsync(UserA, 1));   // inactive
        }

        [Fact]
        public async Task Wishlists_AreIsolatedPerUser()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1);
            TestDbContextFactory.AddProduct(context, 2, 1);

            var service = new WishlistService(context);
            await service.AddAsync(UserA, 1);
            await service.AddAsync(UserB, 2);

            Assert.True(await service.ContainsAsync(UserA, 1));
            Assert.False(await service.ContainsAsync(UserA, 2));
            Assert.True(await service.ContainsAsync(UserB, 2));
            Assert.False(await service.ContainsAsync(UserB, 1));
        }

        /// <summary>
        /// A product the admin deactivates must disappear from the wishlist view
        /// rather than being presented as buyable.
        /// </summary>
        [Fact]
        public async Task GetAsync_HidesDeactivatedProducts()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            var product = TestDbContextFactory.AddProduct(context, 1, 1);

            var service = new WishlistService(context);
            await service.AddAsync(UserA, 1);

            product.IsActive = false;
            await context.SaveChangesAsync();

            var result = await service.GetAsync(UserA, 1, 20, "ar", null);
            Assert.Empty(result.Items);
        }

        [Fact]
        public async Task RemoveAsync_ReturnsFalseWhenNotSaved()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1);

            var service = new WishlistService(context);
            Assert.False(await service.RemoveAsync(UserA, 1));
        }
    }

    /// <summary>
    /// Idempotency replay protection — the guard that stops a retried checkout
    /// from creating a duplicate order.
    /// </summary>
    public class IdempotencyServiceTests
    {
        private const string Endpoint = "POST /api/v1/orders/checkout";

        [Fact]
        public async Task TryGetAsync_ReportsNotFoundForUnknownKey()
        {
            using var context = TestDbContextFactory.Create();
            var service = new IdempotencyService(context);

            var (found, conflict, _, _) = await service.TryGetAsync("key-1", Endpoint, "hash");

            Assert.False(found);
            Assert.False(conflict);
        }

        [Fact]
        public async Task SaveThenTryGet_ReplaysStoredResponse()
        {
            using var context = TestDbContextFactory.Create();
            var service = new IdempotencyService(context);

            var hash = service.ComputeRequestHash("{\"items\":[1]}");
            await service.SaveAsync("key-1", Endpoint, hash, "{\"orderNumber\":\"FTD123\"}", 201);

            var (found, conflict, json, status) = await service.TryGetAsync("key-1", Endpoint, hash);

            Assert.True(found);
            Assert.False(conflict);
            Assert.Equal("{\"orderNumber\":\"FTD123\"}", json);
            Assert.Equal(201, status);
        }

        /// <summary>
        /// Reusing a key with a different body is a client bug. Returning the
        /// earlier (unrelated) order would be wrong, so it must be reported as a
        /// conflict instead.
        /// </summary>
        [Fact]
        public async Task TryGetAsync_DetectsConflictOnDifferentPayload()
        {
            using var context = TestDbContextFactory.Create();
            var service = new IdempotencyService(context);

            var originalHash = service.ComputeRequestHash("{\"items\":[1]}");
            await service.SaveAsync("key-1", Endpoint, originalHash, "{\"ok\":true}", 201);

            var differentHash = service.ComputeRequestHash("{\"items\":[2]}");
            var (found, conflict, _, _) = await service.TryGetAsync("key-1", Endpoint, differentHash);

            Assert.False(found);
            Assert.True(conflict);
        }

        /// <summary>
        /// The same key on a different endpoint is a different operation and
        /// must not replay the first one's response.
        /// </summary>
        [Fact]
        public async Task TryGetAsync_ScopesKeysPerEndpoint()
        {
            using var context = TestDbContextFactory.Create();
            var service = new IdempotencyService(context);

            var hash = service.ComputeRequestHash("{}");
            await service.SaveAsync("key-1", Endpoint, hash, "{\"ok\":true}", 201);

            var (found, _, _, _) = await service.TryGetAsync("key-1", "POST /api/v1/other", hash);
            Assert.False(found);
        }

        [Fact]
        public void ComputeRequestHash_IsStableAndDistinguishing()
        {
            using var context = TestDbContextFactory.Create();
            var service = new IdempotencyService(context);

            Assert.Equal(service.ComputeRequestHash("{\"a\":1}"), service.ComputeRequestHash("{\"a\":1}"));
            Assert.NotEqual(service.ComputeRequestHash("{\"a\":1}"), service.ComputeRequestHash("{\"a\":2}"));
        }
    }

    /// <summary>
    /// The database-backed cart store. Its whole purpose is to satisfy the
    /// existing ICartStorage contract so CartService can be reused unchanged.
    /// </summary>
    public class DbCartStorageTests
    {
        private const string UserId = "cart-user-1";

        [Fact]
        public async Task SetRaw_ThenGetRaw_RoundTripsThroughDatabase()
        {
            using var context = TestDbContextFactory.Create();

            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();

            storage.SetRaw("[{\"ProductId\":1,\"Qty\":2}]");
            await storage.SaveAsync();

            // A fresh instance proves the value was persisted, not just cached.
            var reloaded = new DbCartStorage(context, UserId);
            await reloaded.LoadAsync();

            Assert.Equal("[{\"ProductId\":1,\"Qty\":2}]", reloaded.GetRaw());
        }

        [Fact]
        public async Task GetRaw_ReturnsNullForNewUser()
        {
            using var context = TestDbContextFactory.Create();

            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();

            // Null (not "[]") so CartService treats it as an absent cart.
            Assert.Null(storage.GetRaw());
        }

        /// <summary>
        /// Clearing resets the payload but keeps the row, preserving CreatedAt
        /// for abandoned-cart analytics.
        /// </summary>
        [Fact]
        public async Task Clear_EmptiesCartButKeepsRow()
        {
            using var context = TestDbContextFactory.Create();

            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();
            storage.SetRaw("[{\"ProductId\":1,\"Qty\":2}]");
            await storage.SaveAsync();

            storage.Clear();
            await storage.SaveAsync();

            Assert.Equal("[]", storage.GetRaw());
            Assert.Equal(1, context.UserCarts.Count(c => c.UserId == UserId));
        }

        [Fact]
        public async Task CartsAreIsolatedPerUser()
        {
            using var context = TestDbContextFactory.Create();

            var cartA = new DbCartStorage(context, "user-a");
            await cartA.LoadAsync();
            cartA.SetRaw("[{\"ProductId\":1,\"Qty\":1}]");
            await cartA.SaveAsync();

            var cartB = new DbCartStorage(context, "user-b");
            await cartB.LoadAsync();

            Assert.Null(cartB.GetRaw());
        }

        /// <summary>
        /// ICartStorage is synchronous (it was designed around ISession), so the
        /// row must be loaded before use. Failing loudly beats silently
        /// returning an empty cart and losing the customer's items.
        /// </summary>
        [Fact]
        public void GetRaw_ThrowsIfUsedBeforeLoadAsync()
        {
            using var context = TestDbContextFactory.Create();

            var storage = new DbCartStorage(context, UserId);
            Assert.Throws<InvalidOperationException>(() => storage.GetRaw());
        }

        [Fact]
        public void Constructor_RejectsMissingUserId()
        {
            using var context = TestDbContextFactory.Create();
            Assert.Throws<ArgumentException>(() => new DbCartStorage(context, ""));
        }
    }

    /// <summary>
    /// CartService driven through the DATABASE-backed storage, proving the
    /// existing cart business logic works unchanged against the new store.
    /// </summary>
    public class CartServiceWithDbStorageTests
    {
        private const string UserId = "cart-logic-user";

        [Fact]
        public async Task AddItem_ThenGetCart_ReturnsPricedLineFromCatalog()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 250m, stock: 10);

            var cartService = new CartService(new ContentService(context), context);
            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();

            cartService.AddItem(storage, productId: 1, qty: 2);
            await storage.SaveAsync();

            var cart = await cartService.GetCartAsync(storage);

            var line = Assert.Single(cart.Items);
            Assert.Equal(1, line.ProductId);
            Assert.Equal(2, line.Quantity);
            // Price comes from the catalog, never from the stored cart.
            Assert.Equal(250m, line.UnitPrice);
            Assert.Equal(500m, cart.SubTotal);
        }

        /// <summary>
        /// Free shipping is applied above the configured threshold (seeded at
        /// 5000), so a large order must not be charged the flat fee.
        /// </summary>
        [Fact]
        public async Task GetCart_AppliesFreeShippingAboveThreshold()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 6000m, stock: 10);

            var cartService = new CartService(new ContentService(context), context);
            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();

            cartService.AddItem(storage, 1, 1);
            await storage.SaveAsync();

            var cart = await cartService.GetCartAsync(storage);

            Assert.True(cart.FreeShipping);
            Assert.Equal(6000m, cart.Total);
        }

        /// <summary>
        /// A crafted request with a non-positive quantity must never subtract
        /// lines or produce a negative total (AUDIT_REPORT I-14).
        /// </summary>
        [Fact]
        public async Task AddItem_ClampsNonPositiveQuantity()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var cartService = new CartService(new ContentService(context), context);
            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();

            cartService.AddItem(storage, 1, qty: -5);
            await storage.SaveAsync();

            var cart = await cartService.GetCartAsync(storage);

            Assert.Equal(1, cart.Items.Single().Quantity);
            Assert.Equal(100m, cart.SubTotal);
        }

        [Fact]
        public async Task UpdateQty_ToZero_RemovesLine()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var cartService = new CartService(new ContentService(context), context);
            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();

            cartService.AddItem(storage, 1, 3);
            cartService.UpdateQty(storage, 1, 0);
            await storage.SaveAsync();

            var cart = await cartService.GetCartAsync(storage);
            Assert.Empty(cart.Items);
        }

        /// <summary>
        /// A cart can outlive the products in it. Lines whose product was
        /// deactivated must be dropped rather than priced.
        /// </summary>
        [Fact]
        public async Task GetCart_DropsLinesForDeactivatedProducts()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            var product = TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var cartService = new CartService(new ContentService(context), context);
            var storage = new DbCartStorage(context, UserId);
            await storage.LoadAsync();

            cartService.AddItem(storage, 1, 2);
            await storage.SaveAsync();

            product.IsActive = false;
            await context.SaveChangesAsync();

            var cart = await cartService.GetCartAsync(storage);

            Assert.Empty(cart.Items);
            Assert.Equal(0m, cart.SubTotal);
        }
    }
}
