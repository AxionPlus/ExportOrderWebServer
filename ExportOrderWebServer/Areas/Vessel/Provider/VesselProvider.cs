namespace ExportOrderWebServer.Areas.Vessel.Provider;

public class VesselProvider : IVesselProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private AppObjectResponse appObjResponse;

    public VesselProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
        appObjResponse = new();
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Vessels.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Vessels.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
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

    public async Task<AppObjectResponse> ModifyItemAsync(VesselEntity item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {   
                var modifyItem = await db.Vessels.Include(ve => ve.Flag).AsNoTracking().FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem!.Name != item.Name)
                {
                    bool isItemExist = db.Terminals.Any(s => s.Name!.ToUpper() == item.Name!.ToUpper());

                    if (isItemExist)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.Name = item.Name;
                modifyItem!.IMO = item.IMO;
                modifyItem!.Flag = item.Flag;
                modifyItem!.TerminalCode = item.TerminalCode;
                modifyItem!.CaptainFamily = item.CaptainFamily;
                modifyItem!.CaptainName = item.CaptainName;

                if (modifyItem!.Flag != null)
                    db.Entry(modifyItem.Flag).State = EntityState.Unchanged;
                else
                    db.Entry(modifyItem).Reference("Flag").IsModified = true;                  

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;


                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(ex.Message);

                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(VesselEntity item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);
                        
            bool isItemExist = db.Vessels.Any(s => s.Name == item.Name);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Vessel exists already: {item.Name}");
                return appObjResponse;
            }

            item.CreateUser = User!;

            db.Entry(item.Flag!).State = EntityState.Unchanged;

            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public Task<AppObjectResponse> RemoveItemAsync(long id)
    {
        throw new NotImplementedException();
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
            return await db.Vessels.OrderBy(s => s.Name).Select(s => s.Name!).ToListAsync();
        }
    }

    #region AUXILARY METHODS

    public async Task<IEnumerable<string>> GetIMOnos()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Vessels.Select(s => s.IMO!).ToListAsync();
        }
    }

    #endregion
}
