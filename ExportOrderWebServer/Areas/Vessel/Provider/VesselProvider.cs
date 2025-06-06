using static MudBlazor.CategoryTypes;

namespace ExportOrderWebServer.Areas.Vessel.Provider;

public class VesselProvider : IVesselProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private AppObjectResponse appObjResponse = new();

    public VesselProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Vessels.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Vessels.ToArrayAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Vessels = await db.Vessels
                                        .Where(s => !string.IsNullOrEmpty(filter.IMO) ? s.IMO == filter.IMO : true)
                                        .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.Name == filter.Name : true)
                                        .ToListAsync();

            appObjResponse.Object = Vessels.ToArray();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(VesselEntity item, string? UserName = "")
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

                var modifyItem = await db.Vessels.Include(ve => ve.Flag).AsNoTracking().FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd($"Vessel NOT found.");
                    return appObjResponse;
                }

                if (modifyItem.Name != item.Name)
                {
                    bool isItemExist = db.Terminals.Where(s => !string.IsNullOrWhiteSpace(s.Name))
                                                   .Any(s => s.Name!.ToUpper() == (!string.IsNullOrWhiteSpace(item.Name) ? item.Name.ToUpper() : string.Empty));

                    if (isItemExist)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                modifyItem.CreateUser = User;
                modifyItem.Name = item.Name;
                modifyItem.IMO = item.IMO;
                modifyItem.Flag = item.Flag;
                modifyItem.TerminalCode = item.TerminalCode;
                modifyItem.CaptainFamily = item.CaptainFamily;
                modifyItem.CaptainName = item.CaptainName;

                if (modifyItem.Flag != null)
                    db.Entry(modifyItem.Flag).State = EntityState.Unchanged;
                else
                    db.Entry(modifyItem).Reference("Flag").IsModified = true;                  

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

    public async Task<AppObjectResponse> NewItemAsync(VesselEntity item, string? UserName = "")
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

            bool isItemExist = db.Vessels.Any(s => s.Name == item.Name);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Vessel exists already: {item.Name}");
                return appObjResponse;
            }

            item.CreateUser = User;

            db.Entry(item.Flag!).State = EntityState.Unchanged;

            db.Entry(item).State = EntityState.Added;

            //var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(long id, string? UserName = "")
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

                var modifyItem = await db.Vessels.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

                if (modifyItem is not null)
                {
                    //modifyItem.CreateUser = User;
                    //db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;
                    //modifyItem.Status = EntityStatus.Cancelled;

                    //db.Entry(modifyItem).State = EntityState.Modified;

                    db.Entry(modifyItem).State = EntityState.Deleted;
                    //var bug = db.ChangeTracker.DebugView.LongView;
                    await db.SaveChangesAsync();
                }
                else
                    appObjResponse.ErrorAdd($"Item NOT found.");

                return appObjResponse;
            }
            catch (Exception ex)
            {
                appObjResponse.ErrorAdd(ex.Message);
                return appObjResponse;
            }
        }
    }

    public Task<IEnumerable<string>> GetNames()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Vessels.Where(s => !string.IsNullOrWhiteSpace(s.Name))
                                   .Where(s => s.Status != EntityStatus.Cancelled)
                                   .Select(s => s.Name!).OrderBy(s => s)
                                   .ToArrayAsync();
        }
    }
        
    public async Task<IEnumerable<string>> GetIMOnos()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Vessels.Where(s => !string.IsNullOrWhiteSpace(s.IMO))
                                   .Where(s => s.Status != EntityStatus.Cancelled)
                                   .Select(s => s.IMO!).OrderBy(s => s)
                                   .ToArrayAsync();
        }
    }
}
