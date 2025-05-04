namespace ExportOrderWebServer.Areas.Terminal.Provider;

public class TerminalProvider : ITerminalProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public TerminalProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
        appObjResponse = new();
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Terminals.Include(t => t.Location).Include(t => t.Customs).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Terminals.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Terminals = await db.Terminals.Include(t => t.Location).Include(t => t.Customs)
                                              .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.Name == filter.Name : true)
                                              .Where(s => !string.IsNullOrEmpty(filter.Location) ? s.Location!.Name == filter.Location : true)
                                              .ToListAsync();
            
            appObjResponse.Object = Terminals.ToArray();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(TerminalCatalog item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.Terminals.Include(s => s.Customs).Include(s => s.Location).AsNoTracking().FirstOrDefaultAsync(s => s.Id == item.Id);

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
                modifyItem.Location = item.Location!;
                modifyItem.Customs = item.Customs!;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;
                db.Entry(modifyItem.Location).State = EntityState.Unchanged;
                db.Entry(modifyItem.Customs).State = EntityState.Unchanged;                

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

    public async Task<AppObjectResponse> NewItemAsync(TerminalCatalog item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            bool isItemExist = db.Terminals.Any(s => s.Name == item!.Name);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Terminal exists already: {item!.Name}");
                return appObjResponse;
            }

            item.CreateUser = User!;

            db.Entry(item.CreateUser!).State = EntityState.Unchanged;

            if (item.Location != null)
                db.Entry(item.Location!).State = EntityState.Unchanged;

            if (item.Customs != null)
                db.Entry(item.Customs!).State = EntityState.Unchanged;

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

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Terminals.Select(s => s.Name!).ToListAsync();
        }
    }

    public Task<IEnumerable<string>> GetNamesEn()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<CustomsCatalog>> GetCustomsOfficesAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Item = await db.CustomsOffices.AsNoTracking().ToListAsync();

            return Item!;
        }
    }
}
