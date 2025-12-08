
using System.Linq.Expressions;

namespace ExportOrderWebServer.Areas.ImportDocument.Provider
{
    public interface ICrudProvider<T>
    {
        IQueryable<T> GetAllAsync (params Expression<Func<T, object>>[] includes);
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes);
        Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize, CancellationToken cancellationToken = default,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDesc = false,
            params Expression<Func<T, object>>[] includes);
        Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
    }



    public class PaginatedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}
