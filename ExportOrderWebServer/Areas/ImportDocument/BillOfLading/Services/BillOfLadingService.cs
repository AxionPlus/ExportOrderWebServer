using ExportOrderEntites;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Port.Services;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Services;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Services;

public interface IBillOfLadingService
{
    Task<BillOfLadingBaseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<BillOfLadingBaseDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false);
    Task<BillOfLadingBaseDto> CreateAsync(BillOfLadingBaseDto baseDto, CancellationToken cancellationToken = default);
    Task<BillOfLadingBaseDto> UpdateAsync(BillOfLadingBaseDto baseDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<BillOfLadingBaseDto>> GetByVesselCallIdAsync(Guid vesselCallId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BillOfLadingBaseDto>> GetByContainerNumberAsync(string containerNo, CancellationToken cancellationToken = default);
    Task<bool> CheckBillNumberExistsAsync(string billNo, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task UpdateAsync(Dto.BillOfLadingContainerRecordBaseDto baseDto, CancellationToken cancellationToken = default);
}

public class BillOfLadingService : IBillOfLadingService
{
    private readonly ICrudProvider<BillOfLadingBaseEntity> _billOfLadingCrudProvider;
    private readonly IVesselCallService _vesselCallService;
    private readonly IPortService _portService;
    private readonly ILogger<BillOfLadingService> _logger;
    protected readonly ApplicationDbContext _context;
    public BillOfLadingService(
        ICrudProvider<BillOfLadingBaseEntity> billOfLadingCrudProvider,
    ApplicationDbContext context,
    IVesselCallService vesselCallService,
    IPortService portService,
    ILogger<BillOfLadingService> logger)
    {
        _billOfLadingCrudProvider = billOfLadingCrudProvider;
        _vesselCallService = vesselCallService;
        _portService = portService;
        _logger = logger;
        _context = context;
    }

    public async Task<BillOfLadingBaseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _billOfLadingCrudProvider.GetByIdAsync(
                id,
                cancellationToken,
                s => s.VesselCall,
                s => s.VesselCall.Vessel,
                s => s.VesselCall.Terminal,
                s => s.VesselCall.PortOfLoading,
                s => s.ContainerRecords);

            if (entity == null)
                throw new KeyNotFoundException($"Bill of lading with ID {id} not found");

            return entity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bill of lading by ID: {BillId}", id);
            throw;
        }
    }

    public async Task<PaginatedResult<BillOfLadingBaseDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        try
        {
            var entities = await _billOfLadingCrudProvider.GetPaginatedAsync(
                pageNumber,
                pageSize,
                cancellationToken,
                searchTerm,
                sortBy,
                sortDesc,
                s => s.VesselCall,
                s => s.VesselCall.Vessel);

            return new PaginatedResult<BillOfLadingBaseDto>
            {
                Items = entities.Items.ToDtoList(),
                TotalCount = entities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = entities.TotalPages,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated bills of lading");
            throw;
        }
    }

    public async Task<BillOfLadingBaseDto> CreateAsync(BillOfLadingBaseDto baseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем уникальность номера BL
            var exists = await CheckBillNumberExistsAsync(baseDto.Num, null, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Bill of lading with number '{baseDto.Num}' already exists");

            // Проверяем существование связанной сущности VesselCall
            await ValidateVesselCall(baseDto.VesselCallId, cancellationToken);

            // Устанавливаем BillOfLadingId для ContainerRecords
            foreach (var containerRecord in baseDto.ContainerRecords)
            {
                containerRecord.BillOfLadingId = baseDto.Id;
            }


            var pol = await _portService.GetByIsoCodeAsync(baseDto.Pol?.IsoCode, cancellationToken);

            if (pol != null)
                baseDto.Pol = pol;
            else
            {
                var newPol = new PortDto()
                {
                    NameRu = baseDto.Pol.IsoCode,
                    NameEn = baseDto.Pol.IsoCode,
                    CountryEn = baseDto.Pol.IsoCode,
                    CountryRu = baseDto.Pol.IsoCode,
                    IsoCode = baseDto.Pol.IsoCode,
                    PikYugIsoCode = baseDto.Pol.IsoCode,
                    HandledBySystem = true,
                };

                baseDto.Pol = await _portService.CreateAsync(newPol, cancellationToken);
            }

            var entity = baseDto.ToEntity();

            var createdEntity = await _billOfLadingCrudProvider.CreateAsync(entity, cancellationToken);
            return createdEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bill of lading with number: {BillNumber}", baseDto.Num);
            throw;
        }
    }

    public async Task<BillOfLadingBaseDto> UpdateAsync(BillOfLadingBaseDto baseDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _billOfLadingCrudProvider.GetByIdAsync(
                baseDto.Id,
                cancellationToken,
                s => s.ContainerRecords);

            if (existingEntity == null)
                throw new KeyNotFoundException($"Bill of lading with ID {baseDto.Id} not found");

            // Проверяем уникальность номера BL, если он изменился
            if (existingEntity.Num != baseDto.Num)
            {
                var exists = await CheckBillNumberExistsAsync(baseDto.Num, baseDto.Id, cancellationToken);
                if (exists)
                    throw new InvalidOperationException($"Bill of lading with number '{baseDto.Num}' already exists");
            }

            // Проверяем существование связанной сущности VesselCall
            await ValidateVesselCall(baseDto.VesselCallId, cancellationToken);


            existingEntity.UpdateEntity(baseDto);
            await _billOfLadingCrudProvider.UpdateAsync(existingEntity, cancellationToken);
            return existingEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bill of lading: {BillId}", baseDto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _billOfLadingCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            await _billOfLadingCrudProvider.DeleteAsync(entity, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bill of lading: {BillId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<BillOfLadingBaseDto>> GetByVesselCallIdAsync(Guid vesselCallId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = _billOfLadingCrudProvider
                .GetAllAsync(
                s => s.VesselCall, 
                s => s.Pol,
                s => s.TsPort,
                s=>s.ContainerRecords)
                .AsSplitQuery(); ;
            entities = entities.Where(e => e.VesselCallId == vesselCallId);
            var filteredEntities = await entities.ToListAsync(cancellationToken);
            return filteredEntities.ToDtoList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bills of lading by vessel call ID: {VesselCallId}", vesselCallId);
            throw;
        }
    }

    public async Task<IEnumerable<BillOfLadingBaseDto>> GetByContainerNumberAsync(string containerNo, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = _billOfLadingCrudProvider.GetAllAsync(
                s => s.VesselCall,
                s => s.VesselCall.Vessel,
                s => s.ContainerRecords);

            var filteredEntities = await entities.Where(e =>
                e.ContainerRecords.Any(cr =>
                    cr.ContainerNo.Contains(containerNo, StringComparison.OrdinalIgnoreCase))).ToListAsync(cancellationToken); ;

            return filteredEntities.ToDtoList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bills of lading by container number: {ContainerNo}", containerNo);
            throw;
        }
    }

    public async Task<bool> CheckBillNumberExistsAsync(string billNo, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _billOfLadingCrudProvider.GetAllAsync().ToListAsync(cancellationToken); ;

            if (excludeId.HasValue)
                return entities.Any(e => e.Num == billNo && e.Id != excludeId.Value);

            return entities.Any(e => e.Num == billNo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking bill number existence: {BillNumber}", billNo);
            throw;
        }
    }

    public async Task UpdateAsync(BillOfLadingContainerRecordBaseDto baseDto, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Set<BillOfLadingContainerRecordBaseEntity>().FirstAsync(s => s.Id == baseDto.Id, cancellationToken: cancellationToken);

        if (entity.Timestamp != baseDto.Timestamp)
        {
            throw new ArgumentException($"has been changed by another User");
        }

        entity.UpdateEntity(baseDto);
        _context.Set<BillOfLadingContainerRecordBaseEntity>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);


    }

    private async Task ValidateVesselCall(Guid vesselCallId, CancellationToken cancellationToken)
    {
        try
        {
            await _vesselCallService.GetByIdAsync(vesselCallId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"Vessel call with ID {vesselCallId} does not exist");
        }
    }
}