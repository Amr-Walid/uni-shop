using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FTD.Application.Interfaces;
using FTD.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FTD.Application.Services
{
    /// <summary>
    /// Replay protection for mutating endpoints carrying an
    /// <c>Idempotency-Key</c> header — most importantly checkout.
    /// </summary>
    public class IdempotencyService : IIdempotencyService
    {
        /// <summary>
        /// How long a key stays replayable. 24h comfortably covers a phone that
        /// was offline for a while and retries on reconnect, without growing the
        /// table unbounded.
        /// </summary>
        private static readonly TimeSpan RetentionPeriod = TimeSpan.FromHours(24);

        private readonly IAppDbContext _db;

        public IdempotencyService(IAppDbContext db) => _db = db;

        public async Task<(bool Found, bool Conflict, string? ResponseJson, int StatusCode)> TryGetAsync(
            string key, string endpoint, string requestHash)
        {
            if (string.IsNullOrWhiteSpace(key)) return (false, false, null, 0);

            var record = await _db.IdempotencyRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Key == key && r.Endpoint == endpoint);

            if (record == null) return (false, false, null, 0);

            // An expired record is treated as absent so the action can be
            // performed again rather than replaying a stale response.
            if (record.ExpiresAt <= DateTime.UtcNow) return (false, false, null, 0);

            // Same key, DIFFERENT payload: the client reused a key it should have
            // regenerated. Returning the earlier (unrelated) result would be
            // wrong and confusing, so the caller must surface a 409/422 instead.
            if (!string.IsNullOrEmpty(record.RequestHash) &&
                !string.Equals(record.RequestHash, requestHash, StringComparison.Ordinal))
            {
                return (false, true, null, 0);
            }

            return (true, false, record.ResponseJson, record.StatusCode);
        }

        public async Task SaveAsync(
            string key, string endpoint, string requestHash, string responseJson, int statusCode)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            _db.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Key = key,
                Endpoint = endpoint,
                RequestHash = requestHash,
                ResponseJson = responseJson,
                StatusCode = statusCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(RetentionPeriod)
            });

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Two concurrent retries of the same request both got past
                // TryGetAsync and raced to insert; the unique (Key, Endpoint)
                // index rejected the loser. The action itself already succeeded,
                // so swallowing this is correct — failing here would report an
                // error for an order that was actually created.
            }
        }

        public string ComputeRequestHash(string payload)
        {
            var normalized = payload ?? string.Empty;
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToBase64String(bytes);
        }

        public async Task<int> PruneExpiredAsync()
        {
            // ExecuteDeleteAsync issues a single set-based DELETE instead of
            // loading every expired row into memory first.
            return await _db.IdempotencyRecords
                .Where(r => r.ExpiresAt <= DateTime.UtcNow)
                .ExecuteDeleteAsync();
        }
    }
}
