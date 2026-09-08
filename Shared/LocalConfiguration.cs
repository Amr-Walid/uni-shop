using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Hosting;

namespace FTD.Hosting
{
    /// <summary>
    /// Registers the optional, gitignored <c>appsettings.Local.json</c> used for
    /// developer-machine overrides — for example running against a persistent
    /// Sqlite file instead of SQL Server.
    ///
    /// This lives in Shared/ and is compiled into both FTD.Web and FTD.Api
    /// (rather than FTD.Infrastructure) because it is a host-startup concern and
    /// needs the hosting/JSON-configuration packages that a data-layer class
    /// library deliberately does not reference.
    /// </summary>
    public static class LocalConfiguration
    {
        public const string FileName = "appsettings.Local.json";

        /// <summary>
        /// Inserts the local file immediately after the last JSON source, so it
        /// overrides <c>appsettings.json</c> and
        /// <c>appsettings.{Environment}.json</c> but is still overridden by
        /// environment variables, user secrets and command-line arguments.
        /// </summary>
        /// <remarks>
        /// Appending instead — what a plain <c>AddJsonFile</c> call does — would
        /// make this file outrank environment variables. A developer's leftover
        /// <c>appsettings.Local.json</c> could then silently override deployment
        /// configuration, the precise failure this file must never cause.
        ///
        /// Loaded in Development only, so it cannot influence a deployed
        /// environment even if it is accidentally copied onto a server.
        /// </remarks>
        public static void Insert(IConfigurationBuilder configuration, IHostEnvironment environment)
        {
            if (!environment.IsDevelopment()) return;

            var source = new JsonConfigurationSource
            {
                Path = FileName,
                Optional = true,
                ReloadOnChange = true
            };

            // Resolve against the same file provider the standard appsettings
            // files use; otherwise the path is interpreted relative to the
            // process working directory rather than the content root.
            source.ResolveFileProvider();

            var lastJsonIndex = -1;
            for (var i = 0; i < configuration.Sources.Count; i++)
            {
                if (configuration.Sources[i] is JsonConfigurationSource) lastJsonIndex = i;
            }

            if (lastJsonIndex < 0)
                configuration.Sources.Add(source);
            else
                configuration.Sources.Insert(lastJsonIndex + 1, source);
        }
    }
}
