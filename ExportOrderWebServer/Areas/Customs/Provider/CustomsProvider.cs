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

            var customsOffices = await db.CustomsOffices.Where(s => !string.IsNullOrWhiteSpace(s.Code) && !string.IsNullOrWhiteSpace(s.OfficeShort))
                                                        .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.OfficeShort == filter.Name : true)
                                                        .Where(s => !string.IsNullOrEmpty(filter.Num)? s.Code == filter.Num : true)
                                                        .OrderBy(s =>s.Code)
                                                        .ToArrayAsync();
            
            appObjResponse.Object = customsOffices;

            return appObjResponse;
        }
    }

    public Task<IEnumerable<string>> GetNames()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetNamesEn()
    {
        throw new NotSupportedException();
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CustomsCatalog item, string? UserName = "")
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

                modifyItem!.CreateUser = User;
                modifyItem!.Code = item.Code;
                modifyItem!.Office = item.Office;
                modifyItem!.OfficeShort = item.OfficeShort;
                modifyItem!.Dapartment = item.Dapartment;
                modifyItem!.Email = item.Email;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

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

    public async Task<AppObjectResponse> NewItemAsync(CustomsCatalog item, string? UserName = "")
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

            bool isItemExist = db.CustomsOffices.Any(s => s.Office == item!.Office);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Таможенный пост уже существует: {item!.Office}");
                return appObjResponse;
            }

            item.CreateUser = User;

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
                var dbItem = await db.CustomsOffices.FirstOrDefaultAsync(s => s.Id == id);

                if (dbItem != null)
                {
                    db.Entry(dbItem).State = EntityState.Deleted;
                    //var bug = db.ChangeTracker.DebugView.LongView;
                    await db.SaveChangesAsync();
                }
                else
                    appObjResponse.ErrorAdd("Record wasn't deleted");

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
