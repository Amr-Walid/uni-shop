using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Domain.Entities;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IAppDbContext _db;
        public OrderService(IAppDbContext db) => _db = db;

        public async Task<SalesOrderDto> CreateOrderAsync(CheckoutDto checkout, CartDto cart, string? userId = null)
        {
            var orderNumber = await GenerateUniqueOrderNumberAsync();

            var order = new SalesOrder
            {
                OrderNumber = orderNumber,
                StatusId = 1, // New
                // Null for guest checkout — the storefront path passes no user.
                UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                CustomerName = checkout.CustomerName,
                CustomerPhone = checkout.CustomerPhone,
                CustomerEmail = checkout.CustomerEmail,
                Address = checkout.Address,
                City = checkout.City,
                Governorate = checkout.Governorate,
                Notes = checkout.Notes,
                SubTotal = cart.SubTotal,
                ShippingFee = cart.ShippingFee,
                TotalAmount = cart.Total,
                CreatedAt = DateTime.UtcNow,
                Details = cart.Items.Select(item => new SalesOrderDetail
                {
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SubTotal = item.SubTotal
                }).ToList()
            };

            // Verify stock and deduct.
            // Products are fetched in a single batched query (WHERE Id IN ...) instead of
            // one FindAsync round-trip per cart line (former N+1). They remain tracked so
            // the stock deduction below is persisted by the same SaveChangesAsync that
            // inserts the order — one implicit transaction keeps stock + order atomic.
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var item in cart.Items)
            {
                if (item.Quantity <= 0)
                    throw new InvalidOperationException($"كمية غير صالحة للمنتج رقم {item.ProductId}.");

                if (!products.TryGetValue(item.ProductId, out var product))
                    throw new InvalidOperationException($"المنتج رقم {item.ProductId} غير موجود في النظام.");

                if (product.Stock < item.Quantity)
                    throw new InvalidOperationException($"عذراً، الكمية المطلوبة للمنتج ({product.NameAr}) غير متوفرة حالياً في المخزن. المتاح: {product.Stock}");

                product.Stock -= item.Quantity;
            }

            _db.SalesOrders.Add(order);
            await _db.SaveChangesAsync();

            // Fetch the saved order with navigation properties to return a fully populated DTO
            var savedOrder = await _db.SalesOrders
                .AsNoTracking()
                .Include(o => o.Status)
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return savedOrder.ToDto()!;
        }

        public async Task UpdateStatusAsync(int orderId, int statusId)
        {
            // Guard against a forged/stale statusId that would violate the FK
            // and surface as an opaque SqlException to the admin.
            var statusExists = await _db.OrderStatuses.AnyAsync(s => s.Id == statusId);
            if (!statusExists)
                throw new InvalidOperationException($"حالة الطلب رقم {statusId} غير موجودة.");

            var order = await _db.SalesOrders.FindAsync(orderId);
            if (order != null)
            {
                order.StatusId = statusId;
                order.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<SalesOrderDto>> GetOrdersAsync(int? statusId)
        {
            var query = _db.SalesOrders.AsNoTracking().Include(o => o.Status).AsQueryable();

            if (statusId.HasValue)
            {
                query = query.Where(o => o.StatusId == statusId.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders
                .Select(o => o.ToDto())
                .OfType<SalesOrderDto>()
                .ToList();
        }

        public async Task<SalesOrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _db.SalesOrders
                .AsNoTracking()
                .Include(o => o.Status)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order.ToDto();
        }

        public async Task<List<OrderStatusDto>> GetAllStatusesAsync()
        {
            var statuses = await _db.OrderStatuses
                .AsNoTracking()
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return statuses
                .Select(s => s.ToDto())
                .OfType<OrderStatusDto>()
                .ToList();
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MOBILE / CUSTOMER-FACING ORDER ACCESS
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Status ids that still allow customer-initiated cancellation.</summary>
        private static readonly int[] CancellableStatusIds = { 1 /* New */, 2 /* Confirmed */ };

        private const int CancelledStatusId = 7;

        public async Task<PagedResult<OrderListItemDto>> GetMyOrdersAsync(
            string userId, int page, int pageSize, string lang, string? mediaBaseUrl)
        {
            var (normalizedPage, normalizedSize) = PagedResult<OrderListItemDto>.Normalize(page, pageSize);

            if (string.IsNullOrWhiteSpace(userId))
                return PagedResult<OrderListItemDto>.Empty(normalizedPage, normalizedSize);

            // Authorization is part of the query, not a separate check.
            var query = _db.SalesOrders
                .AsNoTracking()
                .Where(o => o.UserId == userId);

            var totalCount = await query.CountAsync();
            if (totalCount == 0)
                return PagedResult<OrderListItemDto>.Empty(normalizedPage, normalizedSize);

            var orders = await query
                .Include(o => o.Status)
                .Include(o => o.Details).ThenInclude(d => d.Product)
                // Id tie-break keeps paging stable for orders created in the
                // same instant (bulk imports, load tests).
                .OrderByDescending(o => o.CreatedAt)
                .ThenByDescending(o => o.Id)
                .Skip((normalizedPage - 1) * normalizedSize)
                .Take(normalizedSize)
                .ToListAsync();

            var items = orders.Select(o => new OrderListItemDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                Total = o.TotalAmount,
                ItemsCount = o.Details?.Sum(d => d.Quantity) ?? 0,
                CreatedAt = o.CreatedAt,
                Status = o.Status?.ToSummary(lang),
                FirstItemImageUrl = MediaUrlHelper.ToAbsolute(
                    o.Details?.OrderBy(d => d.Id).FirstOrDefault()?.Product?.ImagePath, mediaBaseUrl),
                // Computed server-side so the app never re-implements the rules.
                CanCancel = CancellableStatusIds.Contains(o.StatusId)
            }).ToList();

            return new PagedResult<OrderListItemDto>(items, normalizedPage, normalizedSize, totalCount);
        }

        public async Task<OrderDetailDto?> GetMyOrderDetailAsync(
            int orderId, string userId, string lang, string? mediaBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var order = await BuildDetailQuery()
                // Ownership is enforced in the predicate: a mismatched user gets
                // the same "not found" as a nonexistent id, which also avoids
                // confirming that the order exists at all.
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null) return null;

            return await MapToDetailAsync(order, lang, mediaBaseUrl);
        }

        public async Task<OrderDetailDto?> TrackOrderAsync(
            string orderNumber, string phoneLast4, string lang, string? mediaBaseUrl)
        {
            if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(phoneLast4))
                return null;

            var trimmedNumber = orderNumber.Trim();
            var order = await BuildDetailQuery()
                .FirstOrDefaultAsync(o => o.OrderNumber == trimmedNumber);

            if (order == null) return null;

            // SECOND FACTOR. Order numbers are predictable, so the number alone
            // must never be sufficient to read personal data. Comparing the last
            // 4 phone digits proves the caller actually placed this order.
            var digitsOnly = new string(order.CustomerPhone.Where(char.IsDigit).ToArray());
            var expected = digitsOnly.Length >= 4
                ? digitsOnly.Substring(digitsOnly.Length - 4)
                : digitsOnly;

            var supplied = new string(phoneLast4.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(expected) || !string.Equals(expected, supplied, StringComparison.Ordinal))
                return null;

            return await MapToDetailAsync(order, lang, mediaBaseUrl);
        }

        public async Task<(bool Success, string? ErrorCode, string? Message)> CancelMyOrderAsync(
            int orderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, ApiErrorCodes.Unauthorized, "غير مصرح");

            // Tracked (no AsNoTracking) because this path mutates and saves.
            var order = await _db.SalesOrders
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return (false, ApiErrorCodes.OrderNotFound, "الطلب غير موجود");

            if (!CancellableStatusIds.Contains(order.StatusId))
                return (false, ApiErrorCodes.OrderNotCancellable,
                    "لا يمكن إلغاء الطلب في حالته الحالية. تواصل مع خدمة العملاء للمساعدة.");

            // Return the reserved stock. CreateOrderAsync deducted it, so
            // cancelling MUST put it back or inventory drifts permanently.
            var productIds = order.Details.Select(d => d.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            foreach (var detail in order.Details)
            {
                if (products.TryGetValue(detail.ProductId, out var product))
                    product.Stock += detail.Quantity;
            }

            order.StatusId = CancelledStatusId;
            order.UpdatedAt = DateTime.UtcNow;

            // One SaveChanges → one implicit transaction, so the status change
            // and the stock restoration can never be applied partially.
            await _db.SaveChangesAsync();

            return (true, null, "تم إلغاء الطلب بنجاح وسيتم إعادة الكمية للمخزن");
        }

        public Task<bool> OrderNumberExistsAsync(string orderNumber)
        {
            if (string.IsNullOrWhiteSpace(orderNumber)) return Task.FromResult(false);
            var trimmed = orderNumber.Trim();
            return _db.SalesOrders.AsNoTracking().AnyAsync(o => o.OrderNumber == trimmed);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private IQueryable<SalesOrder> BuildDetailQuery() => _db.SalesOrders
            .AsNoTracking()
            .Include(o => o.Status)
            .Include(o => o.Details).ThenInclude(d => d.Product);

        /// <summary>
        /// Builds the detail DTO, including the full status ladder so the app can
        /// render a progress tracker without hardcoding the workflow (statuses
        /// are admin-editable data, not constants).
        /// </summary>
        private async Task<OrderDetailDto> MapToDetailAsync(
            SalesOrder order, string lang, string? mediaBaseUrl)
        {
            var dto = new OrderDetailDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.CustomerEmail,
                Address = order.Address,
                City = order.City,
                Governorate = order.Governorate,
                Notes = order.Notes,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                Total = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Status = order.Status?.ToSummary(lang),
                CanCancel = CancellableStatusIds.Contains(order.StatusId),
                Items = (order.Details ?? new List<SalesOrderDetail>())
                    .OrderBy(d => d.Id)
                    .Select(d => new OrderLineDto
                    {
                        ProductId = d.ProductId,
                        // Historical snapshot — intentionally the name captured
                        // at purchase time, not the product's current name.
                        ProductName = d.ProductName,
                        Quantity = d.Quantity,
                        UnitPrice = d.UnitPrice,
                        SubTotal = d.SubTotal,
                        ImageUrl = MediaUrlHelper.ToAbsolute(d.Product?.ImagePath, mediaBaseUrl),
                        ProductSlug = d.Product?.Slug
                    })
                    .ToList()
            };

            dto.Timeline = await BuildTimelineAsync(order.StatusId, lang);
            return dto;
        }

        private async Task<List<OrderTimelineStepDto>> BuildTimelineAsync(int currentStatusId, string lang)
        {
            var statuses = await _db.OrderStatuses
                .AsNoTracking()
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            var current = statuses.FirstOrDefault(s => s.Id == currentStatusId);
            var currentSort = current?.SortOrder ?? 0;

            // Terminal states (Returned / Cancelled) are exceptions rather than
            // steps, so the ladder collapses to just the current state instead
            // of implying the order progressed through delivery.
            var isTerminalException = currentStatusId is 6 or CancelledStatusId;

            return statuses
                .Where(s => !isTerminalException
                    ? s.Id is not (6 or CancelledStatusId)
                    : s.Id == currentStatusId)
                .Select(s => new OrderTimelineStepDto
                {
                    StatusId = s.Id,
                    Name = LocalizationHelper.Pick(s.NameAr, s.NameEn, lang),
                    ColorHex = s.ColorHex,
                    Icon = s.Icon,
                    IsReached = s.SortOrder <= currentSort,
                    IsCurrent = s.Id == currentStatusId
                })
                .ToList();
        }

        /// <summary>
        /// Produces an order number that is not already taken.
        ///
        /// The original code built the number from a timestamp plus 3 random
        /// digits and inserted it directly, so two orders placed in the same
        /// second had a ~1-in-900 chance of colliding. Retrying against the
        /// database removes that, and lets the schema keep a plain (non-unique)
        /// index so a startup migration can never fail on legacy duplicates.
        /// </summary>
        private async Task<string> GenerateUniqueOrderNumberAsync()
        {
            const int maxAttempts = 5;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = $"FTD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";
                if (!await OrderNumberExistsAsync(candidate)) return candidate;
            }

            // Exhausted the retries (pathological contention): fall back to a
            // GUID fragment, which is effectively collision-free.
            return $"FTD{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        }
    }
}
