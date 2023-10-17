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
        appObjResponse = new();

        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                // check an Existing item
                var existedItem = await db.ContainerTypeSize.Where(s => s.Normolize == item.Normolize).FirstOrDefaultAsync();

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

    public async Task<IEnumerable<CntrTpSz>> GetCntrTypes()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var res = await db.ContainerTypeSize.AsNoTracking().ToListAsync();
            return res;
        }
    }

}
