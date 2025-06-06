namespace ExportOrderWebServer.Areas.Terminal.Provider;

public class TerminalProvider : ITerminalProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public TerminalProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Terminals.Include(t => t.Location).Include(t => t.Customs).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Terminals.ToArrayAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Terminals = await db.Terminals.Include(t => t.Location).Include(t => t.Customs)
                                              .Where(s => !string.IsNullOrWhiteSpace(s.Name))
                                              .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.Name == filter.Name : true)
                                              .Where(s => !string.IsNullOrEmpty(filter.Location) ? s.Location != null && s.Location.Name == filter.Location : true)
                                              .OrderBy(s => s.Name)
                                              .ToArrayAsync();
            
            appObjResponse.Object = Terminals;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(TerminalCatalog item, string? UserName = "")
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

                modifyItem!.CreateUser = User;
                modifyItem.Name = item.Name;
                modifyItem.Location = item.Location!;
                modifyItem.Customs = item.Customs!;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;
                db.Entry(modifyItem.Location).State = EntityState.Unchanged;
                db.Entry(modifyItem.Customs).State = EntityState.Unchanged;                

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

    public async Task<AppObjectResponse> NewItemAsync(TerminalCatalog item, string? UserName = "")
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

            bool isItemExist = db.Terminals.Any(s => s.Name == item!.Name);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Terminal exists already: {item!.Name}");
                return appObjResponse;
            }

            item.CreateUser = User;

            db.Entry(item.CreateUser).State = EntityState.Unchanged;

            if (item.Location != null)
                db.Entry(item.Location!).State = EntityState.Unchanged;

            if (item.Customs != null)
                db.Entry(item.Customs!).State = EntityState.Unchanged;

            db.Entry(item).State = EntityState.Added;

            //var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

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

                /// Get Existing item
                var dbItem = await db.Set<TerminalCatalog>().FirstOrDefaultAsync(s => s.Id == id);

                if (dbItem != null)
                {
                    db.Entry(dbItem).State = EntityState.Deleted;
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

    public Task<IEnumerable<string>> GetNames()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<string>> GetNamesEn()
    {
        throw new NotImplementedException();
    }
}
