namespace Libify.Domain.Model.Base
{
    /// <summary>
    /// Marca entidades que pertencem a um tenant (usuário). Usado para o filtro
    /// global de isolamento de dados e para carimbar o UsuarioId em inserts.
    /// </summary>
    public interface ITenantOwned
    {
        Guid UsuarioId { get; set; }
    }
}
