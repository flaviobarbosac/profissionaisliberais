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
        public async Task AddAsync_DevePersistirClienteComDatas()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var repository = new BaseRepository<Cliente>(context);
            var service = new BaseServices<Cliente>(repository);

            var cliente = new Cliente { UsuarioId = 1, Nome = "Cliente Teste" };

            await service.AddAsync(cliente);

            var persistido = await service.GetByIdAsync(cliente.Id);
            persistido.Should().NotBeNull();
            persistido!.Nome.Should().Be("Cliente Teste");
            persistido.CreatedAt.Should().NotBe(default);
        }

        [Fact]
        public async Task SoftDeleteAsync_DeveExcluirLogicamente()
        {
            using var context = TestDbContextHelper.CreateInMemory();
            var repository = new BaseRepository<Cliente>(context);
            var service = new BaseServices<Cliente>(repository);

            var cliente = new Cliente { UsuarioId = 1, Nome = "Para Excluir" };
            await service.AddAsync(cliente);

            await service.SoftDeleteAsync(cliente.Id);

            var todos = await service.GetAllAsync();
            todos.Should().BeEmpty();
        }
    }
}
