using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using System.Linq.Expressions;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Provider;

public class BillOfLadingCrudProvider : BaseCrudProvider<BillOfLadingBaseEntity>
{
    public BillOfLadingCrudProvider(
        IRepository<BillOfLadingBaseEntity> repository,
        IHttpContextAccessor httpContextAccessor)
        : base(repository, httpContextAccessor)
    {
    }

    protected IQueryable<BillOfLadingBaseEntity> IncludeNavigationProperties(IQueryable<BillOfLadingBaseEntity> query)
    {
        return query
            .Include(bl => bl.VesselCall)
                .ThenInclude(vc => vc.Vessel)
            .Include(bl => bl.VesselCall)
                .ThenInclude(vc => vc.Terminal)
            .Include(bl => bl.VesselCall)
                .ThenInclude(vc => vc.PortOfLoading)
            .Include(bl => bl.ContainerRecords);
    }

    protected override Expression<Func<BillOfLadingBaseEntity, bool>>? CreateSearchFilter(string searchTerm)
    {
        return e => e.Num.Contains(searchTerm) ||
                   e.ShipperName.Contains(searchTerm) ||
                   e.ShipperCode.Contains(searchTerm) ||
                   e.ConsigneeName.Contains(searchTerm) ||
                   e.ConsigneeCode.Contains(searchTerm) ||
                   e.CargoDescription.Contains(searchTerm) ||
                   (e.VesselCall != null && e.VesselCall.VoyageNo.Contains(searchTerm)) ||
                   e.ContainerRecords.Any(cr => cr.ContainerNo.Contains(searchTerm));
    }

    protected override Func<IQueryable<BillOfLadingBaseEntity>, IOrderedQueryable<BillOfLadingBaseEntity>>?
        CreateOrderBy(string? sortBy, bool sortDesc)
    {
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            return sortBy switch
            {
                "Num" => sortDesc ?
                    q => q.OrderByDescending(e => e.Num) :
                    q => q.OrderBy(e => e.Num),
                "Date" => sortDesc ?
                    q => q.OrderByDescending(e => e.Date) :
                    q => q.OrderBy(e => e.Date),
                "ShipperName" => sortDesc ?
                    q => q.OrderByDescending(e => e.ShipperName) :
                    q => q.OrderBy(e => e.ShipperName),
                "ConsigneeName" => sortDesc ?
                    q => q.OrderByDescending(e => e.ConsigneeName) :
                    q => q.OrderBy(e => e.ConsigneeName),
                "VesselCallVoyageNo" => sortDesc ?
                    q => q.OrderByDescending(e => e.VesselCall.VoyageNo) :
                    q => q.OrderBy(e => e.VesselCall.VoyageNo),
                _ => base.CreateOrderBy(sortBy, sortDesc)
            };
        }

        return base.CreateOrderBy(sortBy, sortDesc);
    }

    public override async Task<BillOfLadingBaseEntity> CreateAsync(BillOfLadingBaseEntity entity, CancellationToken cancellationToken)
    {
        ValidateBillOfLading(entity);
        return await base.CreateAsync(entity, cancellationToken);
    }

    public override async Task UpdateAsync(BillOfLadingBaseEntity entity, CancellationToken cancellationToken)
    {
        ValidateBillOfLading(entity);
        await base.UpdateAsync(entity, cancellationToken);
    }

    public override async Task DeleteAsync(BillOfLadingBaseEntity entity, CancellationToken cancellationToken)
    {
        await base.DeleteAsync(entity, cancellationToken);
    }

    private void ValidateBillOfLading(BillOfLadingBaseEntity entity)
    {
        if (string.IsNullOrWhiteSpace(entity.Num))
            throw new ArgumentException("BL number is required");

        if (entity.Date == default)
            throw new ArgumentException("BL date is required");

        if (entity.VesselCallId == Guid.Empty)
            throw new ArgumentException("Vessel call is required");

        if (string.IsNullOrWhiteSpace(entity.ShipperName))
            throw new ArgumentException("Shipper name is required");

        if (string.IsNullOrWhiteSpace(entity.ConsigneeName))
            throw new ArgumentException("Consignee name is required");

        if (entity.Date > DateTime.UtcNow)
            throw new ArgumentException("BL date cannot be in the future");

        // Проверяем контейнерные записи
        foreach (var container in entity.ContainerRecords)
        {
            ValidateContainerRecord(container);
        }
    }

    private void ValidateContainerRecord(BillOfLadingContainerRecordBaseEntity container)
    {
        if (string.IsNullOrWhiteSpace(container.ContainerNo))
            throw new ArgumentException("Container number is required");

        if (container.NoOfPackage <= 0)
            throw new ArgumentException("Number of packages must be greater than 0");

        if (container.GrossWeight <= 0)
            throw new ArgumentException("Gross weight must be greater than 0");
    }
}