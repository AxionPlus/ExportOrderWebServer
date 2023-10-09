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
                    customsOffices = customsOffices.Where(s => s.CustomsCode == filter.Num).ToList();

                if (!string.IsNullOrEmpty(filter.Name))
                    customsOffices = customsOffices.Where(s => s.CustomsOffice == filter.Name).ToList();
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
            return await db.CustomsOffices.Select(s => s.CustomsOffice!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.CustomsOffices.Select(s => s.CustomsCode!).ToListAsync();
        }
    }

    public Task<AppObjectResponse> ModifyItemAsync(CustomsCatalog item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(CustomsCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.CustomsOffices.Where(s => s.CustomsOffice == item!.CustomsOffice).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Таможенный пост уже существует: {item!.CustomsOffice}");
                return appObjResponse;
            }

            item.CreateUser = User!;

            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(CustomsCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            // check an Existing item
            var itemExistCheck = await db.CustomsOffices.Where(s => s.Id == item!.Id).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                db.Entry(item).State = EntityState.Deleted;
                return appObjResponse;
            }           

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }
}
