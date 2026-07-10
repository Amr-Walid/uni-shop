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
                ShippingFee = cart.FreeShipping ? 0 : cart.ShippingFee,
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

            _db.SalesOrders.Add(order);
            await _db.SaveChangesAsync();

            // Fetch the saved order with navigation properties to return a fully populated DTO
            var savedOrder = await _db.SalesOrders
                .Include(o => o.Status)
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return savedOrder.ToDto()!;
        }

        public async Task UpdateStatusAsync(int orderId, int statusId)
        {
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
            var query = _db.SalesOrders.Include(o => o.Status).AsQueryable();

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
                .Include(o => o.Status)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order.ToDto();
        }

        public async Task<List<OrderStatusDto>> GetAllStatusesAsync()
        {
            var statuses = await _db.OrderStatuses
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return statuses
                .Select(s => s.ToDto())
                .OfType<OrderStatusDto>()
                .ToList();
        }
    }
}
