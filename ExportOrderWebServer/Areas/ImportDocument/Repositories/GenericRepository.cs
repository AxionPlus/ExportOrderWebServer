using ExportOrderWebServer.Areas.ImportDocument.Provider;
using System.Linq.Expressions;

namespace ExportOrderWebServer.Areas.ImportDocument.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly IDbContextFactory<ApplicationDbContext> _context;
        // protected readonly DbSet<T> _dbSet;

        public GenericRepository(IDbContextFactory<ApplicationDbContext> context)
        {
            _context = context;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default, params Expression<Func<T, object>>[] includes)
        {
            await using var context = await _context.CreateDbContextAsync(cancellationToken);
            IQueryable<T> query = context.Set<T>().AsQueryable();

            // Включение связанных данных
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id, cancellationToken);

        }

        public virtual async Task<T?> GetByIdAsync(long id, params Expression<Func<T, object>>[] includes)
        {
            await using var context = await _context.CreateDbContextAsync();
            IQueryable<T> query = context.Set<T>().AsQueryable();

            // Включение связанных данных
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<long>(e, "Id") == id);
        }

        public virtual IQueryable<T> GetAllAsync( params Expression<Func<T, object>>[] includes)
        {
            var context = _context.CreateDbContext();
            IQueryable<T> query = context.Set<T>().AsQueryable();

            // Включение связанных данных
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            await using var context = await _context.CreateDbContextAsync();
         
            return await context.Set<T>().Where(predicate).ToListAsync();
        }

        public virtual async Task<PaginatedResult<T>> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            params Expression<Func<T, object>>[] includes)
        {
            await using var context = await _context.CreateDbContextAsync();
            IQueryable<T> query = context.Set<T>().AsQueryable();

            // Включение связанных данных
            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.CountAsync();

            if (orderBy != null)
            {
                query = orderBy(query);
            }
            else
            {
                query = query.OrderByDescending(e => EF.Property<DateTimeOffset>(e, "CreatedAt"));
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public virtual async Task<T> AddAsync(T entity)
        {

            await using var context = await _context.CreateDbContextAsync();
    
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            await using var context = await _context.CreateDbContextAsync();
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }


        public virtual async Task DeleteAsync(T entity)
        {
            await using var context = await _context.CreateDbContextAsync();
            context.Set<T>().Remove(entity);
        
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public virtual async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            await using var context = await _context.CreateDbContextAsync();
          
            return await context.Set<T>().AnyAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        public virtual async Task<bool> ExistsAsync(long id)
        {
            await using var context = await _context.CreateDbContextAsync();
            return await context.Set<T>().AnyAsync(e => EF.Property<long>(e, "Id") == id);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            await using var context = await _context.CreateDbContextAsync();

            if (predicate != null)
            {
                return await context.Set<T>().CountAsync(predicate);
            }
            return await context.Set<T>().CountAsync();
        }
    }
}