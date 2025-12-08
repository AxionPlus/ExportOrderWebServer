using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Services;
using ExportOrderWebServer.Areas.ImportDocument.Port.Services;

namespace ExportOrderWebServer.Areas.ImportDocument.VesselCall.Services;

public interface IVesselCallService
{
    Task<VesselCallDto> GetByIdForArrivalNoticeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VesselCallDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<VesselCallDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize, CancellationToken cancellationToken,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false);
    Task<VesselCallDto> CreateAsync(VesselCallDto dto, CancellationToken cancellationToken = default);
    Task<VesselCallDto> UpdateAsync(VesselCallDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<VesselCallDto>> GetByVesselIdAsync(Guid vesselId, CancellationToken cancellationToken = default);
    Task<IEnumerable<VesselCallDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<bool> CheckVoyageNumberExistsAsync(string voyageNo, Guid? excludeId = null, CancellationToken cancellationToken = default);
}

public class VesselCallService : IVesselCallService
{
    private readonly ICrudProvider<VesselCallBaseEntity> _vesselCallCrudProvider;
    private readonly IVesselService _vesselService;
    private readonly IPortService _portService;
    private readonly ILogger<VesselCallService> _logger;

    public VesselCallService(
        ICrudProvider<VesselCallBaseEntity> vesselCallCrudProvider,
        IVesselService vesselService,
        IPortService portService,
        ILogger<VesselCallService> logger)
    {
        _vesselCallCrudProvider = vesselCallCrudProvider;
        _vesselService = vesselService;
        _portService = portService;
        _logger = logger;
    }

    public async Task<VesselCallDto> GetByIdForArrivalNoticeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Получаем IQueryable
            var query = _vesselCallCrudProvider.GetAllAsync(
                s => s.Vessel,
                s => s.Terminal,
                s => s.PortOfLoading
            );

            // Добавляем ThenInclude для коллекции BillOfLadings
            query = query
                .Include(s => s.BillOfLadings)
                .ThenInclude(b => b.Pol)
                .Include(s => s.BillOfLadings)
                .ThenInclude(b => b.TsPort)
                .Include(s => s.BillOfLadings)
                .ThenInclude(b => b.ContainerRecords);

            // Выполняем запрос с фильтром
            var entity = await query.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException($"Vessel call with ID {id} not found");

            return entity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vessel call by ID: {VesselCallId}", id);
            throw;
        }
    }

    public async Task<VesselCallDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _vesselCallCrudProvider.GetByIdAsync(id, cancellationToken, s => s.Terminal, s => s.PortOfLoading, s => s.Vessel);
            if (entity == null)
                throw new KeyNotFoundException($"Vessel call with ID {id} not found");

            return entity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vessel call by ID: {VesselCallId}", id);
            throw;
        }
    }

    public async Task<PaginatedResult<VesselCallDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        try
        {
            var entities = await _vesselCallCrudProvider.GetPaginatedAsync(
                pageNumber,
                pageSize,
                cancellationToken,
                searchTerm,
                sortBy,
                sortDesc, s => s.Terminal, s => s.PortOfLoading, s => s.Vessel);

            return new PaginatedResult<VesselCallDto>
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
            _logger.LogError(ex, "Error getting paginated vessel calls");
            throw;
        }
    }

    public async Task<VesselCallDto> CreateAsync(VesselCallDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем уникальность номера рейса
            var exists = await CheckVoyageNumberExistsAsync(dto.VoyageNo, null, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Vessel call with voyage number '{dto.VoyageNo}' already exists");

            // Проверяем существование связанных сущностей
            await ValidateRelatedEntities(dto, cancellationToken);

            if (dto.Id == Guid.Empty)
            {
                dto.Id = Guid.NewGuid();
            }

            if (dto.CreatedAt == default)
            {
                dto.CreatedAt = DateTimeOffset.UtcNow;
            }

            if (dto.Status == default)
            {
                dto.Status = BaseEntityStatus.New;
            }

            if (dto.Version == 0)
            {
                dto.Version = 1;
            }

            var entity = dto.ToEntity();
            var createdEntity = await _vesselCallCrudProvider.CreateAsync(entity, cancellationToken);
            return createdEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vessel call with voyage number: {VoyageNo}", dto.VoyageNo);
            throw;
        }
    }

    public async Task<VesselCallDto> UpdateAsync(VesselCallDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _vesselCallCrudProvider.GetByIdAsync(dto.Id, cancellationToken);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Vessel call with ID {dto.Id} not found");

            // Проверяем уникальность номера рейса, если он изменился
            if (existingEntity.VoyageNo != dto.VoyageNo)
            {
                var exists = await CheckVoyageNumberExistsAsync(dto.VoyageNo, dto.Id, cancellationToken);
                if (exists)
                    throw new InvalidOperationException($"Vessel call with voyage number '{dto.VoyageNo}' already exists");
            }

            // Проверяем существование связанных сущностей
            await ValidateRelatedEntities(dto, cancellationToken);

            // Увеличиваем версию
            dto.Version = existingEntity.Version + 1;
            dto.UpdatedAt = DateTimeOffset.UtcNow;

            existingEntity.UpdateEntity(dto);
            await _vesselCallCrudProvider.UpdateAsync(existingEntity, cancellationToken);
            return existingEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vessel call: {VesselCallId}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _vesselCallCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            // Проверяем, есть ли связанные коносаменты
            if (entity.BillOfLadings != null && entity.BillOfLadings.Any())
                throw new InvalidOperationException($"Cannot delete vessel call with ID {id} because it has related bill of ladings");

            await _vesselCallCrudProvider.DeleteAsync(entity, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vessel call: {VesselCallId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<VesselCallDto>> GetByVesselIdAsync(Guid vesselId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = _vesselCallCrudProvider.GetAllAsync();
            var filteredEntities = await entities.Where(e => e.VesselId == vesselId).ToListAsync(cancellationToken);
            return filteredEntities.ToDtoList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vessel calls by vessel ID: {VesselId}", vesselId);
            throw;
        }
    }

    public async Task<IEnumerable<VesselCallDto>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = _vesselCallCrudProvider.GetAllAsync(s => s.Vessel, s => s.PortOfLoading, s => s.Terminal).AsSplitQuery().AsQueryable();
            var filteredEntities = await entities.Where(e => e.ETA >= startDate && e.ETA <= endDate).ToListAsync(cancellationToken);
            return filteredEntities.ToDtoList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vessel calls by date range: {StartDate} to {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<bool> CheckVoyageNumberExistsAsync(string voyageNo, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = _vesselCallCrudProvider.GetAllAsync();

            if (excludeId.HasValue)
            {
                return await entities.AnyAsync(e => e.VoyageNo == voyageNo && e.Id != excludeId.Value, cancellationToken);
            }

            return await entities.AnyAsync(e => e.VoyageNo == voyageNo, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking voyage number existence: {VoyageNo}", voyageNo);
            throw;
        }
    }

    private async Task ValidateRelatedEntities(VesselCallDto dto, CancellationToken cancellationToken)
    {
        // Проверяем существование судна
        try
        {
            await _vesselService.GetByIdAsync(dto.VesselId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"Vessel with ID {dto.VesselId} does not exist");
        }

        // Проверяем существование порта погрузки
        try
        {
            await _portService.GetByIdAsync(dto.PortOfLoadingId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            throw new ArgumentException($"Port of loading with ID {dto.PortOfLoadingId} does not exist");
        }

        // TODO: Добавить проверку Terminal после создания TerminalService
    }
}