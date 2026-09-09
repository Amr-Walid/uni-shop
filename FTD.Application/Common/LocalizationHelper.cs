using System;

namespace FTD.Application.Common
{
    /// <summary>
    /// Picks the right side of the project's paired Ar/En columns.
    ///
    /// The whole schema stores both languages side by side (NameAr/NameEn,
    /// DescAr/DescEn, ...). Sending both to a mobile client would inflate every
    /// payload by ~40% and force language branching into every widget, so the
    /// API resolves one language per request from the Accept-Language header and
    /// returns a single flat field.
    /// </summary>
    public static class LocalizationHelper
    {
        public const string Arabic = "ar";
        public const string English = "en";

        /// <summary>
        /// Normalizes an Accept-Language value to a supported code.
        /// Accepts full tags and quality lists ("en-US,en;q=0.9") and falls back
        /// to Arabic, the storefront's primary language.
        /// </summary>
        public static string NormalizeLanguage(string? language)
        {
            if (string.IsNullOrWhiteSpace(language)) return Arabic;

            // Take the highest-priority tag, then its primary subtag.
            var primary = language.Split(',')[0].Split(';')[0].Trim();
            if (primary.Length >= 2 &&
                primary.Substring(0, 2).Equals(English, StringComparison.OrdinalIgnoreCase))
            {
                return English;
            }

            return Arabic;
        }

        /// <summary>
        /// Returns the value for the requested language, falling back to the
        /// other one when it is blank.
        ///
        /// The fallback matters in practice: admins routinely fill Arabic and
        /// leave English empty. Without it, English clients would render blank
        /// product names — worse than showing Arabic text.
        /// </summary>
        public static string Pick(string? arabic, string? english, string lang)
        {
            var wantsEnglish = lang == English;

            var preferred = wantsEnglish ? english : arabic;
            if (!string.IsNullOrWhiteSpace(preferred)) return preferred.Trim();

            var alternate = wantsEnglish ? arabic : english;
            return string.IsNullOrWhiteSpace(alternate) ? string.Empty : alternate.Trim();
        }

        /// <summary>
        /// Nullable variant for genuinely optional text (short descriptions,
        /// notes): returns null instead of "" so clients can hide the element
        /// rather than render an empty line.
        /// </summary>
        public static string? PickOptional(string? arabic, string? english, string lang)
        {
            var value = Pick(arabic, english, lang);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
