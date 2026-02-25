using DocumentFormat.OpenXml.Wordprocessing;
using ExportOrderEntites.BillofLading.Dto;
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Dto;
using ExportOrderWebServer.Areas.ImportDocument.BillOfLading.Mapper;
using ExportOrderWebServer.Areas.ImportDocument.Extensions;
using ExportOrderWebServer.Areas.ImportDocument.Port.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Port.Services;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.VesselCall.Dto;
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
    Task DeleteAsync(BillOfLadingBaseDto billOfLadingDto, CancellationToken cancellationToken = default);
    Task<IEnumerable<BillOfLadingBaseDto>> GetByVesselCallIdAsync(Guid vesselCallId, CancellationToken cancellationToken = default);
    Task<IEnumerable<BillOfLadingBaseDto>> GetByContainerNumberAsync(string containerNo, CancellationToken cancellationToken = default);
    Task<bool> CheckBillNumberExistsAsync(string billNo, Guid? excludeId = null, CancellationToken cancellationToken = default);

    Task UpdateAsync(BillOfLadingContainerRecordBaseDto baseDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(BillOfLadingContainerRecordBaseDto baseDto, CancellationToken cancellationToken = default);


    Task TranslateAsync(IEnumerable<BillOfLadingBaseDto> billOfLadingsDto, CancellationToken cancellationToken = default);
    Task TranshipmentDataAsync(IEnumerable<BillOfLadingBaseDto> billOfLadingsDto, CancellationToken cancellationToken = default);
    Task UpdateAsync(IEnumerable<BillOfLadingBaseDto> billOfLadingDtos, VesselCallDto? vesselCall, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> CheckBillNumbersExistsAsync(IEnumerable<string> parsedBillsNum, CancellationToken cancellationToken = default);
}

public class BillOfLadingService : IBillOfLadingService
{
    private readonly ICrudProvider<BillOfLadingBaseEntity> _billOfLadingCrudProvider;
    private readonly IVesselCallService _vesselCallService;
    private readonly IPortService _portService;
    private readonly ILogger<BillOfLadingService> _logger;
    protected readonly IDbContextFactory<ApplicationDbContext> _context;
    public BillOfLadingService(
        ICrudProvider<BillOfLadingBaseEntity> billOfLadingCrudProvider,
        IDbContextFactory<ApplicationDbContext> context,
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
                s => s.TsPort,
                s => s.Pol,
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
      
            // Проверяем уникальность номера BL
            var exists = await CheckBillNumberExistsAsync(baseDto.Num, null, cancellationToken);
            if (exists)
                throw new InvalidOperationException($"Bill of lading with number '{baseDto.Num}' already exists");

            // Проверяем существование связанной сущности VesselCall
            await ValidateVesselCall(baseDto.VesselCallId, cancellationToken);



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

    public async Task DeleteAsync(BillOfLadingBaseDto baseDto, CancellationToken cancellationToken = default)
    {

        await using var db = await _context.CreateDbContextAsync(cancellationToken);

        var entity = await db.Set<BillOfLadingBaseEntity>().Include(s => s.ContainerRecords).FirstAsync(s => s.Id == baseDto.Id, cancellationToken: cancellationToken);

        if (entity.Timestamp != baseDto.Timestamp)
        {
            throw new ArgumentException($"has been changed by another User");
        }

        foreach (var containerRecord in entity.ContainerRecords)
            db.Entry(containerRecord).State = EntityState.Deleted;


        db.Entry(entity).State = EntityState.Deleted;

        await db.SaveChangesAsync(cancellationToken);


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
                s => s.ContainerRecords)
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
        await using var db = await _context.CreateDbContextAsync(cancellationToken);

        var entity = await db.Set<BillOfLadingContainerRecordBaseEntity>().FirstAsync(s => s.Id == baseDto.Id, cancellationToken: cancellationToken);

        if (entity.Timestamp != baseDto.Timestamp)
        {
            throw new ArgumentException($"has been changed by another User");
        }

        entity.UpdateEntity(baseDto);
        db.Set<BillOfLadingContainerRecordBaseEntity>().Update(entity);
        await db.SaveChangesAsync(cancellationToken);


    }

    public async Task DeleteAsync(BillOfLadingContainerRecordBaseDto baseDto, CancellationToken cancellationToken = default)
    {
        await using var db = await _context.CreateDbContextAsync(cancellationToken);

        var entity = await db.Set<BillOfLadingContainerRecordBaseEntity>().FirstAsync(s => s.Id == baseDto.Id, cancellationToken: cancellationToken);

        if (entity.Timestamp != baseDto.Timestamp)
        {
            throw new ArgumentException($"has been changed by another User");
        }

        db.Entry(entity).State = EntityState.Deleted;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task TranslateAsync(IEnumerable<BillOfLadingBaseDto> billOfLadingsDto, CancellationToken cancellationToken = default)
    {
        await using var db = await _context.CreateDbContextAsync(cancellationToken);

        Guid? vesselCallId = null;

        foreach (var billOfLadingDto in billOfLadingsDto)
        {

            var entity = await db.Set<BillOfLadingBaseEntity>()
                .Include(s => s.VesselCall).ThenInclude(s => s.Terminal)
                .Include(s => s.ContainerRecords).FirstOrDefaultAsync(s => s.Num == billOfLadingDto.Num, cancellationToken: cancellationToken);
            if (entity == null) continue;


            if (entity.VesselCall.Terminal.Name == "NUTEP")
            {
                entity.ShipperNameRu = entity.ShipperName;
            }
            if (entity.VesselCall.Terminal.Name == "NLE")
            {
                entity.ShipperNameRu =  Transliteration.ToCyrillicAdvanced(entity.ShipperName);
            }


            entity.ConsigneeNameRu = billOfLadingDto.ConsigneeNameRu.FixCsvContent().ToUpper();
            entity.ConsigneeAddressRu = billOfLadingDto.ConsigneeAddressRu.FixCsvContent().ToUpper();
            entity.ConsigneeCountryRu = billOfLadingDto.ConsigneeCountryRu.FixCsvContent().ToUpper();

            entity.CargoDescriptionRu = billOfLadingDto.CargoDescriptionRu.FixCsvContent().ToUpper().Replace(";", ",");
            entity.CustomsMode = billOfLadingDto.CustomsMode.FixCsvContent().ToUpper();


            if (billOfLadingDto.ContainerRecords.Any())
            {
                foreach (var containerRecordDto in billOfLadingDto.ContainerRecords)
                {
                    var containerRecord =
                        entity.ContainerRecords.FirstOrDefault(s => s.ContainerNo == containerRecordDto.ContainerNo);
                    if (containerRecord == null) continue;

                    containerRecord.CargoDescriptionRu = containerRecordDto.CargoDescriptionRu.FixCsvContent().ToUpper().Replace(";", ",");
                }

                entity.CargoDescriptionRu = "*";
            }
            else
            {
                foreach (var containerRecord in entity.ContainerRecords)
                    containerRecord.CargoDescriptionRu = billOfLadingDto.CargoDescriptionRu.FixCsvContent().ToUpper().Replace(";", ",");
            }


            vesselCallId = entity.VesselCallId;


             await db.SaveChangesAsync(cancellationToken);
        }


        var VesselCall = await db.Set<VesselCallBaseEntity>()
            .FirstAsync(s => s.Id == vesselCallId, cancellationToken: cancellationToken);

        VesselCall.TranslateUpdateTime = DateTime.Now;

    }

    public async Task TranshipmentDataAsync(IEnumerable<BillOfLadingBaseDto> billOfLadingsDto,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _context.CreateDbContextAsync(cancellationToken);
        foreach (var billOfLadingDto in billOfLadingsDto)
        {
            var entity = await db.Set<BillOfLadingBaseEntity>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Num == billOfLadingDto.Num, cancellationToken: cancellationToken);

            if (entity == null) continue;
            if (billOfLadingDto.TsPort == null) continue;

            var tsPort = await db.Set<PortBaseEntity>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.IsoCode == billOfLadingDto.TsPort.IsoCode,
                    cancellationToken: cancellationToken) ?? await db.Set<PortBaseEntity>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.NameEn == billOfLadingDto.TsPort.IsoCode.ToUpper(),
                    cancellationToken: cancellationToken);

            if (tsPort == null) continue;

            entity.TsDate = billOfLadingDto.TsDate;
            entity.TsPortId = tsPort.Id;

            db.Entry(entity).State = EntityState.Modified;
            var bug = db.ChangeTracker.DebugView.LongView;
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }
    }

    public async Task UpdateAsync(IEnumerable<BillOfLadingBaseDto> billOfLadingDtos, VesselCallDto? vesselCall,
        CancellationToken cancellationToken = default)
    {
        await using var db = await _context.CreateDbContextAsync(cancellationToken);

        foreach (var billOfLadingDto in billOfLadingDtos)
        {
            var entity = await db.Set<BillOfLadingBaseEntity>().AsNoTracking()
                .FirstOrDefaultAsync(s => s.Num == billOfLadingDto.Num, cancellationToken: cancellationToken);

            if (entity == null) continue;

            // Обновляем основную сущность
            entity.ReUpdateEntity(billOfLadingDto);
            var pol = await _portService.GetByIsoCodeAsync(billOfLadingDto.Pol?.IsoCode, cancellationToken);

            if (pol != null)
                entity.PolId = pol.Id;
            else
            {
                var newPol = new PortDto()
                {
                    NameRu = billOfLadingDto.Pol.IsoCode,
                    NameEn = billOfLadingDto.Pol.IsoCode,
                    CountryEn = billOfLadingDto.Pol.IsoCode,
                    CountryRu = billOfLadingDto.Pol.IsoCode,
                    IsoCode = billOfLadingDto.Pol.IsoCode,
                    PikYugIsoCode = billOfLadingDto.Pol.IsoCode,
                    HandledBySystem = true,
                };

                var Pol = await _portService.CreateAsync(newPol, cancellationToken);
                entity.PolId = Pol.Id;

            }

            db.Entry(entity).State = EntityState.Modified;
            var bug = db.ChangeTracker.DebugView.LongView;
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();


            // Удаляем старые записи контейнеров
            var entityContainerRecords = await db.Set<BillOfLadingContainerRecordBaseEntity>().AsNoTracking()
                .Where(s => s.BillOfLadingBaseEntityId == entity.Id).ToListAsync(cancellationToken);

            if (entityContainerRecords.Any())
                foreach (var containerRecord in entityContainerRecords)
                {
                    db.Entry(containerRecord).State = EntityState.Deleted;

                    //var currentContainerIds = entity.ContainerRecords
                    //    .Select(c => c.Id)
                    //    .ToList();
                    //await db.Set<BillOfLadingContainerRecordBaseEntity>()
                    //    .Where(c => currentContainerIds.Contains(c.Id))
                    //    .ExecuteDeleteAsync(cancellationToken);
                }

            bug = db.ChangeTracker.DebugView.LongView;
            await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();


            // Если в DTO есть новые контейнеры, добавляем их
            if (billOfLadingDto.ContainerRecords.Any())
            {
                foreach (var containerDto in billOfLadingDto.ContainerRecords)
                {
                    var containerEntity = containerDto.ToEntity();
                    entity.ContainerRecords.Add(containerEntity);
                    containerEntity.BillOfLadingBaseEntityId = entity.Id;
                    db.Entry(containerEntity).State = EntityState.Added;

                }
            }
            db.Entry(entity).State = EntityState.Modified;
            bug = db.ChangeTracker.DebugView.LongView;
            await db.SaveChangesAsync(cancellationToken);
        }


    }

    public async Task<IEnumerable<string>> CheckBillNumbersExistsAsync(IEnumerable<string> parsedBillsNum, CancellationToken cancellationToken = default)
    {
        await using var db = await _context.CreateDbContextAsync(cancellationToken);

        var list = await db.Set<BillOfLadingBaseEntity>().AsNoTracking().Where(s => parsedBillsNum.Any(n => n == s.Num))
            .Select(s => s.Num).ToListAsync(cancellationToken);

        return list;
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