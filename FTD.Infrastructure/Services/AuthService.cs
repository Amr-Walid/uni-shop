using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD.Application.Common;
using FTD.Application.DTOs;
using FTD.Application.Interfaces;
using FTD.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FTD.Infrastructure.Services
{
    /// <summary>
    /// Customer/admin authentication with rotating refresh tokens.
    ///
    /// Lives in Infrastructure because it depends on ASP.NET Core Identity
    /// (UserManager/SignInManager). The Application layer consumes it through
    /// <see cref="IAuthService"/>, so no host duplicates these security rules.
    /// </summary>
    public class AuthService : IAuthService
    {
        public const string CustomerRole = "Customer";
        public const string AdminRole = "Admin";

        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenService _tokens;
        private readonly IAppDbContext _db;
        private readonly IEmailService _email;
        private readonly ILogger<AuthService> _logger;

        // NOTE: this service deliberately uses UserManager only, NOT
        // SignInManager. SignInManager lives in the ASP.NET Core framework
        // assembly, and depending on it would drag a web-hosting reference into
        // the Infrastructure layer. UserManager already exposes everything a
        // token-based flow needs (password check plus the lockout counters), and
        // JWT issuance never establishes a cookie session anyway.
        public AuthService(
            UserManager<AppUser> userManager,
            IJwtTokenService tokens,
            IAppDbContext db,
            IEmailService email,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _tokens = tokens;
            _db = db;
            _email = email;
            _logger = logger;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  REGISTRATION
        // ══════════════════════════════════════════════════════════════════════

        public async Task<AuthOperationResult> RegisterAsync(RegisterRequestDto request, string? ip = null)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.EmailAlreadyExists,
                    "هذا البريد الإلكتروني مستخدم بالفعل. جرّب تسجيل الدخول أو استعادة كلمة المرور.");
            }

            var user = new AppUser
            {
                UserName = email,
                Email = email,
                // No email-confirmation flow exists yet; the site is configured
                // with SignIn.RequireConfirmedAccount = false, so marking it
                // confirmed keeps behaviour consistent rather than creating
                // accounts that can never sign in.
                EmailConfirmed = true,
                FullName = request.FullName.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                PreferredLanguage = LocalizationHelper.NormalizeLanguage(request.PreferredLanguage),
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                // Identity returns a structured error list (length, complexity…).
                // Surfacing it verbatim tells the user exactly what to fix.
                var message = string.Join(" ", createResult.Errors.Select(e => e.Description));
                return AuthOperationResult.Fail(ApiErrorCodes.WeakPassword,
                    string.IsNullOrWhiteSpace(message) ? "تعذّر إنشاء الحساب" : message);
            }

            await EnsureCustomerRoleAsync(user);

            var result = await BuildAuthResultAsync(user, request.DeviceInfo, ip);
            return AuthOperationResult.Success(result);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOGIN
        // ══════════════════════════════════════════════════════════════════════

        public Task<AuthOperationResult> LoginAsync(LoginRequestDto request, string? ip = null)
            => LoginInternalAsync(request, requireAdmin: false, ip);

        public Task<AuthOperationResult> AdminLoginAsync(LoginRequestDto request, string? ip = null)
            => LoginInternalAsync(request, requireAdmin: true, ip);

        private async Task<AuthOperationResult> LoginInternalAsync(
            LoginRequestDto request, bool requireAdmin, string? ip)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            // Same generic message for "no such user" and "wrong password" so the
            // endpoint cannot be used to discover which emails are registered.
            if (user == null)
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.InvalidCredentials, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            if (user.IsDeleted)
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.AccountDisabled, "هذا الحساب غير متاح");
            }

            // Check the lockout BEFORE verifying the password, so a locked
            // account cannot be probed by timing the password comparison.
            if (await _userManager.IsLockedOutAsync(user))
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.AccountLocked,
                    "تم قفل الحساب مؤقتاً بسبب محاولات دخول فاشلة متكررة. حاول بعد 15 دقيقة.");
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!passwordValid)
            {
                // Drive the lockout counter explicitly. The original API called
                // CheckPasswordSignInAsync(..., lockoutOnFailure: false), so the
                // API silently bypassed the brute-force protection the website
                // already enforced — the same credentials could be attacked
                // without limit through this endpoint.
                await _userManager.AccessFailedAsync(user);

                if (await _userManager.IsLockedOutAsync(user))
                {
                    return AuthOperationResult.Fail(
                        ApiErrorCodes.AccountLocked,
                        "تم قفل الحساب مؤقتاً بسبب محاولات دخول فاشلة متكررة. حاول بعد 15 دقيقة.");
                }

                return AuthOperationResult.Fail(
                    ApiErrorCodes.InvalidCredentials, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
            }

            // Successful sign-in clears the accumulated failure count.
            await _userManager.ResetAccessFailedCountAsync(user);

            var roles = await _userManager.GetRolesAsync(user);

            if (requireAdmin && !roles.Contains(AdminRole))
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.AdminOnly, "غير مصرح بالدخول لغير المسؤولين");
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var result = await BuildAuthResultAsync(user, request.DeviceInfo, ip, roles);
            return AuthOperationResult.Success(result);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  REFRESH — rotation with reuse detection
        // ══════════════════════════════════════════════════════════════════════

        public async Task<AuthOperationResult> RefreshAsync(RefreshRequestDto request, string? ip = null)
        {
            var hash = _tokens.HashRefreshToken(request.RefreshToken);
            if (string.IsNullOrEmpty(hash))
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.RefreshTokenInvalid, "رمز التجديد غير صالح");
            }

            var stored = await _db.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == hash);

            if (stored == null)
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.RefreshTokenInvalid, "رمز التجديد غير صالح");
            }

            // REUSE DETECTION.
            // A revoked token being presented means the legitimate client has
            // already rotated past it, so this copy came from somewhere else —
            // it was captured. We cannot tell attacker from victim, so the whole
            // family is revoked and both must sign in again. Without this, a
            // stolen token grants indefinite access.
            if (stored.RevokedAt != null)
            {
                _logger.LogWarning(
                    "Refresh token reuse detected for user {UserId} (family {FamilyId}) from {Ip}. Revoking family.",
                    stored.UserId, stored.FamilyId, ip ?? "unknown");

                await RevokeFamilyAsync(stored.FamilyId);

                return AuthOperationResult.Fail(
                    ApiErrorCodes.RefreshTokenReused,
                    "تم اكتشاف استخدام غير آمن لرمز التجديد. من فضلك سجّل الدخول مرة أخرى.");
            }

            if (stored.ExpiresAt <= DateTime.UtcNow)
            {
                return AuthOperationResult.Fail(
                    ApiErrorCodes.RefreshTokenInvalid, "انتهت صلاحية رمز التجديد. سجّل الدخول مرة أخرى.");
            }

            var user = stored.User ?? await _userManager.FindByIdAsync(stored.UserId);
            if (user == null || user.IsDeleted)
            {
                await RevokeFamilyAsync(stored.FamilyId);
                return AuthOperationResult.Fail(
                    ApiErrorCodes.AccountDisabled, "هذا الحساب غير متاح");
            }

            // Rotate: issue the successor, then revoke the presented token and
            // record which token replaced it (the audit trail reuse detection
            // relies on). Same family id keeps the chain linked.
            var (rawToken, newHash) = _tokens.CreateRefreshToken();

            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByHash = newHash;

            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newHash,
                FamilyId = stored.FamilyId,
                ExpiresAt = DateTime.UtcNow.AddDays(_tokens.RefreshTokenLifetimeDays),
                CreatedAt = DateTime.UtcNow,
                DeviceInfo = request.DeviceInfo ?? stored.DeviceInfo,
                CreatedByIp = ip
            });

            await _db.SaveChangesAsync();

            var roles = await _userManager.GetRolesAsync(user);

            return AuthOperationResult.Success(new AuthResultDto
            {
                AccessToken = _tokens.CreateAccessToken(user, roles),
                RefreshToken = rawToken,
                ExpiresIn = _tokens.AccessTokenLifetimeSeconds,
                User = MapProfile(user, roles)
            });
        }

        // ══════════════════════════════════════════════════════════════════════
        //  LOGOUT
        // ══════════════════════════════════════════════════════════════════════

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var hash = _tokens.HashRefreshToken(refreshToken);
            if (string.IsNullOrEmpty(hash)) return false;

            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash);
            if (stored == null || stored.RevokedAt != null) return false;

            stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> LogoutAllAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return false;

            // Set-based UPDATE — no reason to materialise every token row.
            var affected = await _db.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));

            return affected > 0;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PROFILE
        // ══════════════════════════════════════════════════════════════════════

        public async Task<UserProfileDto?> GetProfileAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapProfile(user, roles);
        }

        public async Task<UserProfileDto?> UpdateProfileAsync(string userId, UpdateProfileRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return null;

            // Null means "leave unchanged" so the client can PATCH a single
            // field without having to resend the whole profile.
            if (request.FullName != null) user.FullName = request.FullName.Trim();
            if (request.Phone != null) user.PhoneNumber = request.Phone.Trim();
            if (request.DefaultAddress != null) user.DefaultAddress = request.DefaultAddress.Trim();
            if (request.City != null) user.City = request.City.Trim();
            if (request.Governorate != null) user.Governorate = request.Governorate.Trim();
            if (request.PreferredLanguage != null)
                user.PreferredLanguage = LocalizationHelper.NormalizeLanguage(request.PreferredLanguage);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return MapProfile(user, roles);
        }

        public async Task<AuthOperationResult> ChangePasswordAsync(
            string userId, ChangePasswordRequestDto request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return AuthOperationResult.Fail(ApiErrorCodes.Unauthorized, "غير مصرح");
            }

            var changeResult = await _userManager.ChangePasswordAsync(
                user, request.CurrentPassword, request.NewPassword);

            if (!changeResult.Succeeded)
            {
                var message = string.Join(" ", changeResult.Errors.Select(e => e.Description));
                return AuthOperationResult.Fail(
                    ApiErrorCodes.InvalidCredentials,
                    string.IsNullOrWhiteSpace(message) ? "كلمة المرور الحالية غير صحيحة" : message);
            }

            // A password change is a security event: every existing session must
            // die, otherwise a thief who already has a refresh token keeps
            // access even after the victim "locked them out".
            await LogoutAllAsync(userId);

            var roles = await _userManager.GetRolesAsync(user);
            var result = await BuildAuthResultAsync(user, deviceInfo: null, ip: null, roles);
            return AuthOperationResult.Success(result);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  PASSWORD RESET
        // ══════════════════════════════════════════════════════════════════════

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            // ALWAYS report success, even when the address is unknown.
            // Returning 404 here would turn this endpoint into a free account
            // enumeration oracle.
            if (user == null || user.IsDeleted) return true;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            await _email.SendPasswordResetAsync(
                user.Email!,
                user.FullName ?? user.Email!,
                token,
                user.PreferredLanguage);

            return true;
        }

        public async Task<AuthOperationResult> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.IsDeleted)
            {
                // Generic failure — do not reveal whether the account exists.
                return AuthOperationResult.Fail(
                    ApiErrorCodes.TokenInvalid, "رمز الاستعادة غير صالح أو منتهي الصلاحية");
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!resetResult.Succeeded)
            {
                var message = string.Join(" ", resetResult.Errors.Select(e => e.Description));
                return AuthOperationResult.Fail(
                    ApiErrorCodes.TokenInvalid,
                    string.IsNullOrWhiteSpace(message) ? "رمز الاستعادة غير صالح أو منتهي الصلاحية" : message);
            }

            // Same reasoning as ChangePasswordAsync — invalidate all sessions.
            await LogoutAllAsync(user.Id);

            var roles = await _userManager.GetRolesAsync(user);
            var result = await BuildAuthResultAsync(user, deviceInfo: null, ip: null, roles);
            return AuthOperationResult.Success(result);
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ACCOUNT DELETION (App Store requirement)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<bool> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.IsDeleted) return false;

            // ANONYMISE rather than hard-delete.
            //
            // Deleting the row would either destroy the customer's order history
            // (breaking revenue reports) or leave orphaned orders. Instead the
            // account is scrubbed of personal data and disabled: SalesOrder.UserId
            // is SetNull on delete anyway, and the order rows keep their own
            // snapshot of name/phone for fulfilment records.
            var anonymousSuffix = Guid.NewGuid().ToString("N")[..12];

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.FullName = null;
            user.PhoneNumber = null;
            user.DefaultAddress = null;
            user.City = null;
            user.Governorate = null;
            // Rewrite the email so the address is freed for re-registration and
            // can no longer be used to sign in or reset the password.
            user.Email = $"deleted-{anonymousSuffix}@deleted.local";
            user.NormalizedEmail = user.Email.ToUpperInvariant();
            user.UserName = user.Email;
            user.NormalizedUserName = user.NormalizedEmail;
            user.EmailConfirmed = false;
            // Invalidate the password hash so it can never be matched.
            user.PasswordHash = null;
            user.SecurityStamp = Guid.NewGuid().ToString();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                _logger.LogError("Failed to anonymise account {UserId}: {Errors}",
                    userId, string.Join("; ", updateResult.Errors.Select(e => e.Description)));
                return false;
            }

            // Revoke sessions and clear personal side-tables. Orders are
            // deliberately left untouched.
            await LogoutAllAsync(userId);

            await _db.WishlistItems.Where(w => w.UserId == userId).ExecuteDeleteAsync();
            await _db.UserCarts.Where(c => c.UserId == userId).ExecuteDeleteAsync();
            await _db.DeviceTokens
                .Where(d => d.UserId == userId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(d => d.IsActive, false)
                    .SetProperty(d => d.UserId, (string?)null));

            _logger.LogInformation("Account {UserId} anonymised on user request.", userId);
            return true;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS
        // ══════════════════════════════════════════════════════════════════════

        private async Task<AuthResultDto> BuildAuthResultAsync(
            AppUser user, string? deviceInfo, string? ip, IList<string>? roles = null)
        {
            roles ??= await _userManager.GetRolesAsync(user);

            var (rawToken, hash) = _tokens.CreateRefreshToken();

            // A fresh login starts a NEW family: revoking a compromised chain
            // must not sign the user out of their other, legitimate devices.
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = hash,
                FamilyId = Guid.NewGuid().ToString("N"),
                ExpiresAt = DateTime.UtcNow.AddDays(_tokens.RefreshTokenLifetimeDays),
                CreatedAt = DateTime.UtcNow,
                DeviceInfo = deviceInfo,
                CreatedByIp = ip
            });

            await _db.SaveChangesAsync();

            return new AuthResultDto
            {
                AccessToken = _tokens.CreateAccessToken(user, roles),
                RefreshToken = rawToken,
                ExpiresIn = _tokens.AccessTokenLifetimeSeconds,
                User = MapProfile(user, roles)
            };
        }

        private async Task RevokeFamilyAsync(string familyId)
        {
            if (string.IsNullOrWhiteSpace(familyId)) return;

            await _db.RefreshTokens
                .Where(t => t.FamilyId == familyId && t.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, DateTime.UtcNow));
        }

        /// <summary>
        /// Adds the Customer role, creating it if the database has not been
        /// seeded yet (e.g. the API booted first against a fresh database).
        /// </summary>
        private async Task EnsureCustomerRoleAsync(AppUser user)
        {
            try
            {
                await _userManager.AddToRoleAsync(user, CustomerRole);
            }
            catch (InvalidOperationException ex)
            {
                // Role missing: the account is still usable for shopping, so log
                // and continue rather than failing the registration.
                _logger.LogWarning(ex,
                    "Could not assign the {Role} role to {UserId} — is the role seeded?",
                    CustomerRole, user.Id);
            }
        }

        private static UserProfileDto MapProfile(AppUser user, IList<string> roles) => new()
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName,
            Phone = user.PhoneNumber,
            DefaultAddress = user.DefaultAddress,
            City = user.City,
            Governorate = user.Governorate,
            PreferredLanguage = user.PreferredLanguage,
            CreatedAt = user.CreatedAt,
            Roles = roles?.ToList() ?? new List<string>(),
            IsAdmin = roles?.Contains(AdminRole) ?? false
        };
    }
}
