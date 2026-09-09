/// Mirror of the backend's `ApiErrorCodes` (FTD.Application/Common).
///
/// The API returns these in the `errorCode` member of every RFC 7807
/// ProblemDetails response. The app branches on THESE constants and never on
/// the human-readable message, because the messages are Arabic prose that
/// content editors will reword — and a published build cannot be patched
/// quickly enough to follow.
library;

class ApiErrorCodes {
  const ApiErrorCodes._();

  // ── Validation ──────────────────────────────────────────────────────────
  static const validationFailed = 'VALIDATION_FAILED';
  static const badRequest = 'BAD_REQUEST';

  // ── Authentication ──────────────────────────────────────────────────────
  static const invalidCredentials = 'INVALID_CREDENTIALS';

  /// Access token expired — the interceptor should refresh and retry silently.
  static const tokenExpired = 'TOKEN_EXPIRED';

  /// Token is structurally invalid — force a sign-out, refreshing will not help.
  static const tokenInvalid = 'TOKEN_INVALID';
  static const refreshTokenInvalid = 'REFRESH_TOKEN_INVALID';

  /// A revoked refresh token was replayed; the server revoked the whole family.
  /// The user must sign in again.
  static const refreshTokenReused = 'REFRESH_TOKEN_REUSED';

  static const accountLocked = 'ACCOUNT_LOCKED';
  static const accountDisabled = 'ACCOUNT_DISABLED';
  static const emailAlreadyExists = 'EMAIL_ALREADY_EXISTS';
  static const weakPassword = 'WEAK_PASSWORD';
  static const unauthorized = 'UNAUTHORIZED';
  static const forbidden = 'FORBIDDEN';
  static const adminOnly = 'ADMIN_ONLY';

  // ── Catalog ─────────────────────────────────────────────────────────────
  static const productNotFound = 'PRODUCT_NOT_FOUND';
  static const productInactive = 'PRODUCT_INACTIVE';
  static const productOutOfStock = 'PRODUCT_OUT_OF_STOCK';
  static const categoryNotFound = 'CATEGORY_NOT_FOUND';
  static const brandNotFound = 'BRAND_NOT_FOUND';
  static const pageNotFound = 'PAGE_NOT_FOUND';

  // ── Cart ────────────────────────────────────────────────────────────────
  static const cartEmpty = 'CART_EMPTY';
  static const cartItemNotFound = 'CART_ITEM_NOT_FOUND';

  // ── Orders ──────────────────────────────────────────────────────────────
  static const orderNotFound = 'ORDER_NOT_FOUND';
  static const orderNotCancellable = 'ORDER_NOT_CANCELLABLE';
  static const orderStatusInvalid = 'ORDER_STATUS_INVALID';

  // ── Idempotency ─────────────────────────────────────────────────────────
  static const idempotencyKeyConflict = 'IDEMPOTENCY_KEY_CONFLICT';

  // ── Infrastructure ──────────────────────────────────────────────────────
  static const rateLimited = 'RATE_LIMITED';
  static const notFound = 'NOT_FOUND';
  static const conflict = 'CONFLICT';
  static const serverError = 'SERVER_ERROR';
  static const maintenance = 'MAINTENANCE';

  // ── Client-only codes (never sent by the server) ─────────────────────────
  static const networkError = 'NETWORK_ERROR';
  static const timeout = 'TIMEOUT';
  static const unknown = 'UNKNOWN';

  /// Codes that mean "the session is unrecoverable — sign the user out".
  ///
  /// Note that [tokenExpired] is deliberately NOT in this set: it is the normal,
  /// expected path handled by a transparent refresh.
  static const Set<String> fatalAuthCodes = {
    tokenInvalid,
    refreshTokenInvalid,
    refreshTokenReused,
    accountDisabled,
  };
}
