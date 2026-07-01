using Libify.Domain.Enum;
using Libify.Domain.Helpers;
using Libify.Domain.Messaging;
using Libify.Domain.Model.Base;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Microsoft.EntityFrameworkCore;
using PropostaEntity = Libify.Domain.Model.Proposta;
using PropostaItemEntity = Libify.Domain.Model.PropostaItem;

namespace Libify.Services.Proposta
{
  public interface IPropostaAppService
  {
    Task<(IEnumerable<PropostaEntity> Items, int Total)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<PropostaEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PropostaEntity> CreateAsync(PropostaEntity proposta, IEnumerable<PropostaItemEntity> itens, CancellationToken cancellationToken = default);
    Task<PropostaEntity> UpdateAsync(PropostaEntity proposta, IEnumerable<PropostaItemEntity> itens, CancellationToken cancellationToken = default);
  }

  /// <summary>
  /// Persistência de proposta + itens (filhos tenant-owned) em transação única.
  /// </summary>
  public class PropostaAppService : IPropostaAppService
  {
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IMessageBus _bus;

    public PropostaAppService(AppDbContext db, ITenantContext tenant, IMessageBus bus)
    {
      _db = db;
      _tenant = tenant;
      _bus = bus;
    }

    public async Task<(IEnumerable<PropostaEntity> Items, int Total)> GetPagedAsync(
      int page,
      int pageSize,
      CancellationToken cancellationToken = default)
    {
      var query = _db.Proposta
        .Include(p => p.Itens.Where(i => i.DeletedAt == null))
        .AsQueryable();

      var total = await query.CountAsync(cancellationToken);
      var items = await query
        .OrderByDescending(p => p.CreatedAt)
        .Skip((Math.Max(page, 1) - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

      return (items, total);
    }

    public async Task<PropostaEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
      return await _db.Proposta
        .Include(p => p.Itens.Where(i => i.DeletedAt == null))
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PropostaEntity> CreateAsync(
      PropostaEntity proposta,
      IEnumerable<PropostaItemEntity> itens,
      CancellationToken cancellationToken = default)
    {
      var usuarioId = RequireUsuarioId();
      var agora = DateTimeHelper.UtcNow;

      if (proposta.Id == Guid.Empty)
        proposta.Id = Guid.CreateVersion7();

      proposta.UsuarioId = usuarioId;
      proposta.CreatedAt = agora;
      proposta.UpdatedAt = agora;
      proposta.Version = 1;

      var itensList = NormalizarItens(itens, proposta.Id, usuarioId, agora).ToList();
      proposta.Itens = itensList;

      _db.Proposta.Add(proposta);
      await PublicarAsync(proposta, SyncOperacao.Upsert, cancellationToken);

      foreach (var item in itensList)
      {
        await PublicarAsync(item, SyncOperacao.Upsert, cancellationToken);
      }

      await _db.SaveChangesAsync(cancellationToken);
      return proposta;
    }

    public async Task<PropostaEntity> UpdateAsync(
      PropostaEntity proposta,
      IEnumerable<PropostaItemEntity> itens,
      CancellationToken cancellationToken = default)
    {
      var usuarioId = RequireUsuarioId();
      var agora = DateTimeHelper.UtcNow;

      var existente = await _db.Proposta
        .Include(p => p.Itens)
        .FirstOrDefaultAsync(p => p.Id == proposta.Id, cancellationToken)
        ?? throw new InvalidOperationException("Proposta não encontrada.");

      existente.ClienteId = proposta.ClienteId;
      existente.Titulo = proposta.Titulo;
      existente.Status = proposta.Status;
      existente.ValorTotal = proposta.ValorTotal;
      existente.Observacoes = proposta.Observacoes;
      existente.UpdatedAt = agora;
      existente.Version += 1;

      var novosItens = NormalizarItens(itens, existente.Id, usuarioId, agora).ToList();
      var idsNovos = novosItens.Select(i => i.Id).ToHashSet();

      foreach (var antigo in existente.Itens.Where(i => i.DeletedAt == null).ToList())
      {
        if (!idsNovos.Contains(antigo.Id))
        {
          antigo.DeletedAt = agora;
          antigo.UpdatedAt = agora;
          antigo.Version += 1;
          await PublicarAsync(antigo, SyncOperacao.Upsert, cancellationToken);
        }
      }

      foreach (var item in novosItens)
      {
        var atual = existente.Itens.FirstOrDefault(i => i.Id == item.Id);
        if (atual is null)
        {
          existente.Itens.Add(item);
          await PublicarAsync(item, SyncOperacao.Upsert, cancellationToken);
        }
        else
        {
          atual.ServicoId = item.ServicoId;
          atual.Descricao = item.Descricao;
          atual.Quantidade = item.Quantidade;
          atual.ValorUnitario = item.ValorUnitario;
          atual.Total = item.Total;
          atual.UpdatedAt = agora;
          atual.Version += 1;
          await PublicarAsync(atual, SyncOperacao.Upsert, cancellationToken);
        }
      }

      await PublicarAsync(existente, SyncOperacao.Upsert, cancellationToken);
      await _db.SaveChangesAsync(cancellationToken);

      existente.Itens = existente.Itens.Where(i => i.DeletedAt == null).ToList();
      return existente;
    }

    private Guid RequireUsuarioId()
    {
      if (!_tenant.UsuarioId.HasValue)
        throw new UnauthorizedAccessException("Usuário não autenticado.");
      return _tenant.UsuarioId.Value;
    }

    private static IEnumerable<PropostaItemEntity> NormalizarItens(
      IEnumerable<PropostaItemEntity> itens,
      Guid propostaId,
      Guid usuarioId,
      DateTime agora)
    {
      return itens.Select(i =>
      {
        var id = i.Id == Guid.Empty ? Guid.CreateVersion7() : i.Id;
        return new PropostaItemEntity
        {
          Id = id,
          PropostaId = propostaId,
          UsuarioId = usuarioId,
          ServicoId = i.ServicoId,
          Descricao = i.Descricao,
          Quantidade = i.Quantidade,
          ValorUnitario = i.ValorUnitario,
          Total = i.Total,
          CreatedAt = agora,
          UpdatedAt = agora,
          Version = 1
        };
      });
    }

    private async Task PublicarAsync<T>(T entity, SyncOperacao operacao, CancellationToken ct)
      where T : ModelBase, ITenantOwned
    {
      await _bus.PublishAsync(new SyncEvent<T>
      {
        EntityId = entity.Id,
        UsuarioId = entity.UsuarioId,
        Version = entity.Version,
        Operacao = operacao,
        Payload = entity,
        OcorridoEm = DateTimeHelper.UtcNow
      }, ct);
    }
  }
}
