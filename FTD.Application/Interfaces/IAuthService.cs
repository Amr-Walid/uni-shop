using System.Threading.Tasks;
using FTD.Application.DTOs;

namespace FTD.Application.Interfaces
{
    /// <summary>
    /// Customer and admin authentication, including refresh-token rotation.
    ///
    /// Lives in the Application layer (not in the API project) so that any
    /// future host — the MVC site, a second API, a background job — shares one
    /// implementation of the login/lockout/token rules rather than duplicating
    /// security-critical logic.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>Creates a Customer account and signs it in immediately.</summary>
        Task<AuthOperationResult> RegisterAsync(RegisterRequestDto request, string? ip = null);

        /// <summary>
        /// Signs in any active account (Customer or Admin).
        /// Honours Identity's lockout so brute-force attempts are throttled.
        /// </summary>
        Task<AuthOperationResult> LoginAsync(LoginRequestDto request, string? ip = null);

        /// <summary>
        /// Signs in ONLY accounts holding the Admin role. Kept separate from
        /// <see cref="LoginAsync"/> so the admin surface stays closed to
        /// customers even if a future change relaxes the customer path.
        /// </summary>
        Task<AuthOperationResult> AdminLoginAsync(LoginRequestDto request, string? ip = null);

        /// <summary>
        /// Exchanges a valid refresh token for a new token pair, rotating the
        /// presented token. Replaying an already-rotated token is treated as
        /// theft and revokes the entire token family.
        /// </summary>
        Task<AuthOperationResult> RefreshAsync(RefreshRequestDto request, string? ip = null);

        /// <summary>Revokes a single refresh token (sign out on this device).</summary>
        Task<bool> LogoutAsync(string refreshToken);

        /// <summary>Revokes every refresh token for the user (sign out everywhere).</summary>
        Task<bool> LogoutAllAsync(string userId);

        Task<UserProfileDto?> GetProfileAsync(string userId);
        Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto request);

        Task<AuthOperationResult> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);

        /// <summary>
        /// Issues a password-reset token and emails it.
        /// Always reports success so the endpoint cannot be used to enumerate
        /// which email addresses have accounts.
        /// </summary>
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request);

        Task<AuthOperationResult> ResetPasswordAsync(ResetPasswordRequestDto request);

        /// <summary>
        /// Anonymises the account and revokes all its tokens.
        ///
        /// Required by App Store Review Guideline 5.1.1(v): any app offering
        /// account creation must offer in-app deletion. We anonymise rather than
        /// hard-delete so historical orders — and therefore revenue reporting —
        /// remain intact.
        /// </summary>
        Task<bool> DeleteAccountAsync(string userId);
    }
}
