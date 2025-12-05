using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Terminal.Mapper;

namespace ExportOrderWebServer.Areas.ImportDocument.Terminal.Services;

public interface ITerminalService
{
    Task<TerminalDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TerminalDto> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<PaginatedResult<TerminalDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize, CancellationToken cancellationToken,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false);
    Task<TerminalDto> CreateAsync(TerminalDto dto, CancellationToken cancellationToken = default);
    Task<TerminalDto> UpdateAsync(TerminalDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<TerminalDto>> GetByCustomsPostAsync(string customsPost, CancellationToken cancellationToken = default);
}

public class TerminalService : ITerminalService
{
    private readonly ICrudProvider<TerminalBaseEntity> _terminalCrudProvider;
    private readonly ILogger<TerminalService> _logger;

    public TerminalService(
        ICrudProvider<TerminalBaseEntity> terminalCrudProvider,
        ILogger<TerminalService> logger)
    {
        _terminalCrudProvider = terminalCrudProvider;
        _logger = logger;
    }

    public async Task<TerminalDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _terminalCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"Terminal with ID {id} not found");

            return entity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting terminal by ID: {TerminalId}", id);
            throw;
        }
    }

    public async Task<TerminalDto> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _terminalCrudProvider.GetAllAsync(cancellationToken);
            var entity = entities.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (entity == null)
                throw new KeyNotFoundException($"Terminal with name '{name}' not found");

            return entity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting terminal by name: {TerminalName}", name);
            throw;
        }
    }

    public async Task<PaginatedResult<TerminalDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        try
        {
            var entities = await _terminalCrudProvider.GetPaginatedAsync(
                pageNumber,
                pageSize,
                cancellationToken,
                searchTerm,
                sortBy,
                sortDesc);

            return new PaginatedResult<TerminalDto>
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
            _logger.LogError(ex, "Error getting paginated terminals");
            throw;
        }
    }

    public async Task<TerminalDto> CreateAsync(TerminalDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем уникальность имени терминала
            var exists = await ExistsByNameAsync(dto.Name, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Terminal with name '{dto.Name}' already exists");

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
            var createdEntity = await _terminalCrudProvider.CreateAsync(entity, cancellationToken);
            return createdEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating terminal with name: {TerminalName}", dto.Name);
            throw;
        }
    }

    public async Task<TerminalDto> UpdateAsync(TerminalDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _terminalCrudProvider.GetByIdAsync(dto.Id, cancellationToken);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Terminal with ID {dto.Id} not found");

            // Проверяем уникальность имени, если оно изменилось
            if (!existingEntity.Name.Equals(dto.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await ExistsByNameAsync(dto.Name, cancellationToken);
                if (exists)
                    throw new InvalidOperationException($"Terminal with name '{dto.Name}' already exists");
            }

            // Увеличиваем версию
            dto.Version = existingEntity.Version + 1;
            dto.UpdatedAt = DateTimeOffset.UtcNow;

            existingEntity.UpdateEntity(dto);
            await _terminalCrudProvider.UpdateAsync(existingEntity, cancellationToken);
            return existingEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating terminal: {TerminalId}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _terminalCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            // TODO: Проверить, нет ли связанных VesselCall записей
            // Если есть связанные VesselCall, нельзя удалять терминал

            await _terminalCrudProvider.DeleteAsync(entity, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting terminal: {TerminalId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _terminalCrudProvider.GetAllAsync(cancellationToken);
            return entities.Any(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking terminal existence by name: {TerminalName}", name);
            throw;
        }
    }

    public async Task<IEnumerable<TerminalDto>> GetByCustomsPostAsync(string customsPost, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _terminalCrudProvider.GetAllAsync(cancellationToken);
            var filteredEntities = entities
                .Where(e => e.CustomsPost.Equals(customsPost, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return filteredEntities.ToDtoList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting terminals by customs post: {CustomsPost}", customsPost);
            throw;
        }
    }
}