using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Location.Provider;

public class LocationProvider : ILocationProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public LocationProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
        appObjResponse = new();
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Locations.Include(lo => lo.Country).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Locations.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
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

    public async Task<AppObjectResponse> ModifyItemAsync(LocationCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
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

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.Name = item.Name;
                modifyItem!.NameEn = item.NameEn;
                modifyItem!.UnLocode = item.UnLocode;

                if (!modifyItem.Country!.Id.Equals(item.Country!.Id))
                    modifyItem!.Country = item.Country;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(ex.Message);

                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(LocationCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.Locations.Where(s => s.UnLocode == item!.UnLocode).FirstOrDefaultAsync();
            if (itemExistCheck is not null)
            {
                appObjResponse.ErrorAdd($"Location exists already: {item!.Name}");
                return appObjResponse;
            }

            item.CreateUser = User!;

            db.Entry(item.Country!).State = EntityState.Unchanged;
            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
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

                // check an Existing item
                var existedItem = await db.Locations.Where(s => s.Id == id).FirstOrDefaultAsync();

                if (existedItem is null)
                {
                    appObjResponse.ErrorAdd("Record wasn't deleted");
                    return appObjResponse;
                }

                db.Entry(existedItem).State = EntityState.Deleted;

                var bug = db.ChangeTracker.DebugView.LongView;
                await db.SaveChangesAsync();

                return appObjResponse;
            }
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            appObjResponse.ErrorAdd(msg);
            return appObjResponse;
        }
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Locations.OrderBy(s => s.Name).Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Locations.OrderBy(s => s.NameEn).Select(s => s.NameEn!).ToListAsync();
        }
    }

    #region AUXILARY METHODS

    public async Task<IEnumerable<string>> GetUNLocodes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Locations.Select(s => s.UnLocode!).ToListAsync();
        }
    }    

    #endregion
}
