using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Services;
using Dto = ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;

namespace ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Services;

public interface IBillOfLadingService
{
    Task<Dto.BillOfLadingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<Dto.BillOfLadingDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false);
    Task<Dto.BillOfLadingDto> CreateAsync(Dto.BillOfLadingDto dto, CancellationToken cancellationToken = default);
    Task<Dto.BillOfLadingDto> UpdateAsync(Dto.BillOfLadingDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dto.BillOfLadingDto>> GetByVesselCallIdAsync(Guid vesselCallId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Dto.BillOfLadingDto>> GetByContainerNumberAsync(string containerNo, CancellationToken cancellationToken = default);
    Task<bool> CheckBillNumberExistsAsync(string billNo, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

public class BillOfLadingService : IBillOfLadingService
{
    private readonly ICrudProvider<BillOfLadingBaseEntity> _billOfLadingCrudProvider;
    private readonly IVesselCallService _vesselCallService;
    private readonly ILogger<BillOfLadingService> _logger;

    public BillOfLadingService(
        ICrudProvider<BillOfLadingBaseEntity> billOfLadingCrudProvider,
        IVesselCallService vesselCallService,
        ILogger<BillOfLadingService> logger)
    {
        _billOfLadingCrudProvider = billOfLadingCrudProvider;
        _vesselCallService = vesselCallService;
        _logger = logger;
    }

    public async Task<Dto.BillOfLadingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
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

    public async Task<PaginatedResult<Dto.BillOfLadingDto>> GetPaginatedAsync(
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

            return new PaginatedResult<Dto.BillOfLadingDto>
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

    public async Task<Dto.BillOfLadingDto> CreateAsync(Dto.BillOfLadingDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем уникальность номера BL
            var exists = await CheckBillNumberExistsAsync(dto.Num, null, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Bill of lading with number '{dto.Num}' already exists");

            // Проверяем существование связанной сущности VesselCall
            await ValidateVesselCall(dto.VesselCallId, cancellationToken);

            // Устанавливаем значения по умолчанию
            if (dto.Id == Guid.Empty)
                dto.Id = Guid.NewGuid();

            if (dto.CreatedAt == default)
                dto.CreatedAt = DateTimeOffset.UtcNow;

            if (dto.Status == default)
                dto.Status = BaseEntityStatus.New;

            if (dto.Version == 0)
                dto.Version = 1;

            // Устанавливаем BillOfLadingId для ContainerRecords
            foreach (var containerRecord in dto.ContainerRecords)
            {
                containerRecord.BillOfLadingId = dto.Id;
            }

            var entity = dto.ToEntity();
            var createdEntity = await _billOfLadingCrudProvider.CreateAsync(entity, cancellationToken);
            return createdEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bill of lading with number: {BillNumber}", dto.Num);
            throw;
        }
    }

    public async Task<Dto.BillOfLadingDto> UpdateAsync(Dto.BillOfLadingDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _billOfLadingCrudProvider.GetByIdAsync(
                dto.Id,
                cancellationToken,
                s => s.ContainerRecords);

            if (existingEntity == null)
                throw new KeyNotFoundException($"Bill of lading with ID {dto.Id} not found");

            // Проверяем уникальность номера BL, если он изменился
            if (existingEntity.Num != dto.Num)
            {
                var exists = await CheckBillNumberExistsAsync(dto.Num, dto.Id, cancellationToken);
                if (exists)
                    throw new InvalidOperationException($"Bill of lading with number '{dto.Num}' already exists");
            }

            // Проверяем существование связанной сущности VesselCall
            await ValidateVesselCall(dto.VesselCallId, cancellationToken);


            existingEntity.UpdateEntity(dto);
            await _billOfLadingCrudProvider.UpdateAsync(existingEntity, cancellationToken);
            return existingEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating bill of lading: {BillId}", dto.Id);
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

    public async Task<IEnumerable<Dto.BillOfLadingDto>> GetByVesselCallIdAsync(Guid vesselCallId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _billOfLadingCrudProvider.GetAllAsync(
                cancellationToken,
                s => s.VesselCall,
                s => s.VesselCall.Vessel);

            var filteredEntities = entities.Where(e => e.VesselCallId == vesselCallId);
            return filteredEntities.ToDtoList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bills of lading by vessel call ID: {VesselCallId}", vesselCallId);
            throw;
        }
    }

    public async Task<IEnumerable<Dto.BillOfLadingDto>> GetByContainerNumberAsync(string containerNo, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _billOfLadingCrudProvider.GetAllAsync(
                cancellationToken,
                s => s.VesselCall,
                s => s.VesselCall.Vessel,
                s => s.ContainerRecords);

            var filteredEntities = entities.Where(e =>
                e.ContainerRecords.Any(cr =>
                    cr.ContainerNo.Contains(containerNo, StringComparison.OrdinalIgnoreCase)));

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
            var entities = await _billOfLadingCrudProvider.GetAllAsync(cancellationToken);

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