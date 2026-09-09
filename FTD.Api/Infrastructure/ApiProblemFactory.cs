using FTD.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FTD.Api.Infrastructure
{
    /// <summary>
    /// Builds RFC 7807 <see cref="ProblemDetails"/> responses carrying a stable
    /// <c>errorCode</c>.
    ///
    /// Every non-success response in the API goes through here so clients get
    /// exactly one error shape to parse, and can branch on a code that never
    /// changes rather than on Arabic prose that will.
    /// </summary>
    public static class ApiProblemFactory
    {
        private const string ErrorCodeKey = "errorCode";
        private const string TraceIdKey = "traceId";

        /// <summary>Creates a problem response and attaches the correlation id.</summary>
        public static ObjectResult Create(
            HttpContext context, int statusCode, string errorCode, string detail, string? title = null)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title ?? DefaultTitleFor(statusCode),
                Detail = detail,
                Type = $"https://api.unishop.eg/errors/{errorCode.ToLowerInvariant()}"
            };

            problem.Extensions[ErrorCodeKey] = errorCode;
            // The trace id lets a support ticket be tied to a specific request
            // in the server logs.
            problem.Extensions[TraceIdKey] = context.TraceIdentifier;

            return new ObjectResult(problem)
            {
                StatusCode = statusCode,
                ContentTypes = { "application/problem+json" }
            };
        }

        public static ObjectResult BadRequest(HttpContext ctx, string detail, string? code = null)
            => Create(ctx, StatusCodes.Status400BadRequest, code ?? ApiErrorCodes.BadRequest, detail);

        public static ObjectResult Unauthorized(HttpContext ctx, string detail, string? code = null)
            => Create(ctx, StatusCodes.Status401Unauthorized, code ?? ApiErrorCodes.Unauthorized, detail);

        public static ObjectResult Forbidden(HttpContext ctx, string detail, string? code = null)
            => Create(ctx, StatusCodes.Status403Forbidden, code ?? ApiErrorCodes.Forbidden, detail);

        public static ObjectResult NotFound(HttpContext ctx, string detail, string? code = null)
            => Create(ctx, StatusCodes.Status404NotFound, code ?? ApiErrorCodes.NotFound, detail);

        public static ObjectResult Conflict(HttpContext ctx, string detail, string? code = null)
            => Create(ctx, StatusCodes.Status409Conflict, code ?? ApiErrorCodes.Conflict, detail);

        /// <summary>423 Locked — used for a temporarily locked-out account.</summary>
        public static ObjectResult Locked(HttpContext ctx, string detail, string? code = null)
            => Create(ctx, StatusCodes.Status423Locked, code ?? ApiErrorCodes.AccountLocked, detail);

        /// <summary>
        /// Maps an <see cref="ApiErrorCodes"/> value to the HTTP status that
        /// belongs with it, so service-layer failures translate consistently
        /// no matter which controller surfaces them.
        /// </summary>
        public static ObjectResult FromErrorCode(HttpContext ctx, string errorCode, string detail)
        {
            var status = errorCode switch
            {
                ApiErrorCodes.ValidationFailed or
                ApiErrorCodes.BadRequest or
                ApiErrorCodes.WeakPassword or
                ApiErrorCodes.CartEmpty or
                ApiErrorCodes.ProductInactive or
                ApiErrorCodes.ProductOutOfStock => StatusCodes.Status400BadRequest,

                ApiErrorCodes.InvalidCredentials or
                ApiErrorCodes.TokenExpired or
                ApiErrorCodes.TokenInvalid or
                ApiErrorCodes.RefreshTokenInvalid or
                ApiErrorCodes.RefreshTokenReused or
                ApiErrorCodes.Unauthorized => StatusCodes.Status401Unauthorized,

                ApiErrorCodes.Forbidden or
                ApiErrorCodes.AdminOnly or
                ApiErrorCodes.AccountDisabled => StatusCodes.Status403Forbidden,

                ApiErrorCodes.ProductNotFound or
                ApiErrorCodes.CategoryNotFound or
                ApiErrorCodes.BrandNotFound or
                ApiErrorCodes.PageNotFound or
                ApiErrorCodes.OrderNotFound or
                ApiErrorCodes.CartItemNotFound or
                ApiErrorCodes.NotFound => StatusCodes.Status404NotFound,

                ApiErrorCodes.EmailAlreadyExists or
                ApiErrorCodes.OrderNotCancellable or
                ApiErrorCodes.IdempotencyKeyConflict or
                ApiErrorCodes.Conflict => StatusCodes.Status409Conflict,

                // 423 Locked communicates "correct credentials, try later"
                // distinctly from 401, so the app can show a countdown instead
                // of telling the user their password is wrong.
                ApiErrorCodes.AccountLocked => StatusCodes.Status423Locked,

                ApiErrorCodes.RateLimited => StatusCodes.Status429TooManyRequests,
                ApiErrorCodes.Maintenance => StatusCodes.Status503ServiceUnavailable,

                _ => StatusCodes.Status400BadRequest
            };

            return Create(ctx, status, errorCode, detail);
        }

        private static string DefaultTitleFor(int statusCode) => statusCode switch
        {
            StatusCodes.Status400BadRequest => "طلب غير صالح",
            StatusCodes.Status401Unauthorized => "غير مصادق",
            StatusCodes.Status403Forbidden => "غير مصرح",
            StatusCodes.Status404NotFound => "غير موجود",
            StatusCodes.Status409Conflict => "تعارض",
            StatusCodes.Status422UnprocessableEntity => "بيانات غير قابلة للمعالجة",
            StatusCodes.Status423Locked => "الحساب مقفل",
            StatusCodes.Status429TooManyRequests => "طلبات كثيرة جداً",
            StatusCodes.Status503ServiceUnavailable => "الخدمة غير متاحة",
            _ => "خطأ في الخادم"
        };
    }
}
