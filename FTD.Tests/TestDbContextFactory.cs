using System;
using FTD.Domain.Entities;
using FTD.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FTD.Tests
{
    /// <summary>
    /// Builds an isolated in-memory <see cref="AppDbContext"/> per test.
    ///
    /// Each context gets a unique database name so tests never share state and
    /// can run in parallel. Seed data mirrors the real seeded order statuses,
    /// because several rules under test (cancellable statuses, timeline
    /// construction) depend on those exact ids and sort orders.
    /// </summary>
    internal static class TestDbContextFactory
    {
        public static AppDbContext Create()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"ftd-tests-{Guid.NewGuid():N}")
                // The InMemory provider warns that it cannot honour transactions.
                // These tests assert business rules, not transactional
                // guarantees, so the warning is suppressed rather than ignored
                // silently.
                .ConfigureWarnings(w => w.Ignore(
                    Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new AppDbContext(options);
            SeedOrderStatuses(context);
            return context;
        }

        /// <summary>
        /// Inserts the seven production order statuses.
        /// HasData seeding does not apply automatically to the InMemory provider
        /// without EnsureCreated, so they are added explicitly here.
        /// </summary>
        private static void SeedOrderStatuses(AppDbContext context)
        {
            context.OrderStatuses.AddRange(
                new OrderStatus { Id = 1, NameAr = "جديد", NameEn = "New", ColorHex = "#1A6BFF", Icon = "🆕", SortOrder = 1 },
                new OrderStatus { Id = 2, NameAr = "مؤكد", NameEn = "Confirmed", ColorHex = "#0E4FCC", Icon = "✅", SortOrder = 2 },
                new OrderStatus { Id = 3, NameAr = "في انتظار الشحن", NameEn = "Pending Shipment", ColorHex = "#FF9500", Icon = "📦", SortOrder = 3 },
                new OrderStatus { Id = 4, NameAr = "مع شركة الشحن", NameEn = "With Courier", ColorHex = "#FF6B35", Icon = "🚚", SortOrder = 4 },
                new OrderStatus { Id = 5, NameAr = "تم التسليم", NameEn = "Delivered", ColorHex = "#00C48C", Icon = "🎉", SortOrder = 5 },
                new OrderStatus { Id = 6, NameAr = "مرتجع", NameEn = "Returned", ColorHex = "#FF3B30", Icon = "↩️", SortOrder = 6 },
                new OrderStatus { Id = 7, NameAr = "ملغي", NameEn = "Cancelled", ColorHex = "#6c757d", Icon = "❌", SortOrder = 7 });

            context.SaveChanges();
        }

        /// <summary>Adds a category and returns it.</summary>
        public static Category AddCategory(
            AppDbContext context, int id, string nameAr = "تابلت", string nameEn = "Tablets", string slug = "tablets")
        {
            var category = new Category
            {
                Id = id,
                NameAr = nameAr,
                NameEn = nameEn,
                Slug = slug,
                IsActive = true,
                SortOrder = id
            };
            context.Categories.Add(category);
            context.SaveChanges();
            return category;
        }

        /// <summary>
        /// Adds a product. SortOrder defaults to 0 for every product on purpose:
        /// that is precisely the condition under which a paging query without a
        /// deterministic tie-break returns unstable results.
        /// </summary>
        public static Product AddProduct(
            AppDbContext context,
            int id,
            int categoryId,
            string nameAr = "منتج",
            string nameEn = "Product",
            decimal price = 100m,
            decimal? oldPrice = null,
            int stock = 10,
            bool isActive = true,
            bool isFeatured = false,
            int sortOrder = 0)
        {
            var product = new Product
            {
                Id = id,
                CategoryId = categoryId,
                NameAr = nameAr,
                NameEn = nameEn,
                Slug = $"product-{id}",
                Price = price,
                OldPrice = oldPrice,
                Stock = stock,
                IsActive = isActive,
                IsFeatured = isFeatured,
                SortOrder = sortOrder,
                CreatedAt = DateTime.UtcNow.AddDays(-id)
            };
            context.Products.Add(product);
            context.SaveChanges();
            return product;
        }
    }
}
