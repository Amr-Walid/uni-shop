using System;

namespace FTD.Application.Common
{
    /// <summary>
    /// Turns the relative image paths stored in the database
    /// (e.g. "/images/products/x.jpg") into absolute URLs.
    ///
    /// WHY THIS IS REQUIRED: uploads live under FTD.Web's wwwroot, while the API
    /// may be served from a different host (api.example.com). A mobile app has
    /// no base URL to resolve a relative path against, so every image would fail
    /// to load. The API therefore prefixes paths with a configured
    /// <c>Media:BaseUrl</c>.
    /// </summary>
    public static class MediaUrlHelper
    {
        /// <summary>
        /// Builds an absolute URL for a stored media path.
        /// Returns null for blank input so callers can distinguish
        /// "no image" (render the emoji placeholder) from a broken link.
        /// Values that are already absolute (a CDN URL entered by the admin)
        /// are passed through untouched.
        /// </summary>
        public static string? ToAbsolute(string? relativePath, string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;

            var path = relativePath.Trim();

            // Already absolute (http/https) or a data URI — leave as-is.
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            // No base configured (e.g. local development): return the relative
            // path unchanged so the web front-end keeps working normally.
            if (string.IsNullOrWhiteSpace(baseUrl)) return path;

            return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }
    }
}
