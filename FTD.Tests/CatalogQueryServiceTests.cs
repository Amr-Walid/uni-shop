using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Services;
using Xunit;

namespace FTD.Tests
{
    /// <summary>
    /// Covers the paging contract, the deterministic-ordering guarantee and the
    /// rule that raw stock is never exposed to storefront clients.
    /// </summary>
    public class CatalogQueryServiceTests
    {
        private static CatalogQueryService CreateService(Infrastructure.Data.AppDbContext context)
            => new(context, new ContentService(context));

        // ══════════════════════════════════════════════════════════════════════
        //  PAGING
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetProductsAsync_RespectsPageSize()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            for (var i = 1; i <= 25; i++)
                TestDbContextFactory.AddProduct(context, i, 1);

            var service = CreateService(context);
            var result = await service.GetProductsAsync(new CatalogFilter(), page: 1, pageSize: 10, "ar", null);

            Assert.Equal(10, result.Items.Count);
            Assert.Equal(25, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.True(result.HasNext);
            Assert.False(result.HasPrevious);
        }

        [Fact]
        public async Task GetProductsAsync_ClampsPageSizeToMaximum()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            for (var i = 1; i <= 80; i++)
                TestDbContextFactory.AddProduct(context, i, 1);

            var service = CreateService(context);

            // A caller asking for 1000 must not be able to bypass the cap and
            // turn pagination back into a full-table read.
            var result = await service.GetProductsAsync(new CatalogFilter(), page: 1, pageSize: 1000, "ar", null);

            Assert.Equal(PagedResult<ProductListItemDto>.MaxPageSize, result.Items.Count);
            Assert.Equal(PagedResult<ProductListItemDto>.MaxPageSize, result.PageSize);
        }

        /// <summary>
        /// THE critical paging regression test.
        ///
        /// Every product here shares SortOrder 0 and IsFeatured false, so the
        /// primary sort keys are entirely tied. Without the ThenBy(Id)
        /// tie-break, the provider may return rows in any order per query and
        /// the two pages can overlap — a product appears twice while another is
        /// never shown at all. Asserting that the union of both pages equals the
        /// full distinct set is what proves the ordering is stable.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_PagesDoNotOverlapOrSkipItems_WhenSortKeysAreTied()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            for (var i = 1; i <= 10; i++)
                TestDbContextFactory.AddProduct(context, i, 1, sortOrder: 0);

            var service = CreateService(context);

            var page1 = await service.GetProductsAsync(new CatalogFilter(), 1, 5, "ar", null);
            var page2 = await service.GetProductsAsync(new CatalogFilter(), 2, 5, "ar", null);

            var ids = page1.Items.Select(p => p.Id).Concat(page2.Items.Select(p => p.Id)).ToList();

            Assert.Equal(10, ids.Count);
            Assert.Equal(10, ids.Distinct().Count());          // no duplicates
            Assert.Equal(Enumerable.Range(1, 10), ids.OrderBy(x => x)); // nothing skipped
        }

        [Fact]
        public async Task GetProductsAsync_ExcludesInactiveProducts()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, isActive: true);
            TestDbContextFactory.AddProduct(context, 2, 1, isActive: false);

            var service = CreateService(context);
            var result = await service.GetProductsAsync(new CatalogFilter(), 1, 20, "ar", null);

            Assert.Single(result.Items);
            Assert.Equal(1, result.Items[0].Id);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  FILTERING & SORTING
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetProductsAsync_SortsByPriceAscending()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 300m);
            TestDbContextFactory.AddProduct(context, 2, 1, price: 100m);
            TestDbContextFactory.AddProduct(context, 3, 1, price: 200m);

            var service = CreateService(context);
            var result = await service.GetProductsAsync(
                new CatalogFilter { Sort = CatalogSort.PriceAscending }, 1, 20, "ar", null);

            Assert.Equal(new[] { 100m, 200m, 300m }, result.Items.Select(p => p.Price));
        }

        [Fact]
        public async Task GetProductsAsync_OnSaleOnly_ReturnsOnlyDiscountedProducts()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, oldPrice: 150m); // discounted
            TestDbContextFactory.AddProduct(context, 2, 1, price: 100m);                 // no old price
            // Old price below current price is not a discount and must be excluded.
            TestDbContextFactory.AddProduct(context, 3, 1, price: 100m, oldPrice: 80m);

            var service = CreateService(context);
            var result = await service.GetProductsAsync(
                new CatalogFilter { OnSaleOnly = true }, 1, 20, "ar", null);

            Assert.Single(result.Items);
            Assert.Equal(1, result.Items[0].Id);
        }

        [Fact]
        public async Task GetProductsAsync_FiltersByPriceRange()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 50m);
            TestDbContextFactory.AddProduct(context, 2, 1, price: 150m);
            TestDbContextFactory.AddProduct(context, 3, 1, price: 250m);

            var service = CreateService(context);
            var result = await service.GetProductsAsync(
                new CatalogFilter { MinPrice = 100m, MaxPrice = 200m }, 1, 20, "ar", null);

            Assert.Single(result.Items);
            Assert.Equal(2, result.Items[0].Id);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  SECURITY / DATA EXPOSURE
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// The raw Stock figure is commercially sensitive. The list DTO must
        /// expose availability only — this test fails if a future refactor
        /// reintroduces a numeric stock field on the storefront contract.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_NeverExposesRawStockCount()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, stock: 137);

            var service = CreateService(context);
            var result = await service.GetProductsAsync(new CatalogFilter(), 1, 20, "ar", null);

            var item = result.Items.Single();
            Assert.True(item.InStock);
            Assert.False(item.LowStock);

            var propertyNames = typeof(ProductListItemDto).GetProperties().Select(p => p.Name).ToList();
            Assert.DoesNotContain("Stock", propertyNames);
        }

        [Theory]
        [InlineData(0, false, false)]  // out of stock
        [InlineData(3, true, true)]    // low stock (<= 5)
        [InlineData(50, true, false)]  // plenty
        public async Task GetProductsAsync_MapsStockFlagsCorrectly(
            int stock, bool expectedInStock, bool expectedLowStock)
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, stock: stock);

            var service = CreateService(context);
            var result = await service.GetProductsAsync(new CatalogFilter(), 1, 20, "ar", null);

            var item = result.Items.Single();
            Assert.Equal(expectedInStock, item.InStock);
            Assert.Equal(expectedLowStock, item.LowStock);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOCALIZATION
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetProductsAsync_ReturnsRequestedLanguage()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, nameAr: "تابلت", nameEn: "Tablet");

            var service = CreateService(context);

            var arabic = await service.GetProductsAsync(new CatalogFilter(), 1, 20, "ar", null);
            var english = await service.GetProductsAsync(new CatalogFilter(), 1, 20, "en", null);

            Assert.Equal("تابلت", arabic.Items.Single().Name);
            Assert.Equal("Tablet", english.Items.Single().Name);
        }

        /// <summary>
        /// Admins routinely fill Arabic and leave English blank. An English
        /// client must then see the Arabic text rather than an empty product
        /// name, which would look like a broken catalog.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_FallsBackToOtherLanguage_WhenTranslationMissing()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, nameAr: "تابلت", nameEn: "");

            var service = CreateService(context);
            var english = await service.GetProductsAsync(new CatalogFilter(), 1, 20, "en", null);

            Assert.Equal("تابلت", english.Items.Single().Name);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  DETAIL & SEARCH
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetProductBySlugAsync_ReturnsNull_ForInactiveProduct()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, isActive: false);

            var service = CreateService(context);
            var result = await service.GetProductBySlugAsync("product-1", "ar", null);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetSearchSuggestionsAsync_ReturnsEmpty_ForTooShortQuery()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, nameAr: "تابلت", nameEn: "Tablet");

            var service = CreateService(context);

            // A single character matches nearly everything and is expensive.
            Assert.Empty(await service.GetSearchSuggestionsAsync("T", 8, "en"));
            Assert.Empty(await service.GetSearchSuggestionsAsync("", 8, "en"));
        }

        [Fact]
        public async Task GetRelatedProductsAsync_ExcludesTheProductItself()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1);
            TestDbContextFactory.AddProduct(context, 2, 1);
            TestDbContextFactory.AddProduct(context, 3, 1);

            var service = CreateService(context);
            var related = await service.GetRelatedProductsAsync(1, 10, "ar", null);

            Assert.Equal(2, related.Count);
            Assert.DoesNotContain(related, p => p.Id == 1);
        }
    }
}
