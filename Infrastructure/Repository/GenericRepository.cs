using Microsoft.EntityFrameworkCore;
using muZilla.Application.Interfaces;
using muZilla.Infrastructure.Data;

namespace muZilla.Infrastructure.Repository
{
    public class GenericRepository : IGenericRepository
    {
        protected readonly MuzillaDbContext _context;

        public GenericRepository(MuzillaDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync<T>(int? id) where T : class, muZilla.Entities.IModel
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public Task<IQueryable<T>> GetAllAsync<T>() where T : class, muZilla.Entities.IModel
        {
            return Task.FromResult(_context.Set<T>().AsQueryable());
        }

        public async Task<T> AddAsync<T>(T entity) where T : class, muZilla.Entities.IModel
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        public Task UpdateAsync<T>(T entity) where T : class, muZilla.Entities.IModel
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public Task RemoveAsync<T>(T entity) where T : class, muZilla.Entities.IModel
        {
            _context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }

        public Task RemoveRangeAsync<T>(List<T> entities) where T : class, muZilla.Entities.IModel
        {
            _context.Set<T>().RemoveRange(entities);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

