using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Customer.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Customer.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Provider;

namespace ExportOrderWebServer.Areas.ImportDocument.Customer.Services;

public interface ICustomerService
{
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedResult<CustomerDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize, CancellationToken cancellationToken,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false);
    Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken cancellationToken = default);
    Task<CustomerDto> UpdateAsync(CustomerDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}

public class CustomerService : ICustomerService
{
    private readonly ICrudProvider<CustomerBaseEntity> _customerCrudProvider;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICrudProvider<CustomerBaseEntity> customerCrudProvider,
        ILogger<CustomerService> logger)
    {
        _customerCrudProvider = customerCrudProvider;
        _logger = logger;
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _customerCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found");

            return entity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by ID: {CustomerId}", id);
            throw;
        }
    }



    public async Task<PaginatedResult<CustomerDto>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDesc = false)
    {
        try
        {
            var entities = await _customerCrudProvider.GetPaginatedAsync(
                pageNumber,
                pageSize,
                cancellationToken,
                searchTerm,
                sortBy,
                sortDesc);

            return new PaginatedResult<CustomerDto>
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
            _logger.LogError(ex, "Error getting paginated customers");
            throw;
        }
    }

    public async Task<CustomerDto> CreateAsync(CustomerDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем уникальность кода
            var exists = await ExistsByCodeAsync(dto.Code, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Customer with code '{dto.Code}' already exists");

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
            var createdEntity = await _customerCrudProvider.CreateAsync(entity, cancellationToken);
            return createdEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer with code: {CustomerCode}", dto.Code);
            throw;
        }
    }

    public async Task<CustomerDto> UpdateAsync(CustomerDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingEntity = await _customerCrudProvider.GetByIdAsync(dto.Id, cancellationToken);
            if (existingEntity == null)
                throw new KeyNotFoundException($"Customer with ID {dto.Id} not found");

            // Проверяем уникальность кода, если он изменился
            if (existingEntity.Code != dto.Code)
            {
                var exists = await ExistsByCodeAsync(dto.Code, cancellationToken);
                if (exists)
                    throw new InvalidOperationException($"Customer with code '{dto.Code}' already exists");
            }

            // Увеличиваем версию
            dto.Version = existingEntity.Version + 1;
            dto.UpdatedAt = DateTimeOffset.UtcNow;

            existingEntity.UpdateEntity(dto);
            await _customerCrudProvider.UpdateAsync(existingEntity, cancellationToken);
            return existingEntity.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer: {CustomerId}", dto.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _customerCrudProvider.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return false;

            await _customerCrudProvider.DeleteAsync(entity, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer: {CustomerId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _customerCrudProvider.GetAllAsync(cancellationToken);
            return entities.Any(e => e.Code == code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking customer existence by code: {CustomerCode}", code);
            throw;
        }
    }
}