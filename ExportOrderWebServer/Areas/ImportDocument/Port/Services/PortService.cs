using DocumentFormat.OpenXml.Office2010.Excel;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Port.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Provider;

namespace ExportOrderWebServer.Areas.ImportDocument.Port.Services;

public interface IPortService
{
    Task<PortDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PortDto?> GetByIsoCodeAsync(string? isoCode, CancellationToken cancellationToken = default);
    Task<PaginatedResult<PortDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize, CancellationToken cancellationToken,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false);
    Task<PortDto> CreateAsync(PortDto dto, CancellationToken cancellationToken = default);
    Task<PortDto> UpdateAsync(PortDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class PortService : IPortService
{
    private readonly ICrudProvider<PortBaseEntity> _portCrudProvider;
    private readonly ILogger<PortService> _logger;

    public PortService(
        ICrudProvider<PortBaseEntity> portCrudProvider,
        ILogger<PortService> logger)
    {
        _portCrudProvider = portCrudProvider;
        _logger = logger;
    }

    public async Task<PortDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _portCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"Port with ID {id} not found");

            return entity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting port by ID: {PortId}", id);
            throw;
        }
    }

    public async Task<PortDto?> GetByIsoCodeAsync(string? isoCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(isoCode))
            return null;

        var query = _portCrudProvider.GetAllAsync();
        query = query.Where(s => s.IsoCode == isoCode);

        var entity = await query.FirstOrDefaultAsync(cancellationToken);

        return entity?.ToDto();
    }

    public async Task<PaginatedResult<PortDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        try
        {
            var entities = await _portCrudProvider.GetPaginatedAsync(
                pageNumber,
                pageSize,
                cancellationToken,
                searchTerm,
                sortBy,
                sortDesc);

            return new PaginatedResult<PortDto>
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
            _logger.LogError(ex, "Error getting paginated ports");
            throw;
        }
    }

    public async Task<PortDto> CreateAsync(PortDto dto, CancellationToken cancellationToken = default)
    {
        try
        {

            var entity = dto.ToEntity();
            var createdEntity = await _portCrudProvider.CreateAsync(entity, cancellationToken);
            return createdEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating port");
            throw;
        }
    }

    public async Task<PortDto> UpdateAsync(PortDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _portCrudProvider.GetByIdAsync(dto.Id, cancellationToken);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Port with ID {dto.Id} not found");

            if (existingEntity.Timestamp != dto.Timestamp)
                throw new NullReferenceException($"has been changed by another user");

            existingEntity.UpdateEntity(dto);
            await _portCrudProvider.UpdateAsync(existingEntity, cancellationToken);
            return existingEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating port: {PortId}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _portCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            await _portCrudProvider.DeleteAsync(entity, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting port: {PortId}", id);
            throw;
        }
    }
}