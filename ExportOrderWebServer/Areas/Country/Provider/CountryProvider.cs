using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Country.Provider;

public class CountryProvider : ICountryProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CountryProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Countries.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Countries.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Commodities = await db.Countries.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Name))
                    Commodities = Commodities.Where(s => s.RUS == filter.Name).ToList();

                if (!string.IsNullOrEmpty(filter.NameEn))
                    Commodities = Commodities.Where(s => s.ENG == filter.NameEn).ToList();
            }

            appObjResponse.Object = Commodities.ToArray();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CountryCatalog item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(CountryCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            //var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.Countries.Where(s => s.ENG == item!.ENG).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Country: {item!.RUS} exists already");
                return appObjResponse;
            }

            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(CountryCatalog item)
    {
        throw new NotImplementedException();
    }

    #region SEARCH METHODS

    public async Task<IEnumerable<string>> GetSearchNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetSearchNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.Select(s => s.EngName!).ToListAsync();
        }
    }

    #endregion
}
