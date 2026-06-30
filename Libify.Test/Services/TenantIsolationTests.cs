using FluentAssertions;
using Libify.Domain.Model;
using Libify.Repository;
using Libify.Services;
using Libify.Test.Helpers;

namespace Libify.Test.Services
{
    public class TenantIsolationTests
    {
        [Fact]
        public async Task CadaTenant_VeSomenteOsProprios_Dados()
        {
            var db = Guid.NewGuid().ToString();
            var tenant1 = Guid.CreateVersion7();
            var tenant2 = Guid.CreateVersion7();

            await AdicionarCliente(db, tenant1, "Cliente do Tenant 1");
            await AdicionarCliente(db, tenant2, "Cliente do Tenant 2");

            var tc = new TestTenantContext(tenant1);
            using var context = TestDbContextHelper.CreateInMemory(tc, db);
            var service = new BaseServices<Cliente>(
                new BaseRepository<Cliente>(context, tc, new NullMessageBus()));

            var todos = (await service.GetAllAsync()).ToList();

            todos.Should().ContainSingle();
            todos[0].Nome.Should().Be("Cliente do Tenant 1");
        }

        private static async Task AdicionarCliente(string db, Guid tenantId, string nome)
        {
            var tc = new TestTenantContext(tenantId);
            using var context = TestDbContextHelper.CreateInMemory(tc, db);
            var service = new BaseServices<Cliente>(
                new BaseRepository<Cliente>(context, tc, new NullMessageBus()));
            await service.AddAsync(new Cliente { Nome = nome });
        }
    }
}
