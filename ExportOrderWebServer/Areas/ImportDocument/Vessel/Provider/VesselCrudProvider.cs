// VesselCrudProvider.cs
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using System.Linq.Expressions;

namespace ExportOrderWebServer.Areas.ImportDocument.Vessel.Provider
{
    public class VesselCrudProvider : BaseCrudProvider<VesselBaseEntity>
    {
        public VesselCrudProvider(
            IRepository<VesselBaseEntity> repository,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, httpContextAccessor)
        {
        }

        protected override Expression<Func<VesselBaseEntity, bool>>? CreateSearchFilter(string searchTerm)
        {
            return e => e.Name.Contains(searchTerm) ||
                       e.ShortName.Contains(searchTerm) ||
                       e.FlagRu.Contains(searchTerm) ||
                       e.FlagEn.Contains(searchTerm) ||
                       e.RolisCode.Contains(searchTerm) ;
        }

        protected override Func<IQueryable<VesselBaseEntity>, IOrderedQueryable<VesselBaseEntity>>?
            CreateOrderBy(string? sortBy, bool sortDesc)
        {
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                return sortBy switch
                {
                    "Name" => sortDesc ?
                        q => q.OrderByDescending(e => e.Name) :
                        q => q.OrderBy(e => e.Name),
                    "ShortName" => sortDesc ?
                        q => q.OrderByDescending(e => e.ShortName) :
                        q => q.OrderBy(e => e.ShortName),
                    "FlagRu" => sortDesc ?
                        q => q.OrderByDescending(e => e.FlagRu) :
                        q => q.OrderBy(e => e.FlagRu),
                    "RolisCode" => sortDesc ?
                        q => q.OrderByDescending(e => e.RolisCode) :
                        q => q.OrderBy(e => e.RolisCode),
                    _ => base.CreateOrderBy(sortBy, sortDesc)
                };
            }

            return base.CreateOrderBy(sortBy, sortDesc);
        }

        public override async Task<VesselBaseEntity> CreateAsync(VesselBaseEntity entity, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Vessel name is required");

            return await base.CreateAsync(entity, cancellationToken);
        }
    }
}
