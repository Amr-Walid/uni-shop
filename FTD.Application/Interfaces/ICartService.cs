using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(ICartStorage storage);
        void AddItem(ICartStorage storage, int productId, int qty = 1);
        void UpdateQty(ICartStorage storage, int productId, int qty);
        void RemoveItem(ICartStorage storage, int productId);
        void ClearCart(ICartStorage storage);
        int GetCount(ICartStorage storage);
    }
}
