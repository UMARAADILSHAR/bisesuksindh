using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BiseSukkur.Infrastructure.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        string? connectionString = null;

        // 1. Check if passed as command line argument or --connection
        if (args != null && args.Length > 0)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("--connection", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                {
                    connectionString = args[i + 1];
                    break;
                }
                else if (!args[i].StartsWith("--") && args[i].Contains("Server=", StringComparison.OrdinalIgnoreCase))
                {
                    connectionString = args[i];
                    break;
                }
            }
        }

        // 2. Check environment variable
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
        }

        // 3. Check Web project's appsettings.Production.json or appsettings.json
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                var candidateDirs = new[]
                {
                    Directory.GetCurrentDirectory(),
                    Path.Combine(Directory.GetCurrentDirectory(), "src", "BiseSukkur.Web"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "BiseSukkur.Web")
                };

                foreach (var dir in candidateDirs)
                {
                    var prodPath = Path.Combine(dir, "appsettings.Production.json");
                    var devPath = Path.Combine(dir, "appsettings.json");

                    if (File.Exists(prodPath) || File.Exists(devPath))
                    {
                        var config = new ConfigurationBuilder()
                            .SetBasePath(dir)
                            .AddJsonFile("appsettings.json", optional: true)
                            .AddJsonFile("appsettings.Production.json", optional: true)
                            .AddEnvironmentVariables()
                            .Build();

                        var cfgConn = config.GetConnectionString("DefaultConnection");
                        if (!string.IsNullOrWhiteSpace(cfgConn) && !cfgConn.Contains("YOUR_SMARTERASP_SQL_SERVER"))
                        {
                            connectionString = cfgConn;
                            break;
                        }
                    }
                }
            }
            catch
            {
                // Fall back to default
            }
        }

        // 4. Default fallback to localdb
        if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_SMARTERASP_SQL_SERVER"))
        {
            connectionString = "Server=(localdb)\\mssqllocaldb;Database=BiseSukkurDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
