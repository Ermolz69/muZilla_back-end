using muZilla.Entities;

namespace muZilla.Application.Interfaces
{
    public interface IGenericRepository
    {
        Task<T?> GetByIdAsync<T>(int? id) where T : class, muZilla.Entities.IModel;
        Task<IQueryable<T>> GetAllAsync<T>() where T : class, muZilla.Entities.IModel;
        Task<T> AddAsync<T>(T entity) where T : class, muZilla.Entities.IModel;
        Task UpdateAsync<T>(T entity) where T : class, muZilla.Entities.IModel;
        Task RemoveAsync<T>(T entity) where T : class, muZilla.Entities.IModel;
        Task RemoveRangeAsync<T>(List<T> entities) where T : class, muZilla.Entities.IModel;
        Task SaveChangesAsync();
    }
}