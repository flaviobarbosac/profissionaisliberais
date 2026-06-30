using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Libify.Infraestructure.Database;

namespace Libify.Infraestructure
{
    /// <summary>
    /// Factory de design-time usada pelo EF Core Tools (migrations).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("PROFLIB_CONNECTION")
                ?? "Host=localhost;Port=5432;Database=libify;Username=postgres;Password=postgres;";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
