namespace Libify.Services.Interface
{
    public interface IBaseServices<T>
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(int page, int pageSize);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
    }
}
