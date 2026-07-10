using System;
using System.Text.RegularExpressions;

namespace FTD.Application.Common
{
    public static class HtmlSanitizer
    {
        /// <summary>
        /// Simple sanitizer to prevent Stored XSS injection vectors (scripts, inline events, javascript links)
        /// while retaining safe HTML structural elements (headings, paragraphs, tables, lists, styles, etc.)
        /// </summary>
        public static string Sanitize(string? html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Strip out <script>...</script> blocks
            var result = Regex.Replace(html, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Strip out inline script event handlers (e.g., onload, onerror, onclick, etc.)
            result = Regex.Replace(result, @"\s*on\w+\s*=\s*(?:""[^""]*""|'[^']*'|[^\s>]+)", "", RegexOptions.IgnoreCase);

            // Strip out javascript: protocol links
            result = Regex.Replace(result, @"href\s*=\s*(?:""\s*javascript:[^""]*""|'\s*javascript:[^']*'|javascript:[^\s>]+)", "", RegexOptions.IgnoreCase);

            return result;
        }
    }
}
