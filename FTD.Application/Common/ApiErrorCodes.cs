namespace FTD.Application.Common
{
    /// <summary>
    /// Stable, machine-readable error codes returned in the <c>errorCode</c>
    /// member of every RFC 7807 ProblemDetails response.
    ///
    /// WHY CODES AND NOT MESSAGES: the human-readable messages in this project
    /// are Arabic and will be reworded by content editors over time. A mobile
    /// app that branches on message text breaks the moment a word changes, and
    /// a published build cannot be patched quickly. Clients therefore branch on
    /// these constants, which are part of the API contract and never change
    /// meaning.
    /// </summary>
    public static class ApiErrorCodes
    {
        // ── Validation ────────────────────────────────────────────────────────
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string BadRequest = "BAD_REQUEST";

        // ── Authentication / authorization ───────────────────────────────────
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string TokenExpired = "TOKEN_EXPIRED";
        public const string TokenInvalid = "TOKEN_INVALID";
        public const string RefreshTokenInvalid = "REFRESH_TOKEN_INVALID";
        /// <summary>A revoked refresh token was replayed — the family is revoked.</summary>
        public const string RefreshTokenReused = "REFRESH_TOKEN_REUSED";
        public const string AccountLocked = "ACCOUNT_LOCKED";
        public const string AccountDisabled = "ACCOUNT_DISABLED";
        public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
        public const string WeakPassword = "WEAK_PASSWORD";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string AdminOnly = "ADMIN_ONLY";

        // ── Catalog ──────────────────────────────────────────────────────────
        public const string ProductNotFound = "PRODUCT_NOT_FOUND";
        public const string ProductInactive = "PRODUCT_INACTIVE";
        public const string ProductOutOfStock = "PRODUCT_OUT_OF_STOCK";
        public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
        public const string BrandNotFound = "BRAND_NOT_FOUND";
        public const string PageNotFound = "PAGE_NOT_FOUND";

        // ── Cart ─────────────────────────────────────────────────────────────
        public const string CartEmpty = "CART_EMPTY";
        public const string CartItemNotFound = "CART_ITEM_NOT_FOUND";

        // ── Orders ───────────────────────────────────────────────────────────
        public const string OrderNotFound = "ORDER_NOT_FOUND";
        public const string OrderNotCancellable = "ORDER_NOT_CANCELLABLE";
        public const string OrderStatusInvalid = "ORDER_STATUS_INVALID";

        // ── Idempotency ──────────────────────────────────────────────────────
        /// <summary>Same key replayed with a different body — a client bug.</summary>
        public const string IdempotencyKeyConflict = "IDEMPOTENCY_KEY_CONFLICT";

        // ── Infrastructure ───────────────────────────────────────────────────
        public const string RateLimited = "RATE_LIMITED";
        public const string NotFound = "NOT_FOUND";
        public const string Conflict = "CONFLICT";
        public const string ServerError = "SERVER_ERROR";
        public const string Maintenance = "MAINTENANCE";
    }
}
