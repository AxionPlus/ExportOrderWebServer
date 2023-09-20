using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;

namespace ExportOrderWebServer.Areas.Carrier.Provider;

public class CarrierProvider : ICarrierProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CarrierProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }



    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Carriers.AsNoTracking().Include(s => s.CarrierDetails).FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Carriers.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Carriers = await db.Carriers.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Name))
                    Carriers = Carriers.Where(s => s.Name == filter.Name).ToList();

                if (!string.IsNullOrEmpty(filter.NameEn))
                    Carriers = Carriers.Where(s => s.NameEn == filter.NameEn).ToList();
            }

            appObjResponse.Object = Carriers.ToArray();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CarrierCatalog item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(CarrierCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.Carriers.Where(s => s.Name == item!.Name).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Carrier exists already: {item!.Name}");
                return appObjResponse;
            }

            item.CreateUser = User!;
            db.Entry(item).State = EntityState.Added;

            foreach (var record in item.CarrierDetails)
                db.Entry(record).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(CarrierCatalog item)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Carriers.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Carriers.Select(s => s.NameEn!).ToListAsync();
        }
    }


    #region AUXILARY

    public async Task<AppObjectResponse> RemoveDetailsAsync(CarrierTerminalDetails item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            // check an Existing item
            var itemExistCheck = await db.CarrierDetails.Where(s => s.Id == item!.Id).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Carrier details dosn't exists for: {item!.TerminalName}");
                return appObjResponse;
            }

            db.Entry(item).State = EntityState.Deleted;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<IEnumerable<string>> GetTerminalNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Terminals.Select(s => s.Name!).ToListAsync();
        }
    }

    #endregion
}
