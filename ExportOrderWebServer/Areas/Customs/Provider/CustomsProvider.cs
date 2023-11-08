using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Customs.Provider;

public class CustomsProvider : ICustomsProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

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

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var customsOffices = await db.CustomsOffices.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Num))
                    customsOffices = customsOffices.Where(s => s.Code == filter.Num).ToList();

                if (!string.IsNullOrEmpty(filter.Name))
                    customsOffices = customsOffices.Where(s => s.OfficeShort == filter.Name).ToList();
            }

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

            // check an Existing item
            var itemExistCheck = await db.CustomsOffices.Where(s => s.Office == item!.Office).FirstOrDefaultAsync();
            if (itemExistCheck != null)
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

            // check an Existing item
            var existedItem = await db.CustomsOffices.Where(s => s.Id == id).FirstOrDefaultAsync();

            if (existedItem != null)
            {
                //db.Entry(item.CreateUser).State = EntityState.Detached;
                db.Entry(existedItem).State = EntityState.Deleted;

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
