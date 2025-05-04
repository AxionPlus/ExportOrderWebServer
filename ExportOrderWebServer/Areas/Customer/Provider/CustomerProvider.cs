namespace ExportOrderWebServer.Areas.Customer.Provider;

public class CustomerProvider : ICustomerProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CustomerProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
        appObjResponse = new();
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Customers.Include(cu => cu.Country).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Customers.ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Customers = await db.Customers.Include(cu => cu.Country)
                                              .Where(s => !string.IsNullOrEmpty(filter.Name) ? s.Name == filter.Name : true)
                                              .Where(s => !string.IsNullOrEmpty(filter.NameEn) ? s.NameEn == filter.NameEn : true)
                                              .Where(s => !string.IsNullOrEmpty(filter.Country) ? s.Country!.RUS == filter.Country : true)
                                              .ToListAsync();

            appObjResponse.Object = Customers;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CustomerCatalog item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.Customers.Include(cu => cu.Country).AsNoTracking().FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem!.Name != item.Name)
                {
                    bool isItemExist = db.Customers.Any(s => s.Name!.ToUpper() == item.Name!.ToUpper());

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
                modifyItem!.NameEn = item.NameEn;
                modifyItem.Country = item.Country!;

                //if (!modifyItem.Country!.Id.Equals(item.Country!.Id))
                //    modifyItem.Country = item.Country!;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;
                db.Entry(modifyItem.Country).State = EntityState.Unchanged;

                db.Entry(modifyItem).State = EntityState.Modified;

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

    public async Task<AppObjectResponse> NewItemAsync(CustomerCatalog item)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            bool isItemRuExist = db.Customers.Any(s => s.Name!.ToUpper() == item!.Name!.ToUpper());
            if (isItemRuExist)
            {
                appObjResponse.ErrorAdd($"Customer exists already: {item!.Name}");
                return appObjResponse;
            }

            // check an Existing item
            bool isItemEnExist = db.Customers.Any(s => s.NameEn!.ToUpper() == item!.NameEn!.ToUpper());
            if (isItemEnExist)
            {
                appObjResponse.ErrorAdd($"Customer exists already: {item!.NameEn}");
                return appObjResponse;
            }

            item.CreateUser = User!;

            db.Entry(item.Country!).State = EntityState.Unchanged;
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
                var existedItem = await db.Customers.Where(s => s.Id == id).FirstOrDefaultAsync();

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

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Customers.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Customers.Select(s => s.NameEn!).ToListAsync();
        }
    }
}
