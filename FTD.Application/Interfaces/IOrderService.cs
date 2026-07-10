using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    public interface IOrderService
    {
        Task<SalesOrderDto> CreateOrderAsync(CheckoutDto checkout, CartDto cart);
        Task UpdateStatusAsync(int orderId, int statusId);
        Task<List<SalesOrderDto>> GetOrdersAsync(int? statusId);
        Task<SalesOrderDto?> GetOrderByIdAsync(int id);
        Task<List<OrderStatusDto>> GetAllStatusesAsync();
    }
}
