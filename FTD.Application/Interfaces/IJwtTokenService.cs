using System.Collections.Generic;
using FTD.Domain.Entities;

namespace FTD.Application.Interfaces
{
    /// <summary>
    /// Issues signed access tokens and cryptographically strong refresh tokens.
    ///
    /// Abstracted so the Application layer can orchestrate the login flow
    /// without referencing JWT libraries or reading configuration directly; the
    /// concrete signing implementation lives in Infrastructure.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>Access-token lifetime in seconds (surfaced to clients as expiresIn).</summary>
        int AccessTokenLifetimeSeconds { get; }

        int RefreshTokenLifetimeDays { get; }

        /// <summary>Creates a signed JWT carrying the user's id, email and roles.</summary>
        string CreateAccessToken(AppUser user, IList<string> roles);

        /// <summary>
        /// Generates a new refresh token.
        /// Returns the RAW value (sent to the client exactly once) and its
        /// SHA-256 hash (the only form persisted), so a database leak cannot be
        /// replayed against the API.
        /// </summary>
        (string RawToken, string TokenHash) CreateRefreshToken();

        /// <summary>Hashes a client-presented refresh token for lookup.</summary>
        string HashRefreshToken(string rawToken);
    }
}
