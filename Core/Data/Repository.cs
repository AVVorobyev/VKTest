using Core.Data.Models;
using System.Linq.Expressions;

namespace Core.Data
{
    public abstract class Repository<T> where T : IModel
    {
        public abstract Task<Result> AddAsync(T entity);

        public abstract Task<Result<T>> GetAsync(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool asNoTracking = false);

        public abstract Task<Result<IEnumerable<T>>> GetRangeAsync(
            Expression<Func<T, bool>>? filter = null,
            int skip = 0,
            int take = 10,
            string? includeProperties = null);

        public abstract Task<Result> DeleteAsync(int id);
    }
}
