using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FTD.Infrastructure.Data
{
    /// <summary>
    /// Single place where the EF Core provider is chosen, shared by FTD.Web and
    /// FTD.Api so the two hosts cannot drift apart on which database they use.
    ///
    /// The provider is read from <c>Database:Provider</c> and defaults to
    /// <b>SqlServer</b>. That default is deliberate: a fresh clone of this
    /// repository — with no environment variables and no untracked local files —
    /// must behave exactly as it did before local development providers existed.
    /// Only an explicit opt-in changes it.
    /// </summary>
    public static class DatabaseProviderSetup
    {
        public const string SqlServer = "SqlServer";
        public const string Sqlite = "Sqlite";
        public const string InMemory = "InMemory";

        /// <summary>
        /// The provider name in effect, defaulting to SqlServer when unset.
        /// </summary>
        public static string ResolveProviderName(IConfiguration configuration)
        {
            var provider = configuration["Database:Provider"];
            return string.IsNullOrWhiteSpace(provider) ? SqlServer : provider.Trim();
        }

        public static bool IsSqlite(string provider) =>
            string.Equals(provider, Sqlite, StringComparison.OrdinalIgnoreCase);

        public static bool IsInMemory(string provider) =>
            string.Equals(provider, InMemory, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Registers <see cref="AppDbContext"/> against the configured provider.
        /// </summary>
        /// <remarks>
        /// Sqlite and InMemory are refused outside Development. Sqlite writes to
        /// a local file that no deployment pipeline manages, and InMemory does
        /// not persist at all and ignores relational constraints — so silently
        /// accepting either in Production would be a data-loss bug rather than a
        /// convenience. Failing loudly at startup is the safer behaviour.
        /// </remarks>
        /// <param name="isDevelopment">
        /// Passed in as a plain flag rather than taking IHostEnvironment, so this
        /// data-layer project does not need a dependency on the hosting stack.
        /// </param>
        public static string AddAppDbContext(
            IServiceCollection services,
            IConfiguration configuration,
            bool isDevelopment)
        {
            var provider = ResolveProviderName(configuration);

            if ((IsSqlite(provider) || IsInMemory(provider)) && !isDevelopment)
                throw new InvalidOperationException(
                    $"Database:Provider={provider} is only permitted in Development. " +
                    $"Use \"{SqlServer}\" and supply ConnectionStrings:DefaultConnection.");

            if (IsSqlite(provider))
            {
                // Default lives beside the solution rather than in the content
                // root so FTD.Web and FTD.Api share one database file, matching
                // the single SQL Server database they share in every other mode.
                var connectionString = configuration.GetConnectionString("SqliteConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                    connectionString = "Data Source=unishop.local.db";

                services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
            }
            else if (IsInMemory(provider))
            {
                // Shared store name so both hosts see the same seeded data when
                // they run side by side.
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("FTD_TechZone_Demo"));
            }
            else
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            }

            return provider;
        }

        /// <summary>
        /// Brings the schema up to date for the chosen provider.
        /// </summary>
        /// <remarks>
        /// Only SQL Server can run the migration history. The committed
        /// migrations are SQL-Server-specific — they declare
        /// <c>nvarchar(max)</c> columns, which is a syntax error in Sqlite, and
        /// the AddMobileApiSupport migration uses <c>GETUTCDATE()</c>, which
        /// Sqlite does not implement. So the relational-but-not-SQL-Server
        /// providers build the schema from the current model instead.
        ///
        /// EnsureCreated is also what materialises the <c>HasData</c> seeds, so
        /// it must run before any other write: an earlier write creates the
        /// database implicitly without seeds and leaves the catalogue
        /// permanently empty.
        /// </remarks>
        public static async Task InitializeSchemaAsync(AppDbContext db, string provider)
        {
            if (IsSqlite(provider) || IsInMemory(provider))
                await db.Database.EnsureCreatedAsync();
            else
                await db.Database.MigrateAsync();
        }
    }
}
