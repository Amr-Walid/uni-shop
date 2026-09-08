using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FTD.Application.Interfaces;
using FTD.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FTD.Infrastructure.Services
{
    /// <summary>Signing and lifetime configuration for API tokens.</summary>
    public class JwtSettings
    {
        public string Secret { get; set; } = "";
        public string Issuer { get; set; } = "FTD.Api";
        public string Audience { get; set; } = "FTD.Client";

        /// <summary>
        /// Access-token lifetime. Short by design: the token cannot be revoked
        /// once issued, so a small window limits the damage of a stolen token.
        /// Continuity comes from the (revocable) refresh token instead.
        /// </summary>
        public int AccessTokenMinutes { get; set; } = 15;

        /// <summary>
        /// Refresh-token lifetime. Long enough that a normal shopper is never
        /// asked to sign in again, and revocable server-side at any time.
        /// </summary>
        public int RefreshTokenDays { get; set; } = 60;
    }

    /// <summary>
    /// Issues signed JWT access tokens and cryptographically random refresh
    /// tokens.
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        /// <summary>
        /// Minimum secret length in bytes (256 bits) — matches the HMAC-SHA256
        /// output size. A shorter key weakens the signature.
        /// </summary>
        private const int MinimumSecretBytes = 32;

        private readonly JwtSettings _settings;
        private readonly SymmetricSecurityKey _signingKey;

        public JwtTokenService(JwtSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(_settings.Secret))
            {
                throw new InvalidOperationException(
                    "JWT secret is not configured. Set JwtSettings:Secret via environment " +
                    "variable or user-secrets — never commit it to appsettings.json.");
            }

            // UTF8, not ASCII. The original code used Encoding.ASCII, which
            // silently replaces any non-ASCII byte with '?' — that collapses the
            // key space and makes a long random secret far weaker than it looks.
            var keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);

            if (keyBytes.Length < MinimumSecretBytes)
            {
                throw new InvalidOperationException(
                    $"JWT secret is too short ({keyBytes.Length} bytes). " +
                    $"Provide at least {MinimumSecretBytes} bytes (256 bits) of entropy.");
            }

            _signingKey = new SymmetricSecurityKey(keyBytes);
        }

        public int AccessTokenLifetimeSeconds => _settings.AccessTokenMinutes * 60;

        public int RefreshTokenLifetimeDays => _settings.RefreshTokenDays;

        public string CreateAccessToken(AppUser user, IList<string> roles)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.FullName ?? user.Email ?? string.Empty),
                // Unique token id — the anchor for future denylisting if needed.
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            };

            foreach (var role in roles ?? new List<string>())
                claims.Add(new Claim(ClaimTypes.Role, role));

            var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes),
                SigningCredentials = credentials,
                Issuer = _settings.Issuer,
                Audience = _settings.Audience
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public (string RawToken, string TokenHash) CreateRefreshToken()
        {
            // 256 bits from a CSPRNG. Base64Url-encoded so it survives headers,
            // JSON and URLs without escaping.
            var bytes = RandomNumberGenerator.GetBytes(32);
            var rawToken = Base64UrlEncode(bytes);
            return (rawToken, HashRefreshToken(rawToken));
        }

        public string HashRefreshToken(string rawToken)
        {
            if (string.IsNullOrWhiteSpace(rawToken)) return string.Empty;

            // Plain SHA-256 is appropriate here (unlike for passwords): the input
            // is already 256 bits of uniform entropy, so brute-forcing it is
            // infeasible and a slow KDF would only add latency to every refresh.
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToBase64String(hash);
        }

        private static string Base64UrlEncode(byte[] bytes)
            => Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
    }
}
