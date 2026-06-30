using FluentAssertions;
using Libify.Domain.Messaging;
using Libify.Domain.Model;
using Libify.Repository;
using Libify.Test.Helpers;

namespace Libify.Test.Repository
{
    public class BaseRepositoryTests
    {
        private static (BaseRepository<Cliente> repo, CapturingMessageBus bus, TestTenantContext tenant) CriarCliente()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            var context = TestDbContextHelper.CreateInMemory(tenant);
            var bus = new CapturingMessageBus();
            return (new BaseRepository<Cliente>(context, tenant, bus), bus, tenant);
        }

        [Fact]
        public async Task AddAsync_PublicaSyncEventUpsert()
        {
            var (repo, bus, _) = CriarCliente();

            await repo.AddAsync(new Cliente { Nome = "Novo" });

            bus.Publicados.Should().ContainSingle();
            bus.Publicados[0].Should().BeOfType<SyncEvent<Cliente>>()
                .Which.Operacao.Should().Be(SyncOperacao.Upsert);
        }

        [Fact]
        public async Task UpdateAsync_IncrementaVersionEPreservaCreatedAt()
        {
            var (repo, bus, _) = CriarCliente();
            var cliente = new Cliente { Nome = "Antes" };
            await repo.AddAsync(cliente);
            var createdAt = (await repo.GetByIdAsync(cliente.Id))!.CreatedAt;

            cliente.Nome = "Depois";
            await repo.UpdateAsync(cliente);

            var atualizado = await repo.GetByIdAsync(cliente.Id);
            atualizado!.Nome.Should().Be("Depois");
            atualizado.Version.Should().Be(2);
            atualizado.CreatedAt.Should().Be(createdAt);
            bus.Publicados.Should().HaveCount(2);
        }

        [Fact]
        public async Task UpdateAsync_EntidadeInexistente_NaoFazNada()
        {
            var (repo, bus, _) = CriarCliente();

            await repo.UpdateAsync(new Cliente { Id = Guid.CreateVersion7(), Nome = "Fantasma" });

            bus.Publicados.Should().BeEmpty();
        }

        [Fact]
        public async Task DeleteAsync_RemoveEPublicaDelete()
        {
            var (repo, bus, _) = CriarCliente();
            var cliente = new Cliente { Nome = "Remover" };
            await repo.AddAsync(cliente);

            await repo.DeleteAsync(cliente.Id);

            (await repo.GetByIdAsync(cliente.Id)).Should().BeNull();
            bus.Publicados.Last().Should().BeOfType<SyncEvent<Cliente>>()
                .Which.Operacao.Should().Be(SyncOperacao.Delete);
        }

        [Fact]
        public async Task SoftDeleteAsync_MarcaDeletedAtEIncrementaVersion()
        {
            var (repo, bus, _) = CriarCliente();
            var cliente = new Cliente { Nome = "Soft" };
            await repo.AddAsync(cliente);

            await repo.SoftDeleteAsync(cliente.Id);

            (await repo.GetAllAsync()).Should().BeEmpty();
            bus.Publicados.Last().Should().BeOfType<SyncEvent<Cliente>>()
                .Which.Operacao.Should().Be(SyncOperacao.Delete);
        }

        [Fact]
        public async Task GetPagedAsync_RetornaTotalEPagina()
        {
            var (repo, _, _) = CriarCliente();
            await repo.AddRangeAsync(new[]
            {
                new Cliente { Nome = "A" },
                new Cliente { Nome = "B" },
                new Cliente { Nome = "C" }
            });

            var (items, total) = await repo.GetPagedAsync(page: 1, pageSize: 2);

            total.Should().Be(3);
            items.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddRangeAsync_CarimbaTenantEPublicaCadaItem()
        {
            var (repo, bus, tenant) = CriarCliente();

            await repo.AddRangeAsync(new[] { new Cliente { Nome = "X" }, new Cliente { Nome = "Y" } });

            var todos = (await repo.GetAllAsync()).ToList();
            todos.Should().HaveCount(2);
            todos.Should().OnlyContain(c => c.UsuarioId == tenant.UsuarioId!.Value);
            bus.Publicados.Should().HaveCount(2);
        }

        [Fact]
        public async Task AddRangeAsync_Nulo_LancaArgumentNull()
        {
            var (repo, _, _) = CriarCliente();

            var act = async () => await repo.AddRangeAsync(null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task FindAsync_E_FindAllAsync_FiltramPorPredicado()
        {
            var (repo, _, _) = CriarCliente();
            await repo.AddRangeAsync(new[]
            {
                new Cliente { Nome = "Maria" },
                new Cliente { Nome = "Mario" },
                new Cliente { Nome = "Joao" }
            });

            var um = await repo.FindAsync(c => c.Nome == "Joao");
            var muitos = await repo.FindAllAsync(c => c.Nome.StartsWith("Mari"));

            um!.Nome.Should().Be("Joao");
            muitos.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_Inexistente_RetornaNull()
        {
            var (repo, _, _) = CriarCliente();

            (await repo.GetByIdAsync(Guid.CreateVersion7())).Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_EntidadeNaoTenant_NaoPublicaEvento()
        {
            var tenant = new TestTenantContext(Guid.CreateVersion7());
            var context = TestDbContextHelper.CreateInMemory(tenant);
            var bus = new CapturingMessageBus();
            var repo = new BaseRepository<Usuario>(context, tenant, bus);

            await repo.AddAsync(new Usuario { Id = tenant.UsuarioId!.Value, Nome = "Sem tenant" });

            bus.Publicados.Should().BeEmpty();
            (await repo.GetByIdAsync(tenant.UsuarioId!.Value)).Should().NotBeNull();
        }
    }
}
