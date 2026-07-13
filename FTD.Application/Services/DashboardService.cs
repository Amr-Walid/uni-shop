using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Application.Mappers;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IAppDbContext _db;

        public DashboardService(IAppDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var today = DateTime.UtcNow.Date;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var recentOrders = await _db.SalesOrders
                .AsNoTracking()
                .Include(o => o.Status)
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToListAsync();

            var totalProducts = await _db.Products.CountAsync(p => p.IsActive);
            var totalOrders = await _db.SalesOrders.CountAsync();
            var newOrders = await _db.SalesOrders.CountAsync(o => o.StatusId == 1);
            var pendingOrders = await _db.SalesOrders.CountAsync(o => o.StatusId == 3 || o.StatusId == 4);

            var todayRevenue = await _db.SalesOrders
                .Where(o => o.CreatedAt.Date == today && o.StatusId != 7)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var monthRevenue = await _db.SalesOrders
                .Where(o => o.CreatedAt >= monthStart && o.StatusId != 7)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var ordersByStatus = await _db.OrderStatuses
                .Select(s => new OrderStatusCountDto
                {
                    StatusName = s.NameAr,
                    ColorHex = s.ColorHex,
                    Count = s.Orders.Count
                })
                .ToListAsync();

            return new DashboardDto
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                NewOrders = newOrders,
                PendingOrders = pendingOrders,
                TodayRevenue = todayRevenue,
                MonthRevenue = monthRevenue,
                RecentOrders = recentOrders.Select(o => o.ToDto()).Where(o => o != null).Select(o => o!).ToList(),
                OrdersByStatus = ordersByStatus
            };
        }
    }
}
