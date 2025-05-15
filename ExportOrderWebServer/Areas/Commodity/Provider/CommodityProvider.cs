namespace ExportOrderWebServer.Areas.Commodity.Provider;

public class CommodityProvider : ICommodityProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;    

    private AppObjectResponse appObjResponse = new();

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

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            
            var commodities = await db.Commodities.AsNoTracking()
                                                  .Where(s => string.IsNullOrEmpty(filter.HScode) ? true : s.HSCode == filter.HScode)
                                                  .Where(s => string.IsNullOrEmpty(filter.Name) ? true : s.Name == filter.Name)
                                                  .Where(s => string.IsNullOrEmpty(filter.NameEn) ? true : s.NameEn == filter.NameEn)
                                                  .Where(s => !filter.IsIMO.HasValue ? true : s.IsIMO == filter.IsIMO)
                                                  .ToListAsync();

            if (commodities is null)
                appObjResponse.ErrorAdd("Commodities not found.");

            appObjResponse.Object = commodities;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CommodityCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.Commodities.FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd($" {item.Name} not found.");
                    return appObjResponse;
                }

                if (modifyItem.Name != item.Name)
                {
                    bool isItemExist = db.Commodities.Any(s => s.Name!.ToUpper() == item.Name!.ToUpper());

                    if (isItemExist)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem.CreateUser = User!;
                modifyItem.Name = item.Name;
                modifyItem.NameEn = item.NameEn;
                modifyItem.HSCode = item.HSCode;
                modifyItem.IMO = item.IMO;
                modifyItem.UNNO = item.UNNO;
                modifyItem.IsIMO = item.IsIMO;

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

    public async Task<AppObjectResponse> NewItemAsync(CommodityCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);
            if (User is null)
            {
                appObjResponse.ErrorAdd("User not found.");
                return appObjResponse;
            }
            // check an Existing item
            //var itemExistCheck = await db.Commodities.Where(s => s.Name == item!.Name || s.NameEn == item.NameEn).FirstOrDefaultAsync();
            //if (itemExistCheck != null)
            //{
            //    appObjResponse.ErrorAdd($"Commodity exists already.<br>HS code: {item!.HSCode}<br>Name: {item.Name} ");
            //    return appObjResponse;
            //}

            try
            {
                item.CreateUser = User;
                db.Entry(item).State = EntityState.Added;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                appObjResponse.ErrorAdd(ex.Message);
                return appObjResponse;
            }
            
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

                /// check an Existing item
                var existedItem = await db.Commodities.Where(s => s.Id == id).FirstOrDefaultAsync();

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
            appObjResponse.ErrorAdd(ex.Message);
            return appObjResponse;
        }
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.AsNoTracking().Select(s => s.Name).Distinct().OrderBy(s => s).ToArrayAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.AsNoTracking().Select(s => s.NameEn).Distinct().OrderBy(s => s).ToArrayAsync();
        }
    }

    public async Task<IEnumerable<string>> GetHScodes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.AsNoTracking().Select(s => s.HSCode).Distinct().OrderBy(s => s).ToArrayAsync();
        }
    }
}
