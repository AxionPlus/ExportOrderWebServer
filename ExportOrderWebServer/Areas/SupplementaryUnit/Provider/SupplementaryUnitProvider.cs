namespace ExportOrderWebServer.Areas.SupplementaryUnit.Provider;

public interface ISupplementaryUnitProvider
{    
    Task<SupplementaryUnitCatalog?> GetItemAsync(ushort code);
    Task<IEnumerable<SupplementaryUnitCatalog>?> GetItemsAsync();    
    Task<bool> IsSupplementaryUnitExists(ushort code);
    Task<AppObjectResponse> NewItemAsync(SupplementaryUnitCatalog item);
}

public class SupplementaryUnitProvider : ISupplementaryUnitProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public SupplementaryUnitProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<SupplementaryUnitCatalog?> GetItemAsync(ushort code)
    {
        using var _db = _dbContext.CreateDbContextAsync();
        var db = await _db;

        var Item = await db.SupplementaryUnits.AsNoTracking().FirstOrDefaultAsync(x => x.Code == code);

        return Item;
    }

    public async Task<IEnumerable<SupplementaryUnitCatalog>?> GetItemsAsync()
    {
        using var _db = _dbContext.CreateDbContextAsync();
        var db = await _db;

        var Items = await db.SupplementaryUnits.AsNoTracking().ToArrayAsync();

        if (Items is not null && Items.Any())
            return Items.OrderBy(x => x.Code);

        return null;
    }    

    public async Task<bool> IsSupplementaryUnitExists(ushort code)
    {
        using var _db = _dbContext.CreateDbContextAsync();
        var db = await _db;

        return db.SupplementaryUnits.AsNoTracking().Any(x => x.Code == code);
    }

    public async Task<AppObjectResponse> NewItemAsync(SupplementaryUnitCatalog item)
    {
        appObjResponse = new();

        using var _db = _dbContext.CreateDbContextAsync();

        var db = await _db;

        try
        {
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            var dbItems = await db.SupplementaryUnits.AsNoTracking().ToArrayAsync();

            /// check an Existing item
            var existedItemDb = dbItems.Where(s => s.Code == item.Code || s.ShortName == item.ShortName || s.FullName == item.FullName).FirstOrDefault();

            if (existedItemDb is not null)
            {
                appObjResponse.ErrorAdd($"Item exists already: Code - {existedItemDb.Code}, Name - {existedItemDb.ShortName}");
                return appObjResponse;
            }

            item.CreateUser = User!;
            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); return appObjResponse; }
    }
}