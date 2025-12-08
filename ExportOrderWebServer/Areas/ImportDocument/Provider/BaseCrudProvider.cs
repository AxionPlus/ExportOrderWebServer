// BaseCrudProvider.cs
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using global::ExportOrderEntites.ImportDocument;
using System.Linq.Expressions;



namespace ExportOrderWebServer.Areas.ImportDocument.Provider
{
    public abstract class BaseCrudProvider<T> : ICrudProvider<T> where T : class, IBaseEntity
    {
        protected readonly IRepository<T> _repository;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public BaseCrudProvider(
            IRepository<T> repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            return await _repository.GetByIdAsync(id, cancellationToken, includes);
        }

        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken,
            string? searchTerm = null,
            string? sortBy = null,
            bool sortDesc = false, 
            params Expression<Func<T, object>>[] includes)
        {
            // Фильтр для исключения удаленных записей
            Expression<Func<T, bool>> baseFilter = e => e.Status != BaseEntityStatus.Canceled;

            Expression<Func<T, bool>>? searchFilter = null;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchFilter = CreateSearchFilter(searchTerm);
            }

            // Комбинируем фильтры
            Expression<Func<T, bool>>? finalFilter = null;
            if (searchFilter != null)
            {
                finalFilter = Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(
                        baseFilter.Body,
                        Expression.Invoke(searchFilter, baseFilter.Parameters[0])),
                    baseFilter.Parameters);
            }
            else
            {
                finalFilter = baseFilter;
            }

            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = CreateOrderBy(sortBy, sortDesc);

            return await _repository.GetPaginatedAsync(
                pageNumber,
                pageSize,
                finalFilter,
                orderBy, includes);
        }

        protected virtual Expression<Func<T, bool>>? CreateSearchFilter(string searchTerm)
        {
            // Базовая реализация поиска по Id
            if (Guid.TryParse(searchTerm, out var guid))
            {
                return e => e.Id == guid;
            }

            // Поиск по CreatedBy/UpdatedBy
            return e => (e.CreatedBy != null && e.CreatedBy.Contains(searchTerm)) ||
                       (e.UpdatedBy != null && e.UpdatedBy.Contains(searchTerm)) ||
                       (e.DeletedBy != null && e.DeletedBy.Contains(searchTerm));
        }

        protected virtual Func<IQueryable<T>, IOrderedQueryable<T>>? CreateOrderBy(string? sortBy, bool sortDesc)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
            {
                return sortDesc ?
                    q => q.OrderByDescending(e => e.CreatedAt) :
                    q => q.OrderBy(e => e.CreatedAt);
            }

            return sortBy switch
            {
                "CreatedAt" => sortDesc ?
                    q => q.OrderByDescending(e => e.CreatedAt) :
                    q => q.OrderBy(e => e.CreatedAt),
                "UpdatedAt" => sortDesc ?
                    q => q.OrderByDescending(e => e.UpdatedAt) :
                    q => q.OrderBy(e => e.UpdatedAt),
                "Status" => sortDesc ?
                    q => q.OrderByDescending(e => e.Status) :
                    q => q.OrderBy(e => e.Status),
                _ => sortDesc ?
                    q => q.OrderByDescending(e => e.CreatedAt) :
                    q => q.OrderBy(e => e.CreatedAt)
            };
        }

        public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken)
        {
          //  SetAuditFields(entity, true);
            return await _repository.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(T entity, CancellationToken cancellationToken)
        {
          //  SetAuditFields(entity, false);
            entity.Version++;
            await _repository.UpdateAsync(entity);
        }

        public virtual async Task DeleteAsync(T entity, CancellationToken cancellationToken)
        {
           //entity.Status = BaseEntityStatus.Canceled;
           //entity.DeletedAt = DateTimeOffset.UtcNow;
           //entity.DeletedBy = GetCurrentUserId();
           //entity.DeleteReason = "Deleted via CRUD provider";
           //entity.Version++;

            await _repository.DeleteAsync(entity);
        }

        protected virtual void SetAuditFields(T entity, bool isNew)
        {
            var currentUserId = GetCurrentUserId();
            var currentTime = DateTimeOffset.UtcNow;

            if (isNew)
            {
                entity.CreatedAt = currentTime;
                entity.CreatedBy = currentUserId;
                entity.Status = BaseEntityStatus.New;
                entity.Version = 1;
                entity.HandledBySystem = false;

                if (entity.Id == Guid.Empty)
                {
                    entity.Id = Guid.NewGuid();
                }
            }
            else
            {
                entity.UpdatedAt = currentTime;
                entity.UpdatedBy = currentUserId;
            }
        }

        protected virtual string GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ??
                   _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ??
                   "System";
        }

        public virtual IQueryable<T> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            return  _repository.GetAllAsync(includes);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        {
            var baseFilter = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(
                    predicate.Body,
                    Expression.Equal(
                        Expression.Property(predicate.Parameters[0], "Status"),
                        Expression.Constant(BaseEntityStatus.Canceled, typeof(BaseEntityStatus)))),
                predicate.Parameters);

            return await _repository.FindAsync(baseFilter);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate, CancellationToken cancellationToken)
        {
            if (predicate == null)
            {
                return await _repository.CountAsync(e => e.Status != BaseEntityStatus.Canceled);
            }

            var baseFilter = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(
                    predicate.Body,
                    Expression.NotEqual(
                        Expression.Property(predicate.Parameters[0], "Status"),
                        Expression.Constant(BaseEntityStatus.Canceled, typeof(BaseEntityStatus)))),
                predicate.Parameters);

            return await _repository.CountAsync(baseFilter);
        }
    }
}