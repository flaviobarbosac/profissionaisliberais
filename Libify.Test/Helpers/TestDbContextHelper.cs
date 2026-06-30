using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Libify.Test.Helpers
{
    public static class TestDbContextHelper
    {
        public static AppDbContext CreateInMemory(ITenantContext? tenant = null, string? dbName = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options, tenant ?? new TestTenantContext(Guid.CreateVersion7()));
        }
    }
}
