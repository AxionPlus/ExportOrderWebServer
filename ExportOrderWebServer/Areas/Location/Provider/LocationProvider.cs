using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Location.Provider;

public class LocationProvider : ILocationProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

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

            appObjResponse.Object = await db.Locations.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Locations.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Locations = await db.Locations.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Name))
                    Locations = Locations.Where(s => s.Name == filter.Name).ToList();

                if (!string.IsNullOrEmpty(filter.UNLocode))
                    Locations = Locations.Where(s => s.UnLocode == filter.UNLocode).ToList();

                if (!string.IsNullOrEmpty(filter.Country))
                    Locations = Locations.Where(s => s.Country.RUS == filter.Country).ToList();
            }

            appObjResponse.Object = Locations.ToArray();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(LocationCatalog item)
    {
        throw new NotImplementedException();
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
            if (itemExistCheck != null)
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

    public async Task<AppObjectResponse> RemoveItemAsync(LocationCatalog item)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Locations.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        throw new NotImplementedException();
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
