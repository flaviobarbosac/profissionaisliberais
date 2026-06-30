using Libify.Domain.Model.Base;
using Libify.Repository.Interface;
using Libify.Services.Interface;

namespace Libify.Services
{
    public class BaseServices<T> : IBaseServices<T> where T : ModelBase
    {
        private readonly IBaseRepository<T> _repository;

        public BaseServices(IBaseRepository<T> repository)
        {
            _repository = repository;
        }

        public Task<T?> GetByIdAsync(Guid id) => _repository.GetByIdAsync(id);

        public Task<IEnumerable<T>> GetAllAsync() => _repository.GetAllAsync();

        public Task<(IEnumerable<T> Items, int Total)> GetPagedAsync(int page, int pageSize)
            => _repository.GetPagedAsync(page, pageSize);

        public Task AddAsync(T entity) => _repository.AddAsync(entity);

        public Task UpdateAsync(T entity) => _repository.UpdateAsync(entity);

        public Task DeleteAsync(Guid id) => _repository.DeleteAsync(id);

        public Task SoftDeleteAsync(Guid id) => _repository.SoftDeleteAsync(id);
    }
}
