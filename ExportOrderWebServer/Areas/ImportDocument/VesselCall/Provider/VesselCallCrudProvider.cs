using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.ImportDocument.VesselCall.Provider
{
    public class VesselCallCrudProvider : BaseCrudProvider<VesselCallBaseEntity>
    {
        public VesselCallCrudProvider(
            IRepository<VesselCallBaseEntity> repository,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, httpContextAccessor)
        {
        }

        protected IQueryable<VesselCallBaseEntity> IncludeNavigationProperties(IQueryable<VesselCallBaseEntity> query)
        {
            return query
                .Include(vc => vc.Vessel)
                .Include(vc => vc.Terminal)
                .Include(vc => vc.PortOfLoading)
                .Include(vc => vc.BillOfLadings);
        }

        protected override Expression<Func<VesselCallBaseEntity, bool>>? CreateSearchFilter(string searchTerm)
        {
            return e => e.VoyageNo.Contains(searchTerm) ||
                       e.TerminalVoyageNo.Contains(searchTerm) ||
                       e.FeederBlNo.Contains(searchTerm) ||
                       (e.Vessel != null && e.Vessel.Name.Contains(searchTerm)) ||
                       (e.Vessel != null && e.Vessel.ShortName.Contains(searchTerm)) ||
                       (e.Terminal != null && e.Terminal.Name.Contains(searchTerm)) ||
                       (e.PortOfLoading != null && e.PortOfLoading.NameRu.Contains(searchTerm)) ||
                       (e.PortOfLoading != null && e.PortOfLoading.NameEn.Contains(searchTerm));
        }

        protected override Func<IQueryable<VesselCallBaseEntity>, IOrderedQueryable<VesselCallBaseEntity>>?
            CreateOrderBy(string? sortBy, bool sortDesc)
        {
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                return sortBy switch
                {
                    "VoyageNo" => sortDesc ?
                        q => q.OrderByDescending(e => e.VoyageNo) :
                        q => q.OrderBy(e => e.VoyageNo),
                    "VesselName" => sortDesc ?
                        q => q.OrderByDescending(e => e.Vessel.Name) :
                        q => q.OrderBy(e => e.Vessel.Name),
                    "ETA" => sortDesc ?
                        q => q.OrderByDescending(e => e.ETA) :
                        q => q.OrderBy(e => e.ETA),
                    "ETS" => sortDesc ?
                        q => q.OrderByDescending(e => e.ETS) :
                        q => q.OrderBy(e => e.ETS),
                    "PortOfLoading" => sortDesc ?
                        q => q.OrderByDescending(e => e.PortOfLoading.NameRu) :
                        q => q.OrderBy(e => e.PortOfLoading.NameRu),
                    "TerminalName" => sortDesc ?
                        q => q.OrderByDescending(e => e.Terminal.Name) :
                        q => q.OrderBy(e => e.Terminal.Name),
                    _ => base.CreateOrderBy(sortBy, sortDesc)
                };
            }

            return base.CreateOrderBy(sortBy, sortDesc);
        }

        public override async Task<VesselCallBaseEntity> CreateAsync(VesselCallBaseEntity entity, CancellationToken cancellationToken)
        {
            ValidateVesselCall(entity);
            return await base.CreateAsync(entity, cancellationToken);
        }

        public override async Task UpdateAsync(VesselCallBaseEntity entity, CancellationToken cancellationToken)
        {
            ValidateVesselCall(entity);
             await base.UpdateAsync(entity, cancellationToken);
        }

        private void ValidateVesselCall(VesselCallBaseEntity entity)
        {
            if (entity.VesselId == Guid.Empty)
                throw new ArgumentException("Vessel is required");

            if (string.IsNullOrWhiteSpace(entity.VoyageNo))
                throw new ArgumentException("Voyage number is required");

            if (entity.TerminalId == Guid.Empty)
                throw new ArgumentException("Terminal is required");

            if (entity.PortOfLoadingId == Guid.Empty)
                throw new ArgumentException("Port of loading is required");

            if (entity.ETA == default)
                throw new ArgumentException("ETA is required");

            if (entity.ETS == default)
                throw new ArgumentException("ETS is required");

            if (entity.ETS < entity.ETA)
                throw new ArgumentException("ETS must be after or equal to ETA");
        }
    }
}