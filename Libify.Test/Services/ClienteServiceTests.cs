using FluentAssertions;
using Libify.Domain.Model;
using Libify.Repository;
using Libify.Services;
using Libify.Test.Helpers;

namespace Libify.Test.Services
{
    public class ClienteServiceTests
    {
        [Fact]
        public async Task AddAsync_DevePersistirComDatasETenant()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            using var context = TestDbContextHelper.CreateInMemory(tenant);
            var repository = new BaseRepository<Cliente>(context, tenant, new NullMessageBus());
            var service = new BaseServices<Cliente>(repository);

            var cliente = new Cliente { Nome = "Cliente Teste" };
            await service.AddAsync(cliente);

            var persistido = await service.GetByIdAsync(cliente.Id);
            persistido.Should().NotBeNull();
            persistido!.Nome.Should().Be("Cliente Teste");
            persistido.CreatedAt.Should().NotBe(default);
            persistido.UsuarioId.Should().Be(tenant.UsuarioId!.Value);
            persistido.Version.Should().Be(1);
        }

        [Fact]
        public async Task SoftDeleteAsync_DeveExcluirLogicamente()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            using var context = TestDbContextHelper.CreateInMemory(tenant);
            var repository = new BaseRepository<Cliente>(context, tenant, new NullMessageBus());
            var service = new BaseServices<Cliente>(repository);

            var cliente = new Cliente { Nome = "Para Excluir" };
            await service.AddAsync(cliente);

            await service.SoftDeleteAsync(cliente.Id);

            var todos = await service.GetAllAsync();
            todos.Should().BeEmpty();
        }

        [Fact]
        public async Task UpdateEDelete_DelegamParaRepositorio()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            using var context = TestDbContextHelper.CreateInMemory(tenant);
            var service = new BaseServices<Cliente>(
                new BaseRepository<Cliente>(context, tenant, new NullMessageBus()));

            var cliente = new Cliente { Nome = "Original" };
            await service.AddAsync(cliente);

            cliente.Nome = "Editado";
            await service.UpdateAsync(cliente);
            (await service.GetByIdAsync(cliente.Id))!.Nome.Should().Be("Editado");

            await service.DeleteAsync(cliente.Id);
            (await service.GetByIdAsync(cliente.Id)).Should().BeNull();
        }

        [Fact]
        public async Task GetPagedAsync_DelegaParaRepositorio()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            using var context = TestDbContextHelper.CreateInMemory(tenant);
            var service = new BaseServices<Cliente>(
                new BaseRepository<Cliente>(context, tenant, new NullMessageBus()));

            await service.AddAsync(new Cliente { Nome = "A" });
            await service.AddAsync(new Cliente { Nome = "B" });

            var (items, total) = await service.GetPagedAsync(1, 10);

            total.Should().Be(2);
            items.Should().HaveCount(2);
        }
    }
}
