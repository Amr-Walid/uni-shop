using System.Collections.Generic;
using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    public interface IMessageService
    {
        Task SaveMessageAsync(ContactMessageDto dto);
        Task<List<ContactMessageDto>> GetAllMessagesAsync();
        Task MarkReadAsync(int id);
        Task DeleteAsync(int id);
        Task DeleteReadAsync();
        Task<int> GetUnreadCountAsync();
    }
}
