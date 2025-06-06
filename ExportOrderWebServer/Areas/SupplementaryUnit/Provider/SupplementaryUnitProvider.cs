namespace ExportOrderWebServer.Areas.SupplementaryUnit.Provider;

public interface ISupplementaryUnitProvider
{    
    Task<IEnumerable<SupplementaryUnitCatalog>?> GetItemsAsync();
    Task<IEnumerable<ushort?>?> GetUnitCodesAsync();    
    Task<AppObjectResponse> NewItemAsync(SupplementaryUnitCatalog item, string? UserName = "");
    Task<AppObjectResponse> RemoveItemAsync(long id);
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

            return await db.SupplementaryUnits.ToArrayAsync();
            //var Items = await db.SupplementaryUnits.ToArrayAsync();

            //if (Items is not null && Items.Any())
            //    return Items.OrderBy(x => x.Code);
            //else
            //    return null;
        }
    }

    public async Task<IEnumerable<ushort?>?> GetUnitCodesAsync()
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

                if (isItemDbExists)
                {
                    appObjResponse.ErrorAdd($"Item exists already: Code - {item.Code}, Name - {item.ShortName}");
                    return appObjResponse;
                }

                item.CreateUser = User;
                db.Entry(item.CreateUser).State = EntityState.Unchanged;
                db.Entry(item).State = EntityState.Added;

                //var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                return appObjResponse;
            }
            catch (Exception ex) { appObjResponse.ErrorAdd(ex.Message); return appObjResponse; }
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(long id)
    {
        appObjResponse = new();

        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                /// Get Existing item
                var existedItem = await db.SupplementaryUnits.FirstOrDefaultAsync(s => s.Id == id);

                if (existedItem != null)
                {
                    db.Entry(existedItem).State = EntityState.Deleted;
                    //var bug = db.ChangeTracker.DebugView.LongView;
                    await db.SaveChangesAsync();
                }
                else
                    appObjResponse.ErrorAdd("Record not deleted");

                return appObjResponse;
            }
        }
        catch (Exception ex)
        {
            appObjResponse.ErrorAdd(ex.Message);
            return appObjResponse;
        }
    }
}