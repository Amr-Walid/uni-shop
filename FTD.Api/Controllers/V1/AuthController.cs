using System.Threading.Tasks;
using FTD.Api.Infrastructure;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FTD.Api.Controllers.V1
{
    /// <summary>
    /// Customer and admin authentication: registration, sign-in, token
    /// rotation, password recovery and account deletion.
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    [Tags("Authentication")]
    public class AuthController : ApiControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        /// <summary>Creates a customer account and returns a token pair.</summary>
        /// <response code="200">Account created and signed in.</response>
        /// <response code="409">Email already registered (EMAIL_ALREADY_EXISTS).</response>
        /// <response code="429">Too many attempts from this IP.</response>
        [HttpPost("register")]
        [EnableRateLimiting("register-policy")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _auth.RegisterAsync(request, ClientIp);

            return result.Succeeded
                ? Ok(result.Data)
                : ProblemFromCode(result.ErrorCode!, result.ErrorMessage!);
        }

        /// <summary>
        /// Signs in a customer or admin.
        ///
        /// Rate limited to 5 attempts per 30 seconds per IP, and the account
        /// itself locks for 15 minutes after 5 failures.
        /// </summary>
        /// <response code="200">Signed in.</response>
        /// <response code="401">Wrong email or password (INVALID_CREDENTIALS).</response>
        /// <response code="423">Account temporarily locked (ACCOUNT_LOCKED).</response>
        [HttpPost("login")]
        [EnableRateLimiting("login-policy")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status423Locked)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _auth.LoginAsync(request, ClientIp);

            return result.Succeeded
                ? Ok(result.Data)
                : ProblemFromCode(result.ErrorCode!, result.ErrorMessage!);
        }

        /// <summary>
        /// Signs in an admin only.
        ///
        /// Kept separate from <c>/login</c> so the admin surface stays closed to
        /// customer accounts regardless of future changes to the customer path.
        /// </summary>
        [HttpPost("admin/login")]
        [EnableRateLimiting("login-policy")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AdminLogin([FromBody] LoginRequestDto request)
        {
            var result = await _auth.AdminLoginAsync(request, ClientIp);

            return result.Succeeded
                ? Ok(result.Data)
                : ProblemFromCode(result.ErrorCode!, result.ErrorMessage!);
        }

        /// <summary>
        /// Exchanges a refresh token for a new token pair.
        ///
        /// The presented token is rotated (revoked and replaced). Replaying an
        /// already-rotated token indicates theft and revokes the whole token
        /// family, returning REFRESH_TOKEN_REUSED — the client must then sign in
        /// again.
        ///
        /// CLIENT NOTE: serialise refresh calls behind a mutex. Firing several
        /// concurrent refreshes with the same token makes the later ones look
        /// like reuse and signs the user out.
        /// </summary>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            var result = await _auth.RefreshAsync(request, ClientIp);

            return result.Succeeded
                ? Ok(result.Data)
                : ProblemFromCode(result.ErrorCode!, result.ErrorMessage!);
        }

        /// <summary>Revokes one refresh token (sign out on this device).</summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
        {
            await _auth.LogoutAsync(request.RefreshToken);

            // Always 200: an already-revoked or unknown token still leaves the
            // caller logged out, which is the outcome they asked for.
            return Ok(new { success = true, message = "تم تسجيل الخروج بنجاح" });
        }

        /// <summary>Revokes every refresh token for the account (sign out everywhere).</summary>
        [HttpPost("logout-all")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LogoutAll()
        {
            await _auth.LogoutAllAsync(CurrentUserId!);
            return Ok(new { success = true, message = "تم تسجيل الخروج من جميع الأجهزة" });
        }

        /// <summary>
        /// Starts password recovery by emailing a reset token.
        ///
        /// Always returns 200, even for an unknown address: a 404 here would let
        /// anyone test which emails have accounts.
        /// </summary>
        [HttpPost("forgot-password")]
        [EnableRateLimiting("forgot-password-policy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            await _auth.ForgotPasswordAsync(request);

            return Ok(new
            {
                success = true,
                message = "إذا كان البريد الإلكتروني مسجلاً لدينا، ستصلك رسالة بها رمز إعادة التعيين."
            });
        }

        /// <summary>
        /// Completes password recovery using the emailed token.
        /// All existing sessions are revoked on success.
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            var result = await _auth.ResetPasswordAsync(request);

            return result.Succeeded
                ? Ok(result.Data)
                : ProblemFromCode(result.ErrorCode!, result.ErrorMessage!);
        }

        /// <summary>
        /// Changes the password of the signed-in account.
        /// Returns a fresh token pair; all other sessions are revoked.
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var result = await _auth.ChangePasswordAsync(CurrentUserId!, request);

            return result.Succeeded
                ? Ok(result.Data)
                : ProblemFromCode(result.ErrorCode!, result.ErrorMessage!);
        }

        /// <summary>
        /// Permanently deletes the signed-in account.
        ///
        /// Required by App Store Review Guideline 5.1.1(v) — an app that offers
        /// account creation must offer in-app deletion, and its absence is a
        /// guaranteed rejection.
        ///
        /// The account is anonymised rather than row-deleted so historical
        /// orders (and revenue reporting) survive; personal data is scrubbed,
        /// the email is released, and all sessions are revoked.
        /// </summary>
        [HttpDelete("account")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccount()
        {
            var deleted = await _auth.DeleteAccountAsync(CurrentUserId!);

            return deleted
                ? Ok(new { success = true, message = "تم حذف حسابك نهائياً. نأسف لمغادرتك." })
                : NotFoundProblem("الحساب غير موجود", ApiErrorCodes.NotFound);
        }
    }
}
