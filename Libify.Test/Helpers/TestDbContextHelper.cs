using Microsoft.EntityFrameworkCore;
using Libify.Infraestructure.Database;

namespace Libify.Test.Helpers
{
    public static class TestDbContextHelper
    {
        public static AppDbContext CreateInMemory()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }
    }
}
