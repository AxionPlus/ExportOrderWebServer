namespace ExportOrderWebServer.Areas.Catalog;

public interface ICatalogProvider
{
    Task<IEnumerable<string>> GetLocationNames();
    Task<IEnumerable<string>> GetTerminalNames();
    Task<IEnumerable<string>> GetCustomsOfficesCodes();
    Task<IEnumerable<string>> GetCustomsOfficesNames();
    Task<IEnumerable<CustomsCatalog>> GetCustomsOfficesAsync();
}

public class CatalogProvider : ICatalogProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public CatalogProvider (IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<string>> GetLocationNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            IEnumerable<string> Items = Enumerable.Empty<string>();

            Items = await db.Locations.Where(s => !string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Name!).OrderBy(n => n).ToArrayAsync();

            return Items;
        }
    }

    public async Task<IEnumerable<string>> GetTerminalNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            IEnumerable<string> Items = Enumerable.Empty<string>();

            Items = await db.Terminals.Where(s => !string.IsNullOrWhiteSpace(s.Name)).Select(s => s.Name!).OrderBy(n => n).ToArrayAsync();

            return Items;
        }
    }

    public async Task<IEnumerable<string>> GetCustomsOfficesCodes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            IEnumerable<string> Items = Enumerable.Empty<string>();

            Items = await db.CustomsOffices.Where(s => !string.IsNullOrWhiteSpace(s.Code)).Select(s => s.Code!).OrderBy(c => c).ToArrayAsync();

            return Items;
        }
    }

    public async Task<IEnumerable<string>> GetCustomsOfficesNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            IEnumerable<string> Items = Enumerable.Empty<string>();

            Items = await db.CustomsOffices.Where(s => !string.IsNullOrWhiteSpace(s.OfficeShort)).Select(s => s.OfficeShort!).OrderBy(of => of).ToArrayAsync();

            return Items;
        }
    }

    public async Task<IEnumerable<CustomsCatalog>> GetCustomsOfficesAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            IEnumerable<CustomsCatalog> Items = Enumerable.Empty<CustomsCatalog>();

            Items = await db.CustomsOffices.AsNoTracking()
                                           .Where(s => !string.IsNullOrWhiteSpace(s.Code) && !string.IsNullOrWhiteSpace(s.OfficeShort))
                                           .OrderBy(s => s.Code)
                                           .ToArrayAsync();

            return Items;
        }
    }
}
