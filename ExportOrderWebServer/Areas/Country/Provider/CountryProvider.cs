namespace ExportOrderWebServer.Areas.Country.Provider;

public class CountryProvider : ICountryProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CountryProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
        appObjResponse = new();
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Countries.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Countries.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Commodities = await db.Countries
                                            .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.RUS == filter.Name : true)
                                            .Where(s => !string.IsNullOrEmpty(filter.NameEn) ? s.ENG == filter.NameEn : true)
                                            .ToListAsync();

            appObjResponse.Object = Commodities;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CountryCatalog item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.Countries.FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd($"Record not found");
                    return appObjResponse;
                }

                if (modifyItem is not null && modifyItem.RUS != item.RUS)
                {
                    bool isItemExist = db.Countries.Any(s => s.RUS.ToUpper() == item.RUS.ToUpper());

                    if (isItemExist)
                    {
                        appObjResponse.ErrorAdd($" {item.RUS} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.RUS = item.RUS;
                modifyItem!.ENG = item.ENG;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

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

    public async Task<AppObjectResponse> NewItemAsync(CountryCatalog item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            bool isItemExist = db.Countries.Any(s => s.ENG == item.ENG);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Country: {item!.RUS} exists already");
                return appObjResponse;
            }

            item.CreateUser = User!;
            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(long id)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                /// Get Existing item
                var existedItem = await db.Countries.Where(s => s.Id == id).FirstOrDefaultAsync();

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
            return await db.Countries.Select(s => s.RUS!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Countries.Select(s => s.ENG!).ToListAsync();
        }
    }

}
