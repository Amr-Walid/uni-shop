using System.Threading.Tasks;

namespace FTD.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendContactNotificationAsync(string name, string email, string phone, string message);
    }
}
