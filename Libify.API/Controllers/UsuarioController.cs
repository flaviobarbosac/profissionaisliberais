using AutoMapper;
using Libify.API.Dto;
using Libify.Domain.Model;
using Libify.Domain.Ports;
using Libify.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Libify.API.Controllers
{
    /// <summary>
    /// Leitura + atualização limitada do perfil do usuário autenticado (tenant raiz).
    /// Criação/remoção continua via fluxo de autenticação.
    /// </summary>
    public class UsuarioController : ReadOnlyController<Usuario, UsuarioDto>
    {
        private readonly ITenantContext _tenant;

        public UsuarioController(
            IBaseServices<Usuario> services,
            IMapper mapper,
            ITenantContext tenant) : base(services, mapper)
        {
            _tenant = tenant;
        }

        /// <summary>Atualiza o perfil do usuário logado (somente o próprio registro).</summary>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UsuarioDto>> Put(Guid id, [FromBody] UsuarioDto dados, CancellationToken ct)
        {
            if (!_tenant.UsuarioId.HasValue || _tenant.UsuarioId.Value != id)
                return Forbid();

            var existente = await Services.GetByIdAsync(id);
            if (existente is null)
                return NotFound();

            existente.Nome = string.IsNullOrWhiteSpace(dados.Nome) ? existente.Nome : dados.Nome.Trim();
            existente.Email = dados.Email?.Trim();
            existente.Telefone = dados.Telefone?.Trim();
            existente.CpfCnpj = dados.CpfCnpj?.Trim();
            existente.Categoria = dados.Categoria;
            existente.Plano = dados.Plano;

            if (!string.IsNullOrWhiteSpace(dados.Locale))
                existente.Locale = dados.Locale.Trim();
            if (!string.IsNullOrWhiteSpace(dados.Pais))
                existente.Pais = dados.Pais.Trim();

            await Services.UpdateAsync(existente);
            return Ok(Mapper.Map<UsuarioDto>(existente));
        }
    }
}
