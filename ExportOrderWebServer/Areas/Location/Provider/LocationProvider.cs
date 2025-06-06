namespace ExportOrderWebServer.Areas.Location.Provider;

public class LocationProvider : ILocationProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public LocationProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Locations.Include(lo => lo.Country).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Items = await db.Locations.ToArrayAsync();

            if (Items is not null && Items.Any())
                appObjResponse.Object = Items;
            else
                appObjResponse.ErrorAdd("Items not found.");
            
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Locations = await db.Locations.Include(lo => lo.Country)
                                              .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.Name == filter.Name : true)
                                              .Where(s => !string.IsNullOrEmpty(filter.UNLocode) ? s.UnLocode == filter.UNLocode : true)
                                              .Where(s => !string.IsNullOrEmpty(filter.Country) ? s.Country!.RUS == filter.Country : true)
                                              .ToListAsync();

            appObjResponse.Object = Locations;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(LocationCatalog item, string? UserName = "")
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

                var modifyItem = await db.Locations.Include(lo => lo.Country).FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem!.Name != item.Name)
                {
                    var itemExistCheck = await db.Locations
                                                         .Where(s => s.Name!.ToUpper() == item.Name!.ToUpper())
                                                         .FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                modifyItem!.CreateUser = User;
                modifyItem!.Name = item.Name;
                modifyItem!.NameEn = item.NameEn;
                modifyItem!.UnLocode = item.UnLocode;

                if (!modifyItem.Country!.Id.Equals(item.Country!.Id))
                    modifyItem!.Country = item.Country;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                db.Entry(modifyItem).State = EntityState.Modified;

                //var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                appObjResponse.ErrorAdd(ex.Message);
                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(LocationCatalog item, string? UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);
            if (User is null)
            {
                appObjResponse.ErrorAdd($"User NOT found.");
                return appObjResponse;
            }

            // check an Existing item
            var itemExistCheck = await db.Locations.Where(s => s.UnLocode == item!.UnLocode).FirstOrDefaultAsync();
            if (itemExistCheck is not null)
            {
                appObjResponse.ErrorAdd($"Location exists already: {item!.Name}");
                return appObjResponse;
            }

            item.CreateUser = User;

            db.Entry(item.Country!).State = EntityState.Unchanged;
            db.Entry(item).State = EntityState.Added;

            //var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(long id, string? UserName = "")
    {
        appObjResponse = new();

        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                /// Get Existing item
                var existedItem = await db.Locations.FirstOrDefaultAsync(s => s.Id == id);

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

    public Task<IEnumerable<string>> GetNames()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Locations.OrderBy(s => s.NameEn).Select(s => s.NameEn!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetUNLocodes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var Items = await db.Locations.Where(s => !string.IsNullOrWhiteSpace(s.UnLocode)).Select(s => s.UnLocode!).ToArrayAsync();

            if (Items is not null && Items.Any())
                return Items.Order().ToArray();
            else
                return Enumerable.Empty<string>();
        }
    }
}
