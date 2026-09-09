using System.Threading.Tasks;

namespace FTD.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendContactNotificationAsync(string name, string email, string phone, string message);

        /// <summary>
        /// Emails a password-reset token to the account owner.
        ///
        /// The token is delivered out-of-band on purpose: the API response to
        /// "forgot password" must never contain it, otherwise anyone could reset
        /// any account by simply calling the endpoint.
        /// </summary>
        Task SendPasswordResetAsync(string toEmail, string userName, string resetToken, string language = "ar");
    }
}
