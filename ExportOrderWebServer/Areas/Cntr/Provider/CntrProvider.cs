using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Cntr.Provider;

public class CntrProvider : ICntrProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CntrProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public Task<AppObjectResponse> GetItemAsync(long id)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> GetItemAsync(string Num)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Containers.AsNoTracking().FirstOrDefaultAsync(s => s.Num == Num);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Containers.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var containers = await db.Containers.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Name))
                    containers = containers.Where(s => s.Num == filter.CntrNum).ToList();

                if (!string.IsNullOrWhiteSpace(filter.CntrType))
                    containers = containers.Where(s => s.TpSz.ToString() == filter.CntrType).ToList();

                if (filter.CntrIsSOC.HasValue)
                    containers = containers.Where(s => s.IsSOC == filter.CntrIsSOC.Value).ToList();

                if (!string.IsNullOrWhiteSpace(filter.Carrier))
                    containers = containers.Where(s => s.Carrier.ShortName == filter.Carrier).ToList();
            }

            appObjResponse.Object = containers.ToArray();

            return appObjResponse;
        }
    }
    
    public async Task<AppObjectResponse> ModifyItemAsync(CntrEntity item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(CntrEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.Containers.Where(s => s.Num == item.Num).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Location exists already: {item.Num}");
                return appObjResponse;
            }

            item.Num = item.Num.ToUpper();

            db.Entry(item.TpSz).State = EntityState.Unchanged;
            db.Entry(item.Carrier).State = EntityState.Unchanged;
            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(CntrEntity item)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetNames()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetNamesEn()
    {
        throw new NotImplementedException();
    }


    #region AUXILARY METHODS

    public async Task<IEnumerable<string>> GetCntrNums()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Containers.Select(s => s.Num).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetCntrTypeNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.ContainerTypeSize.Select(s => s.Normolize!).ToListAsync();
        }
    }

    public async Task<IEnumerable<CntrTpSz>> GetCntrTypes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            return await db.ContainerTypeSize.AsNoTracking().ToListAsync();
        }
    }
    #endregion
}
