using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Domain.Entities;
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

        public async Task<SalesOrderDto> CreateOrderAsync(CheckoutDto checkout, CartDto cart)
        {
            var orderNumber = $"FTD{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

            var order = new SalesOrder
            {
                OrderNumber = orderNumber,
                StatusId = 1, // New
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
    }
}
