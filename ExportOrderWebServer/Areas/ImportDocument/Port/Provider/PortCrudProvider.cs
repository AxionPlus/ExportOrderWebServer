// PortCrudProvider.cs
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using System.Linq.Expressions;

namespace ExportOrderWebServer.Areas.ImportDocument.Port.Provider
{
    public class PortCrudProvider : BaseCrudProvider<PortBaseEntity>
    {
        public PortCrudProvider(
            IRepository<PortBaseEntity> repository,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, httpContextAccessor)
        {
        }

        protected override Expression<Func<PortBaseEntity, bool>>? CreateSearchFilter(string searchTerm)
        {
            return e => e.NameRu.ToUpper().Contains(searchTerm.ToUpper()) ||
                        e.NameEn.ToUpper().Contains(searchTerm.ToUpper()) ||
                        e.CountryRu.ToUpper().Contains(searchTerm.ToUpper()) ||
                        e.CountryEn.ToUpper().Contains(searchTerm.ToUpper()) ||
                        e.IsoCode.ToUpper().Contains(searchTerm.ToUpper()) ||
                        e.AuxIsoCode.ToUpper().Contains(searchTerm.ToUpper()) ||
                        e.PikYugIsoCode.ToUpper().Contains(searchTerm.ToUpper());
        }

        protected override Func<IQueryable<PortBaseEntity>, IOrderedQueryable<PortBaseEntity>>?
            CreateOrderBy(string? sortBy, bool sortDesc)
        {
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                return sortBy switch
                {
                    "NameRu" => sortDesc ? q => q.OrderByDescending(e => e.NameRu) : q => q.OrderBy(e => e.NameRu),
                    "NameEn" => sortDesc ? q => q.OrderByDescending(e => e.NameEn) : q => q.OrderBy(e => e.NameEn),
                    "CountryRu" => sortDesc
                        ? q => q.OrderByDescending(e => e.CountryRu)
                        : q => q.OrderBy(e => e.CountryRu),
                    "IsoCode" => sortDesc
                        ? q => q.OrderByDescending(e => e.IsoCode)
                        : q => q.OrderBy(e => e.IsoCode),
                    "FullRu" => sortDesc
                        ? q => q.OrderByDescending(e => e.NameRu).ThenByDescending(e => e.CountryRu)
                        : q => q.OrderBy(e => e.NameRu).ThenBy(e => e.CountryRu),
                    "FullEn" => sortDesc
                        ? q => q.OrderByDescending(e => e.NameEn).ThenByDescending(e => e.CountryEn)
                        : q => q.OrderBy(e => e.NameEn).ThenBy(e => e.CountryEn),
                    _ => base.CreateOrderBy(sortBy, sortDesc)
                };
            }

            return base.CreateOrderBy(sortBy, sortDesc);
        }

        public override async Task<PortBaseEntity> CreateAsync(PortBaseEntity entity,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(entity.NameRu))
                throw new ArgumentException("Russian port name is required");

            if (string.IsNullOrWhiteSpace(entity.CountryRu))
                throw new ArgumentException("Russian country name is required");

            if (string.IsNullOrWhiteSpace(entity.IsoCode))
                throw new ArgumentException("ISO code is required");

            return await base.CreateAsync(entity, cancellationToken);
        }
    }

}
