using System;
using System.Threading;
using System.Threading.Tasks;
using FTD.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FTD.Api.Infrastructure
{
    /// <summary>
    /// Converts any unhandled exception into an RFC 7807 problem response.
    ///
    /// Without this, an unexpected exception returns the default developer/HTML
    /// error page. A mobile client parsing JSON would fail on that, turning a
    /// recoverable server error into an unexplained crash — and in production
    /// the default page can also leak stack traces and connection strings.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            // A cancelled request is the client hanging up (very common on
            // mobile), not a server fault. Logging it as an error would bury
            // real failures in noise, and there is nobody left to answer.
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogDebug("Request {Path} was aborted by the client.", httpContext.Request.Path);
                return true;
            }

            var (statusCode, errorCode, detail) = MapException(exception);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception,
                    "Unhandled exception on {Method} {Path} (trace {TraceId})",
                    httpContext.Request.Method, httpContext.Request.Path, httpContext.TraceIdentifier);
            }
            else
            {
                _logger.LogWarning(
                    "Request failed on {Method} {Path}: {Message}",
                    httpContext.Request.Method, httpContext.Request.Path, exception.Message);
            }

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode >= 500 ? "خطأ في الخادم" : "طلب غير صالح",
                Detail = detail,
                Type = $"https://api.unishop.eg/errors/{errorCode.ToLowerInvariant()}"
            };

            problem.Extensions["errorCode"] = errorCode;
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            // Stack traces are attached ONLY outside production. In production
            // they would hand an attacker a map of the internals.
            if (!_environment.IsProduction() && statusCode >= 500)
            {
                problem.Extensions["exception"] = exception.GetType().Name;
                problem.Extensions["stackTrace"] = exception.StackTrace;
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/problem+json";
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

            return true;
        }

        /// <summary>
        /// Maps known exception types to a status and error code.
        ///
        /// The service layer signals expected business-rule violations with
        /// InvalidOperationException (e.g. "out of stock", "slug already used"),
        /// and those messages are already user-facing Arabic, so they are passed
        /// through as 400s rather than reported as server errors.
        /// </summary>
        private (int StatusCode, string ErrorCode, string Detail) MapException(Exception exception) => exception switch
        {
            InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.BadRequest,
                exception.Message),

            ArgumentException => (
                StatusCodes.Status400BadRequest,
                ApiErrorCodes.ValidationFailed,
                exception.Message),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                ApiErrorCodes.Forbidden,
                "غير مصرح بهذه العملية"),

            // Anything else is genuinely unexpected: return a generic message so
            // internal details never reach the client.
            _ => (
                StatusCodes.Status500InternalServerError,
                ApiErrorCodes.ServerError,
                "حدث خطأ غير متوقع. تم تسجيل المشكلة وسيتم معالجتها.")
        };
    }
}
