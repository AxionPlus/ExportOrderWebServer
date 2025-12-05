using DocumentFormat.OpenXml.Spreadsheet;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Vessel.Mapper;
using System.Threading;
using System.Threading.Tasks;

namespace ExportOrderWebServer.Areas.ImportDocument.Vessel.Services;

public interface IVesselService
{
    Task<VesselDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<VesselDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize, CancellationToken cancellationToken,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false);
    Task<VesselDto> CreateAsync(VesselDto dto, CancellationToken cancellationToken=default);
    Task<VesselDto> UpdateAsync(VesselDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}



public class VesselService : IVesselService
{
    private readonly ICrudProvider<VesselBaseEntity> _vesselCrudProvider;
    private readonly ILogger<VesselService> _logger;

    public VesselService(
        ICrudProvider<VesselBaseEntity> vesselCrudProvider,
        ILogger<VesselService> logger)
    {
        _vesselCrudProvider = vesselCrudProvider;
        _logger = logger;
    }

    public async Task<VesselDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _vesselCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new NullReferenceException($"Vessel not found ");

            return entity?.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vessel by ID: {VesselId}", id);
            throw;
        }
    }

    public async Task<PaginatedResult<VesselDto>> GetPaginatedAsync(int pageNumber,
        int pageSize, CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        try
        {
            var entities = await _vesselCrudProvider.GetPaginatedAsync(pageNumber,
                 pageSize, cancellationToken,
                 searchTerm,
                 sortBy,
                 sortDesc);

            return new()
            {
                Items = entities.Items.ToDtoList(),
                TotalCount= entities.TotalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = entities.TotalPages,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all vessels");
            throw;
        }
    }

    public async Task<VesselDto> CreateAsync(VesselDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
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
            var createdEntity = await _vesselCrudProvider.CreateAsync(entity, cancellationToken);
            return createdEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vessel");
            throw;
        }
    }

    public async Task<VesselDto> UpdateAsync(VesselDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _vesselCrudProvider.GetByIdAsync(dto.Id, cancellationToken);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Vessel with ID {dto.Id} not found");

            // Увеличиваем версию
            dto.Version = existingEntity.Version + 1;
            dto.UpdatedAt = DateTimeOffset.UtcNow;

            existingEntity.UpdateEntity(dto);
            await _vesselCrudProvider.UpdateAsync(existingEntity, cancellationToken);
            return existingEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vessel: {VesselId}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _vesselCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            await _vesselCrudProvider.DeleteAsync(entity, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vessel: {VesselId}", id);
            throw;
        }
    }
}
