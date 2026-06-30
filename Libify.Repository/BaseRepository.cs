using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Libify.Domain.Helpers;
using Libify.Domain.Messaging;
using Libify.Domain.Model.Base;
using Libify.Domain.Ports;
using Libify.Infraestructure.Database;
using Libify.Repository.Interface;

namespace Libify.Repository
{
    /// <summary>
    /// Repositório genérico tenant-aware: carimba UsuarioId em inserts, controla Version
    /// (LWW) e timestamps, respeita os filtros globais de tenant/soft-delete e publica
    /// eventos de sincronização na MESMA transação (outbox transacional).
    /// </summary>
    public class BaseRepository<T> : IBaseRepository<T> where T : ModelBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantContext _tenant;
        private readonly IMessageBus _bus;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext context, ITenantContext tenant, IMessageBus bus)
        {
            _context = context;
            _tenant = tenant;
            _bus = bus;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(int page, int pageSize)
        {
            var total = await _dbSet.CountAsync();
            var items = await _dbSet
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (items, total);
        }

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public async Task AddAsync(T entity)
        {
            CarimbarTenant(entity);
            if (entity.Id == Guid.Empty)
                entity.Id = Guid.CreateVersion7();
            entity.CreatedAt = DateTimeHelper.UtcNow;
            entity.UpdatedAt = entity.CreatedAt;
            entity.Version = 1;

            await _dbSet.AddAsync(entity);
            await PublicarAsync(entity, SyncOperacao.Upsert);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            var existing = await _dbSet.FirstOrDefaultAsync(e => e.Id == entity.Id);
            if (existing == null)
                return;

            var entry = _context.Entry(existing);
            var originalCreated = existing.CreatedAt;
            var originalVersion = existing.Version;
            var originalDeletedAt = existing.DeletedAt;
            var originalTenant = (existing as ITenantOwned)?.UsuarioId;

            entry.CurrentValues.SetValues(entity);

            existing.CreatedAt = originalCreated;
            // Soft-delete só muda via SoftDeleteAsync; o update genérico não exclui nem "ressuscita".
            existing.DeletedAt = originalDeletedAt;
            existing.Version = originalVersion + 1;
            existing.UpdatedAt = DateTimeHelper.UtcNow;

            if (existing is ITenantOwned owned && originalTenant.HasValue)
                owned.UsuarioId = originalTenant.Value;

            await PublicarAsync(existing, SyncOperacao.Upsert);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await PublicarDeleteAsync(id, entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null && entity.DeletedAt == null)
            {
                entity.DeletedAt = DateTimeHelper.UtcNow;
                entity.UpdatedAt = entity.DeletedAt.Value;
                entity.Version += 1;
                await PublicarDeleteAsync(id, entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            var lista = entities.ToList();
            foreach (var entity in lista)
            {
                CarimbarTenant(entity);
                if (entity.Id == Guid.Empty)
                    entity.Id = Guid.CreateVersion7();
                entity.CreatedAt = DateTimeHelper.UtcNow;
                entity.UpdatedAt = entity.CreatedAt;
                entity.Version = 1;
            }

            await _dbSet.AddRangeAsync(lista);
            foreach (var entity in lista)
                await PublicarAsync(entity, SyncOperacao.Upsert);
            await _context.SaveChangesAsync();
        }

        private void CarimbarTenant(T entity)
        {
            if (entity is ITenantOwned owned && _tenant.UsuarioId.HasValue && owned.UsuarioId == Guid.Empty)
                owned.UsuarioId = _tenant.UsuarioId.Value;
        }

        private Task PublicarAsync(T entity, SyncOperacao operacao)
        {
            if (entity is not ITenantOwned owned)
                return Task.CompletedTask;

            return _bus.PublishAsync(new SyncEvent<T>
            {
                EntityId = entity.Id,
                UsuarioId = _tenant.UsuarioId ?? owned.UsuarioId,
                Version = entity.Version,
                Operacao = operacao,
                Payload = entity,
                OcorridoEm = DateTimeHelper.UtcNow
            });
        }

        private Task PublicarDeleteAsync(Guid id, T entity)
        {
            if (entity is not ITenantOwned owned)
                return Task.CompletedTask;

            return _bus.PublishAsync(new SyncEvent<T>
            {
                EntityId = id,
                UsuarioId = _tenant.UsuarioId ?? owned.UsuarioId,
                Version = entity.Version,
                Operacao = SyncOperacao.Delete,
                OcorridoEm = DateTimeHelper.UtcNow
            });
        }
    }
}
