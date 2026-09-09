using System.Threading.Tasks;

namespace FTD.Application.Interfaces
{
    /// <summary>
    /// Replay protection for mutating endpoints that carry an
    /// <c>Idempotency-Key</c> header.
    ///
    /// WHY: mobile networks drop requests mid-flight constantly. A client that
    /// never receives the response cannot distinguish "the order was not
    /// created" from "the order was created but the reply was lost", so it
    /// retries. Without this guard the customer ends up with two identical
    /// orders and stock is deducted twice.
    /// </summary>
    public interface IIdempotencyService
    {
        /// <summary>
        /// Looks up a previous result for this key.
        /// Returns <c>Found = true</c> with the stored response when the exact
        /// same request was already processed, or <c>Conflict = true</c> when the
        /// key was reused with a DIFFERENT body (a client bug or an attack) —
        /// the caller must then reject the request rather than return an
        /// unrelated result.
        /// </summary>
        Task<(bool Found, bool Conflict, string? ResponseJson, int StatusCode)> TryGetAsync(
            string key, string endpoint, string requestHash);

        /// <summary>Stores a successful response so a retry can replay it verbatim.</summary>
        Task SaveAsync(string key, string endpoint, string requestHash, string responseJson, int statusCode);

        /// <summary>Computes the canonical hash used to detect payload mismatches.</summary>
        string ComputeRequestHash(string payload);

        /// <summary>Deletes expired records. Safe to call opportunistically.</summary>
        Task<int> PruneExpiredAsync();
    }
}
