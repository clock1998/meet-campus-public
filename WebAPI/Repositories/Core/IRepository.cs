using WebAPI.Infrastructure;

namespace Template.WebAPI.Repositories.Core
{
    public interface IRepository<T> where T : class
    {
        public IQueryable<T> GetAll(QueryStringParameters queryParameters);

        public Task<T> GetAsync(Guid id);

        public Task<T> CreateAsync(T model);

        public void UpdateAsync(Guid id, T model);
        public Task SaveChangesAsync();

        public Task<T> Delete(Guid id);

    }
}
