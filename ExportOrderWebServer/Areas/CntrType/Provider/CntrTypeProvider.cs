using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Cntr.Provider;

public class CntrTypeProvider : ICntrTypeProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CntrTypeProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.ContainerTypeSize.AsNoTracking().OrderBy(v => v.Normolize).ToListAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemsAsync(List<CntrTpSz> NewItems)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            foreach (var item in NewItems)
            {
                // check an Existing item
                var itemExistCheck = await db.ContainerTypeSize.Where(s => s.ISO == item.ISO).FirstOrDefaultAsync();
                if (itemExistCheck != null)
                {
                    appObjResponse.ErrorAdd($"Type is exists already: {item.Normolize}");
                    return appObjResponse;
                }

                db.Entry(item).State = EntityState.Added;
            }
            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(CntrTpSz item)
    {
        await Task.Delay(5);
        appObjResponse = new();
        return appObjResponse;
    }

    public async Task<IEnumerable<CntrTpSz>> GetCntrTypes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var res = await db.ContainerTypeSize.AsNoTracking().ToListAsync();
            return res;
        }
    }


    #region NOT USED
    public async Task<CntrTpSz> GetCntrType(string type)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Types = await db.ContainerTypeSize.AsNoTracking().ToListAsync();

            var Type = Types.FirstOrDefault(x => x.Normolize == type);

            if (Type != null)
                return Type;
            else
                return new CntrTpSz();
        }
    }

    public async Task<IEnumerable<string>> GetCntrNums()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetCntrTypeNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.ContainerTypeSize.Select(s => s.Normolize!).ToListAsync();
        }
    }

    #endregion
}
