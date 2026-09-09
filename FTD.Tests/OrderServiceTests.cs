using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Services;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FTD.Tests
{
    /// <summary>
    /// Covers the order rules that protect money and inventory: guest vs
    /// account linkage, stock movement, ownership isolation and the tracking
    /// second factor.
    /// </summary>
    public class OrderServiceTests
    {
        private const string OwnerId = "user-owner-1";
        private const string OtherUserId = "user-other-2";

        private static CartDto BuildCart(int productId, int quantity, decimal unitPrice)
            => new()
            {
                Items = new List<CartItemDto>
                {
                    new()
                    {
                        ProductId = productId,
                        ProductName = "منتج",
                        UnitPrice = unitPrice,
                        Quantity = quantity
                    }
                },
                ShippingFee = 150m,
                FreeShippingAbove = 5000m
            };

        private static CheckoutDto BuildCheckout(string phone = "01012345678")
            => new()
            {
                CustomerName = "أحمد محمد",
                CustomerPhone = phone,
                Address = "12 شارع النيل",
                City = "القاهرة",
                Governorate = "القاهرة"
            };

        // ══════════════════════════════════════════════════════════════════════
        //  CHECKOUT
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// The storefront has always allowed guest checkout, and every
        /// historical order predates accounts. UserId must therefore stay
        /// nullable and default to null when no user is supplied.
        /// </summary>
        [Fact]
        public async Task CreateOrderAsync_LeavesUserIdNull_ForGuestCheckout()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 2, 100m));

            var saved = await context.SalesOrders.FirstAsync(o => o.Id == order.Id);
            Assert.Null(saved.UserId);
        }

        [Fact]
        public async Task CreateOrderAsync_LinksOrderToUser_WhenSignedIn()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);

            var saved = await context.SalesOrders.FirstAsync(o => o.Id == order.Id);
            Assert.Equal(OwnerId, saved.UserId);
        }

        [Fact]
        public async Task CreateOrderAsync_DeductsStock()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 3, 100m), OwnerId);

            var product = await context.Products.FirstAsync(p => p.Id == 1);
            Assert.Equal(7, product.Stock);
        }

        [Fact]
        public async Task CreateOrderAsync_Throws_WhenStockInsufficient()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 2);

            var service = new OrderService(context);

            // Surfaces as a localized 400 through GlobalExceptionHandler.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 5, 100m), OwnerId));
        }

        /// <summary>
        /// Order numbers were previously timestamp + 3 random digits inserted
        /// directly, so two orders in the same second collided about once in
        /// 900. Generation must now guarantee uniqueness.
        /// </summary>
        [Fact]
        public async Task CreateOrderAsync_GeneratesUniqueOrderNumbers()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 10m, stock: 1000);

            var service = new OrderService(context);
            var numbers = new List<string>();

            for (var i = 0; i < 15; i++)
            {
                var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 10m), OwnerId);
                numbers.Add(order.OrderNumber);
            }

            Assert.Equal(numbers.Count, numbers.Distinct().Count());
            Assert.All(numbers, n => Assert.StartsWith("FTD", n));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  OWNERSHIP ISOLATION (IDOR protection)
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task GetMyOrdersAsync_ReturnsOnlyTheCallersOrders()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 100);

            var service = new OrderService(context);
            await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);
            await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);
            await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OtherUserId);
            await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m)); // guest

            var result = await service.GetMyOrdersAsync(OwnerId, 1, 20, "ar", null);

            Assert.Equal(2, result.TotalCount);
        }

        /// <summary>
        /// Requesting another customer's order must behave exactly like
        /// requesting a nonexistent one — returning 403 would confirm the order
        /// exists, which is itself an information leak.
        /// </summary>
        [Fact]
        public async Task GetMyOrderDetailAsync_ReturnsNull_ForAnotherUsersOrder()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 100);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);

            var asOwner = await service.GetMyOrderDetailAsync(order.Id, OwnerId, "ar", null);
            var asOther = await service.GetMyOrderDetailAsync(order.Id, OtherUserId, "ar", null);

            Assert.NotNull(asOwner);
            Assert.Null(asOther);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GUEST TRACKING — phone second factor
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task TrackOrderAsync_Succeeds_WithCorrectPhoneLast4()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 100);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(
                BuildCheckout("01012345678"), BuildCart(1, 1, 100m));

            var tracked = await service.TrackOrderAsync(order.OrderNumber, "5678", "ar", null);

            Assert.NotNull(tracked);
            Assert.Equal(order.OrderNumber, tracked!.OrderNumber);
        }

        /// <summary>
        /// Order numbers follow a guessable pattern (FTD + timestamp + digits),
        /// so the number alone must never be sufficient to read customer PII.
        /// </summary>
        [Fact]
        public async Task TrackOrderAsync_ReturnsNull_WithWrongPhoneLast4()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 100);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(
                BuildCheckout("01012345678"), BuildCart(1, 1, 100m));

            Assert.Null(await service.TrackOrderAsync(order.OrderNumber, "0000", "ar", null));
            Assert.Null(await service.TrackOrderAsync(order.OrderNumber, "", "ar", null));
            Assert.Null(await service.TrackOrderAsync(order.OrderNumber, null!, "ar", null));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  CANCELLATION
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Cancelling MUST return the reserved stock. Checkout deducted it, so
        /// skipping the restore would make inventory drift down permanently on
        /// every cancellation.
        /// </summary>
        [Fact]
        public async Task CancelMyOrderAsync_RestoresStockAndSetsCancelledStatus()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 4, 100m), OwnerId);

            Assert.Equal(6, (await context.Products.FirstAsync(p => p.Id == 1)).Stock);

            var (success, errorCode, _) = await service.CancelMyOrderAsync(order.Id, OwnerId);

            Assert.True(success);
            Assert.Null(errorCode);

            // Stock back to its original level.
            var product = await context.Products.AsNoTracking().FirstAsync(p => p.Id == 1);
            Assert.Equal(10, product.Stock);

            var saved = await context.SalesOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
            Assert.Equal(7, saved.StatusId); // Cancelled
        }

        [Fact]
        public async Task CancelMyOrderAsync_Fails_ForAnotherUsersOrder()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);

            var (success, errorCode, _) = await service.CancelMyOrderAsync(order.Id, OtherUserId);

            Assert.False(success);
            Assert.Equal(ApiErrorCodes.OrderNotFound, errorCode);

            // The order must be untouched.
            var saved = await context.SalesOrders.AsNoTracking().FirstAsync(o => o.Id == order.Id);
            Assert.Equal(1, saved.StatusId);
        }

        /// <summary>
        /// Once an order has shipped, self-service cancellation must be refused —
        /// otherwise stock would be credited back for goods already in transit.
        /// </summary>
        [Fact]
        public async Task CancelMyOrderAsync_Fails_WhenOrderAlreadyShipped()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 2, 100m), OwnerId);

            await service.UpdateStatusAsync(order.Id, 4); // With Courier

            var (success, errorCode, _) = await service.CancelMyOrderAsync(order.Id, OwnerId);

            Assert.False(success);
            Assert.Equal(ApiErrorCodes.OrderNotCancellable, errorCode);

            // Stock must NOT have been credited back.
            var product = await context.Products.AsNoTracking().FirstAsync(p => p.Id == 1);
            Assert.Equal(8, product.Stock);
        }

        [Fact]
        public async Task UpdateStatusAsync_Throws_ForUnknownStatusId()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);

            // Validated up front so a bad id becomes a 400, not an opaque FK 500.
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateStatusAsync(order.Id, 999));
        }

        // ══════════════════════════════════════════════════════════════════════
        //  TIMELINE
        // ══════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task OrderDetail_Timeline_MarksReachedAndCurrentSteps()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);
            await service.UpdateStatusAsync(order.Id, 3); // Pending Shipment

            var detail = await service.GetMyOrderDetailAsync(order.Id, OwnerId, "ar", null);

            Assert.NotNull(detail);
            // Returned (6) and Cancelled (7) are exceptions, not steps.
            Assert.Equal(5, detail!.Timeline.Count);

            var current = detail.Timeline.Single(s => s.IsCurrent);
            Assert.Equal(3, current.StatusId);

            Assert.True(detail.Timeline.Where(s => s.StatusId <= 3).All(s => s.IsReached));
            Assert.True(detail.Timeline.Where(s => s.StatusId > 3).All(s => !s.IsReached));
        }

        /// <summary>
        /// A cancelled order never progressed through delivery, so the ladder
        /// must collapse to the terminal state instead of implying it did.
        /// </summary>
        [Fact]
        public async Task OrderDetail_Timeline_CollapsesForCancelledOrder()
        {
            using var context = TestDbContextFactory.Create();
            TestDbContextFactory.AddCategory(context, 1);
            TestDbContextFactory.AddProduct(context, 1, 1, price: 100m, stock: 10);

            var service = new OrderService(context);
            var order = await service.CreateOrderAsync(BuildCheckout(), BuildCart(1, 1, 100m), OwnerId);
            await service.CancelMyOrderAsync(order.Id, OwnerId);

            var detail = await service.GetMyOrderDetailAsync(order.Id, OwnerId, "ar", null);

            Assert.NotNull(detail);
            var step = Assert.Single(detail!.Timeline);
            Assert.Equal(7, step.StatusId);
            Assert.True(step.IsCurrent);
        }
    }
}
