namespace ExportOrderWebServer.Areas.SupplementaryUnit.Provider;

public interface ISupplementaryUnitProvider
{    
    Task<IEnumerable<SupplementaryUnitCatalog>?> GetItemsAsync();
    Task<IEnumerable<ushort>?> GetUnitCodesAsync();    
    Task<AppObjectResponse> NewItemAsync(SupplementaryUnitCatalog item, string? UserName = "");
}

public class SupplementaryUnitProvider : ISupplementaryUnitProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public SupplementaryUnitProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<IEnumerable<SupplementaryUnitCatalog>?> GetItemsAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Items = await db.SupplementaryUnits.ToArrayAsync();

            if (Items is not null && Items.Any())
                return Items.OrderBy(x => x.Code);

            return null;
        }
    }

    public async Task<IEnumerable<ushort>?> GetUnitCodesAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            return await db.SupplementaryUnits.Select(s => s.Code).ToListAsync();
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(SupplementaryUnitCatalog item, string? UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);
                if (User is null)
                {
                    appObjResponse.ErrorAdd($"User NOT found.");
                    return appObjResponse;
                }

                var dbItems = await db.SupplementaryUnits.AsNoTracking().ToArrayAsync();

                /// check an Existing item
                bool isItemDbExists = dbItems.Any(s => s.Code == item.Code || s.ShortName == item.ShortName || s.FullName == item.FullName);

                if (!isItemDbExists)
                {
                    appObjResponse.ErrorAdd($"Item exists already: Code - {item.Code}, Name - {item.ShortName}");
                    return appObjResponse;
                }

                item.CreateUser = User;
                db.Entry(item).State = EntityState.Added;

                //var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                return appObjResponse;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); return appObjResponse; }
        }
    }
}