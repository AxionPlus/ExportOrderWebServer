// IRepository.cs
using System.Linq.Expressions;
using ExportOrderWebServer.Areas.ImportDocument.Provider;

namespace ExportOrderWebServer.Areas.ImportDocument.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id,  CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync(long id, params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync( params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteAsync(Guid id);
        Task DeleteAsync(long id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsAsync(long id);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    }

 
}