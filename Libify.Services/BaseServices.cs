using Libify.Domain.Helpers;
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

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task AddAsync(T entity)
        {
            entity.CreatedAt = DateTimeHelper.Now;
            entity.UpdatedAt = DateTimeHelper.Now;
            await _repository.AddAsync(entity);
        }

        public async Task UpdateAsync(T entity)
        {
            entity.UpdatedAt = DateTimeHelper.Now;
            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        public async Task SoftDeleteAsync(int id)
        {
            await _repository.SoftDeleteAsync(id);
        }
    }
}
