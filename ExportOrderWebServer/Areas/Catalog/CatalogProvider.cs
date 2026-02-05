namespace ExportOrderWebServer.Areas.Catalog;

public interface ICatalogProvider
{
    Task<IEnumerable<string>> GetCarrierNames();
    Task<IEnumerable<string>> GetVesselNames();
    Task<IEnumerable<string>> GetLocationNames();
    Task<IEnumerable<string>> GetTerminalNames();
    Task<IEnumerable<string>> GetCustomsOfficesCodes();
    Task<IEnumerable<string>> GetCustomsOfficesNames();
    Task<IEnumerable<string>> GetVoyages();
    Task<IEnumerable<CustomsCatalog>> GetCustomsOfficesAsync();    
}

public class CatalogProvider : ICatalogProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    public CatalogProvider (IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CustomsCatalog>> GetCustomsOfficesAsync()
    {
        await using var db = await _dbContext.CreateDbContextAsync();

        return await db.CustomsOffices.AsNoTracking()
                                      .Where(s => !string.IsNullOrWhiteSpace(s.Code) && !string.IsNullOrWhiteSpace(s.OfficeShort))
                                      .OrderBy(s => s.Code)
                                      .ToArrayAsync()
            ?? Enumerable.Empty<CustomsCatalog>();

        //using (var _db = _dbContext.CreateDbContextAsync())
        //{
        //    var db = await _db;

        //    IEnumerable<CustomsCatalog> Items = Enumerable.Empty<CustomsCatalog>();

        //    Items = await db.CustomsOffices.AsNoTracking()
        //                                   .Where(s => !string.IsNullOrWhiteSpace(s.Code) && !string.IsNullOrWhiteSpace(s.OfficeShort))
        //                                   .OrderBy(s => s.Code)
        //                                   .ToArrayAsync();

        //    return Items;
        //}
    }

    public async Task<IEnumerable<string>> GetLocationNames()
    {
        await using var db = await _dbContext.CreateDbContextAsync();

        return await db.Locations.Where(s => !string.IsNullOrWhiteSpace(s.NameEn)).Select(s => s.NameEn!).ToListAsync()
            ?? Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<string>> GetTerminalNames()
    {
        await using var db = await _dbContext.CreateDbContextAsync();

        return await db.Terminals.Where(s => !string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Name!).ToListAsync()
            ?? Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<string>> GetCustomsOfficesCodes()
    {
        await using var db = await _dbContext.CreateDbContextAsync();

        return await db.CustomsOffices.Where(s => !string.IsNullOrWhiteSpace(s.Code)).Select(s => s.Code!).ToListAsync()
            ?? Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<string>> GetCustomsOfficesNames()
    {
        await using var db = await _dbContext.CreateDbContextAsync();

        return await db.CustomsOffices.Where(s => !string.IsNullOrWhiteSpace(s.OfficeShort)).Select(s => s.OfficeShort!).ToListAsync()
            ?? Enumerable.Empty<string>();
    }
    
    public async Task<IEnumerable<string>> GetVesselNames()
    {
        await using var db = await _dbContext.CreateDbContextAsync();
                
        return await db.Vessels.Where(s => !string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Name!).ToListAsync()
            ?? Enumerable.Empty<string>();
    }
    
    public async Task<IEnumerable<string>> GetVoyages()
    {
        await using var db = await _dbContext.CreateDbContextAsync();

        //return await db.VesselCalls.OrderByDescending(s => s.CreateTime).Select(s => s.VoyageNo).ToArrayAsync()
        //return await db.VesselCalls.Select(s => s.VoyageNo).ToArrayAsync()
        return await db.VesselCalls.OrderByDescending(s => s.CreateTime).Select(s => s.VoyageNo).Distinct().ToArrayAsync()
            ?? Enumerable.Empty<string>();
    }

    public async Task<IEnumerable<string>> GetCarrierNames()
    {
        await using var db = await _dbContext.CreateDbContextAsync();

        return await db.Carriers.Where(s => !string.IsNullOrWhiteSpace(s.NameEn)).Select(s => s.NameEn!).ToListAsync()
            ?? Enumerable.Empty<string>();
    }
}
