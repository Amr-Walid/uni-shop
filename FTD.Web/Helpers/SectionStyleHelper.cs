using System.Text.Json;

namespace FTD.Web.Helpers
{
    /// <summary>
    /// Shared appearance options for page-builder sections.
    /// Parsed from the "style" object inside the section's ContentJson:
    ///   { "style": { "bg":"light|dark|brand|none", "pad":"sm|md|lg", "align":"start|center|end" } }
    /// </summary>
    public class SectionStyle
    {
        public string Bg { get; set; } = "none";
        public string Pad { get; set; } = "md";
        public string Align { get; set; } = "start";

        public static SectionStyle Parse(string? contentJson)
        {
            var s = new SectionStyle();
            if (string.IsNullOrEmpty(contentJson)) return s;
            try
            {
                var doc = JsonDocument.Parse(contentJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                    doc.RootElement.TryGetProperty("style", out var st) &&
                    st.ValueKind == JsonValueKind.Object)
                {
                    if (st.TryGetProperty("bg", out var bg) && !string.IsNullOrEmpty(bg.GetString())) s.Bg = bg.GetString()!;
                    if (st.TryGetProperty("pad", out var pad) && !string.IsNullOrEmpty(pad.GetString())) s.Pad = pad.GetString()!;
                    if (st.TryGetProperty("align", out var al) && !string.IsNullOrEmpty(al.GetString())) s.Align = al.GetString()!;
                }
            }
            catch { /* legacy / malformed JSON — defaults apply */ }
            return s;
        }

        /// <summary>Inline CSS for the section wrapper (works in RTL & LTR).</summary>
        public string WrapperCss()
        {
            var css = "";

            css += Bg switch
            {
                "light" => "background:var(--bg,#f5f7fa);border-radius:20px;",
                "dark" => "background:var(--dark,#0d1b2a);border-radius:20px;color:#fff;",
                "brand" => "background:var(--blue-light,#e8f0ff);border-radius:20px;",
                _ => ""
            };

            css += Pad switch
            {
                "sm" => "padding:1rem 1.2rem;",
                "lg" => "padding:3.5rem 2.5rem;",
                _ => Bg == "none" ? "" : "padding:2rem 1.8rem;"
            };

            css += Align switch
            {
                "center" => "text-align:center;",
                "end" => "text-align:left;", // RTL site: "end" = left
                _ => ""
            };

            return css;
        }

        /// <summary>True when the dark background is active (partials can invert text colors).</summary>
        public bool IsDark => Bg == "dark";
    }

    /// <summary>Small JSON read helpers used by the section partials.</summary>
    public static class SectionJson
    {
        public static string Str(JsonElement el, string prop)
            => el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
               ? v.GetString() ?? "" : "";

        public static int Int(JsonElement el, string prop, int fallback = 0)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var v))
            {
                if (v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n)) return n;
                if (v.ValueKind == JsonValueKind.String && int.TryParse(v.GetString(), out var s)) return s;
            }
            return fallback;
        }

        /// <summary>
        /// Returns list items whether the JSON is the legacy bare array [...]
        /// or the new wrapped form {"items":[...], "style":{...}}.
        /// </summary>
        public static List<JsonElement> Items(string? contentJson)
        {
            var list = new List<JsonElement>();
            if (string.IsNullOrEmpty(contentJson)) return list;
            try
            {
                var doc = JsonDocument.Parse(contentJson);
                var root = doc.RootElement;
                JsonElement arr;
                if (root.ValueKind == JsonValueKind.Array) arr = root;
                else if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("items", out var it) && it.ValueKind == JsonValueKind.Array) arr = it;
                else return list;

                foreach (var el in arr.EnumerateArray()) list.Add(el.Clone());
            }
            catch { }
            return list;
        }

        /// <summary>Parses the root object (or returns default) for object-shaped sections.</summary>
        public static JsonElement? Root(string? contentJson)
        {
            if (string.IsNullOrEmpty(contentJson)) return null;
            try
            {
                var doc = JsonDocument.Parse(contentJson);
                if (doc.RootElement.ValueKind == JsonValueKind.Object) return doc.RootElement.Clone();
            }
            catch { }
            return null;
        }

        /// <summary>
        /// Converts a YouTube / Vimeo / direct video URL into an embeddable form.
        /// Returns (embedUrl, isDirectFile).
        /// </summary>
        public static (string url, bool isFile) VideoEmbed(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return ("", false);
            raw = raw.Trim();

            // YouTube: watch?v=, youtu.be/, shorts/
            var yt = System.Text.RegularExpressions.Regex.Match(raw,
                @"(?:youtube\.com/(?:watch\?v=|shorts/|embed/)|youtu\.be/)([A-Za-z0-9_-]{6,20})");
            if (yt.Success) return ($"https://www.youtube.com/embed/{yt.Groups[1].Value}", false);

            // Vimeo
            var vm = System.Text.RegularExpressions.Regex.Match(raw, @"vimeo\.com/(?:video/)?(\d+)");
            if (vm.Success) return ($"https://player.vimeo.com/video/{vm.Groups[1].Value}", false);

            // Direct file (mp4/webm) — including uploaded paths
            if (raw.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                raw.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
                return (raw, true);

            return (raw, false);
        }
    }
}
