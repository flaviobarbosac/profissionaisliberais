using System.Linq.Expressions;

namespace Libify.Repository.Interface
{
    public interface IBaseRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(int page, int pageSize);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        Task AddRangeAsync(IEnumerable<T> entities);
    }
}
