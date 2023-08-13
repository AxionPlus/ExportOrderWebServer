using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Commodity.Provider;

public class CommodityProvider : ICommodityProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;    

    private AppObjectResponse appObjResponse;

    public CommodityProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Commodities.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Commodities.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Commodities = await db.Commodities.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.HScode))
                    Commodities = Commodities.Where(s => s.HSCode == filter.HScode).ToList();

                if (!string.IsNullOrEmpty(filter.Name))
                    Commodities = Commodities.Where(s => s.Name == filter.Name).ToList();

                if (!string.IsNullOrEmpty(filter.NameEn))
                    Commodities = Commodities.Where(s => s.EngName == filter.NameEn).ToList();
            }

            appObjResponse.Object = Commodities.ToArray();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CommodityCatalog item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(CommodityCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            //var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.Commodities.Where(s => s.HSCode == item!.HSCode).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"HS code: {item!.HSCode} exists already");
                return appObjResponse;
            }

            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(CommodityCatalog item)
    {
        throw new NotImplementedException();
    }

    #region SEARCH METHODS

    public async Task<IEnumerable<string>> GetHScodes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.Select(s => s.HSCode).ToListAsync();
        }
    }

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
