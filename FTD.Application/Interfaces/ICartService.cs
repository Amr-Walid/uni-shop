using System.Threading.Tasks;
using FTD.Application.DTOs;
using Microsoft.AspNetCore.Http;

namespace FTD.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(ISession session);
        void AddItem(ISession session, int productId, int qty = 1);
        void UpdateQty(ISession session, int productId, int qty);
        void RemoveItem(ISession session, int productId);
        void ClearCart(ISession session);
        int GetCount(ISession session);
    }
}
