using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using System.Linq.Expressions;

namespace ExportOrderWebServer.Areas.ImportDocument.Terminal.Provider
{
    public class TerminalCrudProvider : BaseCrudProvider<TerminalBaseEntity>
    {
        public TerminalCrudProvider(
            IRepository<TerminalBaseEntity> repository,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, httpContextAccessor)
        {
        }

        protected override Expression<Func<TerminalBaseEntity, bool>>? CreateSearchFilter(string searchTerm)
        {
            return e => e.Name.Contains(searchTerm) ||
                       e.Email.Contains(searchTerm) ||
                       e.CustomsPost.Contains(searchTerm) ||
                       e.CustomsPostName.Contains(searchTerm);
        }

        protected override Func<IQueryable<TerminalBaseEntity>, IOrderedQueryable<TerminalBaseEntity>>?
            CreateOrderBy(string? sortBy, bool sortDesc)
        {
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                return sortBy switch
                {
                    "Name" => sortDesc ?
                        q => q.OrderByDescending(e => e.Name) :
                        q => q.OrderBy(e => e.Name),
                    "CustomsPost" => sortDesc ?
                        q => q.OrderByDescending(e => e.CustomsPost) :
                        q => q.OrderBy(e => e.CustomsPost),
                    "CustomsPostName" => sortDesc ?
                        q => q.OrderByDescending(e => e.CustomsPostName) :
                        q => q.OrderBy(e => e.CustomsPostName),
                    "FullName" => sortDesc ?
                        q => q.OrderByDescending(e => e.CustomsPostName ?? e.Name) :
                        q => q.OrderBy(e => e.CustomsPostName ?? e.Name),
                    _ => base.CreateOrderBy(sortBy, sortDesc)
                };
            }

            return base.CreateOrderBy(sortBy, sortDesc);
        }

        public override async Task<TerminalBaseEntity> CreateAsync(TerminalBaseEntity entity, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Terminal name is required");

            return await base.CreateAsync(entity, cancellationToken);
        }
    }
}