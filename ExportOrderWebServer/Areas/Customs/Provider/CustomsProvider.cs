namespace ExportOrderWebServer.Areas.Customs.Provider;

public class CustomsProvider : ICustomsProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public CustomsProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.CustomsOffices.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public Task<AppObjectResponse> GetItemsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var customsOffices = await db.CustomsOffices
                                                        .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.OfficeShort == filter.Name : true)
                                                        .Where(s => !string.IsNullOrEmpty(filter.Num)? s.Code == filter.Num : true)
                                                        .ToListAsync();
            
            appObjResponse.Object = customsOffices.ToArray();

            return appObjResponse;
        }
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.CustomsOffices.Select(s => s.OfficeShort!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.CustomsOffices.Select(s => s.Code!).ToListAsync();
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CustomsCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.CustomsOffices.FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem!.Office != item.Office)
                {
                    var itemExistCheck = await db.CustomsOffices
                                                         .Where(s => s.Office!.ToUpper() == item.Office!.ToUpper())
                                                         .FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($" {item.Office} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.Code = item.Code;
                modifyItem!.Office = item.Office;
                modifyItem!.OfficeShort = item.OfficeShort;
                modifyItem!.Dapartment = item.Dapartment;
                modifyItem!.Email = item.Email;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

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

    public async Task<AppObjectResponse> NewItemAsync(CustomsCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            bool isItemExist = db.CustomsOffices.Any(s => s.Office == item!.Office);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Таможенный пост уже существует: {item!.Office}");
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
        appObjResponse = new();

        try
        { 
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                bool isItemExist = db.CustomsOffices.Any(s => s.Id == id);

                if (isItemExist)
                {
                    db.Entry(isItemExist).State = EntityState.Deleted;

                    var bug = db.ChangeTracker.DebugView.LongView;
                    await db.SaveChangesAsync();
                }
                else
                    appObjResponse.ErrorAdd("Record wasn't deleted");

                return appObjResponse;
            }
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            return appObjResponse;
        }
    }
}
