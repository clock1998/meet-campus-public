using Microsoft.EntityFrameworkCore;
using WebAPI.Infrastructure;
using WebAPI.Infrastructure.Context;

namespace Template.WebAPI.Repositories.Core
{
    public class Repository<T> : IRepository<T> where T : class
    {
        public readonly AppDbContext _dbContext;
        private DbSet<T> _dbSet;
        public Repository(AppDbContext context)
        {
            _dbContext = context;
            _dbSet = _dbContext.Set<T>();
        }
        public virtual IQueryable<T> GetAll(QueryStringParameters queryParameters)
        {
            return _dbSet;
            //return await Task.FromResult(context.Set<T>().AsQueryable().OrderBy(n => n.Id).Skip((queryParameters.PageNumber-1) * queryParameters.PageSize).Take(queryParameters.PageSize));
        }
        public virtual async Task<T> GetAsync(Guid id)
        {
            var result = await _dbSet.FindAsync(id);
            if(result == null) throw new InvalidOperationException($"A {typeof(T)} with ID {id} was not found.");
            return result;
        }

        public virtual async Task<T> CreateAsync(T model)
        {
            await _dbSet.AddAsync(model);
            return model;
        }

        public virtual void UpdateAsync(Guid id, T model)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<T> Delete(Guid id)
        {
            var model = await GetAsync(id);
            if (model == null) throw new InvalidOperationException($"A {typeof(T)} with ID {id} was not found.");
            _dbSet.Remove(model);
            await _dbContext.SaveChangesAsync();
            return model;
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
