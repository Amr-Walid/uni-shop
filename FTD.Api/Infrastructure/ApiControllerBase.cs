using System.Security.Claims;
using FTD.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FTD.Api.Infrastructure
{
    /// <summary>
    /// Shared plumbing for every API controller: request language resolution,
    /// the authenticated user id, the media base URL and problem responses.
    ///
    /// Centralising these removes the temptation to re-derive them per
    /// controller — in particular the user id, which must ALWAYS come from the
    /// validated token and never from a route or body parameter.
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Language for this request, from the <c>Accept-Language</c> header.
        /// Defaults to Arabic, the storefront's primary language.
        /// </summary>
        protected string RequestLanguage
        {
            get
            {
                var header = Request.Headers.AcceptLanguage.ToString();
                return LocalizationHelper.NormalizeLanguage(header);
            }
        }

        /// <summary>
        /// Absolute base URL for media, from <c>Media:BaseUrl</c>.
        ///
        /// Falls back to this request's own scheme+host, which is what makes the
        /// API work out of the box in development while still supporting a CDN
        /// or a separate web host in production.
        /// </summary>
        protected string? MediaBaseUrl
        {
            get
            {
                var configured = HttpContext.RequestServices
                    .GetService(typeof(IConfiguration)) as IConfiguration;

                var fromConfig = configured?["Media:BaseUrl"];
                if (!string.IsNullOrWhiteSpace(fromConfig)) return fromConfig.TrimEnd('/');

                return $"{Request.Scheme}://{Request.Host}";
            }
        }

        /// <summary>
        /// Id of the authenticated user, or null for an anonymous request.
        /// Read from the JWT's NameIdentifier claim — the only trustworthy
        /// source. Accepting a user id from the request would be an IDOR.
        /// </summary>
        protected string? CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        protected bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUserId);

        protected bool IsAdmin => User.IsInRole("Admin");

        /// <summary>Client IP, used for refresh-token audit records.</summary>
        protected string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        // ── Problem helpers ───────────────────────────────────────────────────

        protected ObjectResult Problem(int statusCode, string errorCode, string detail)
            => ApiProblemFactory.Create(HttpContext, statusCode, errorCode, detail);

        protected ObjectResult ProblemFromCode(string errorCode, string detail)
            => ApiProblemFactory.FromErrorCode(HttpContext, errorCode, detail);

        protected ObjectResult BadRequestProblem(string detail, string? code = null)
            => ApiProblemFactory.BadRequest(HttpContext, detail, code);

        protected ObjectResult NotFoundProblem(string detail, string? code = null)
            => ApiProblemFactory.NotFound(HttpContext, detail, code);

        protected ObjectResult UnauthorizedProblem(string detail, string? code = null)
            => ApiProblemFactory.Unauthorized(HttpContext, detail, code);

        protected ObjectResult ForbiddenProblem(string detail, string? code = null)
            => ApiProblemFactory.Forbidden(HttpContext, detail, code);

        protected ObjectResult ConflictProblem(string detail, string? code = null)
            => ApiProblemFactory.Conflict(HttpContext, detail, code);
    }
}
