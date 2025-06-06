using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Country.Provider;
public interface ICountryProvider : IEntityProvider<CountryCatalog>
{
    public  Task<IEnumerable<string>> GetNames();


    public  Task<IEnumerable<string>> GetNamesEn();

}

public class CountryProvider : ICountryProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

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

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Commodities = await db.Countries.Where(s => !string.IsNullOrEmpty(filter.Name) ? s.RUS == filter.Name : true)
                                                .Where(s => !string.IsNullOrEmpty(filter.NameEn) ? s.ENG == filter.NameEn : true)
                                                .ToArrayAsync();

            appObjResponse.Object = Commodities;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CountryCatalog item, string? UserName = "")
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

                modifyItem!.CreateUser = User;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.RUS = item.RUS;
                modifyItem!.ENG = item.ENG;

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

    public async Task<AppObjectResponse> NewItemAsync(CountryCatalog item, string? UserName = "")
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

            bool isItemExist = db.Countries.Any(s => s.RUS == item.RUS && s.ENG == item.ENG);

            if (!isItemExist)
            {
                item.CreateUser = User;
                db.Entry(item.CreateUser).State = EntityState.Unchanged;
                db.Entry(item).State = EntityState.Added;
                
                //var bug = db.ChangeTracker.DebugView.LongView;
                await db.SaveChangesAsync();
            }
            else
                appObjResponse.ErrorAdd($"Country: {item.RUS} exists already");

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

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);
                if (User is null)
                {
                    appObjResponse.ErrorAdd($"User NOT found.");
                    return appObjResponse;
                }

                /// Get Existing item
                var existedItem = await db.Countries.FirstOrDefaultAsync(s => s.Id == id);

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

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var Items = await db.Countries.Where(s =>!string.IsNullOrWhiteSpace(s.RUS)).Select(s => s.RUS!).ToArrayAsync();

            if (Items != null && Items.Any())
                return Items.Distinct().Order().ToArray();
            else
                return Enumerable.Empty<string>();
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
