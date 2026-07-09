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
    public class OrderService
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
                CreatedAt = DateTime.UtcNow
            };

            _db.SalesOrders.Add(order);
            await _db.SaveChangesAsync();

            foreach (var item in cart.Items)
            {
                _db.SalesOrderDetails.Add(new SalesOrderDetail
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    SubTotal = item.SubTotal
                });
            }
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
    }
}
