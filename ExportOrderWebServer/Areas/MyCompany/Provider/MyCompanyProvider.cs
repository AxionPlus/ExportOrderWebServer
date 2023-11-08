

namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public class MyCompanyProvider : IMyCompanyProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public MyCompanyProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> GetItemAsync(uint id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.MyCompany.Include(mc => mc.Persons).AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<bool> IsItemNullAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var com = await db.MyCompany.ToListAsync();

            return com.Count() == 0;
        }
    }

    public async Task<AppObjectResponse> GetLastItemAsync()
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.MyCompany.Include(mc => mc.Persons).AsNoTracking().OrderBy(mc => mc.Id).LastOrDefaultAsync();
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.MyCompany.Where(s => s.Name == item!.Name).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Company exists already: {item!.Name} ");
                return appObjResponse;
            }

            item.CreateUser = User!;

            foreach (var person in item.Persons!)
            {
                person.CreateUser = User!;
                person.CreateTime = DateTime.Now;
                db.Entry(person).State = EntityState.Added;
            }            

            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(MyCompanyEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.MyCompany.Include(s => s.Persons).FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd("Item not found");
                    return appObjResponse;
                }

                if (modifyItem.Name != item.Name)
                {
                    var itemExistCheck = await db.MyCompany
                                                         .Where(s => s.Name!.ToUpper() == item.Name!.ToUpper())
                                                         .FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.Name = item.Name;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                // compaire new item with existed
                foreach (var modifyItemPerson in modifyItem.Persons!)
                    if (!item.Persons!.Any(s => s.Id == modifyItemPerson.Id))
                        modifyItem.Persons!.Remove(modifyItemPerson);
                    else
                    {
                        modifyItemPerson.CreateUser = User;
                        modifyItemPerson.CreateTime = DateTime.Now;
                        modifyItemPerson.Name = item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Name;
                        modifyItemPerson.FamilyName = item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.FamilyName;
                        modifyItemPerson.SurName= item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.SurName;
                        modifyItemPerson.Phone= item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Phone;
                        modifyItemPerson.BornYear= item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.BornYear;
                        modifyItemPerson.BornPlace= item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.BornPlace;
                        modifyItemPerson.CompanyName= item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.CompanyName;
                        modifyItemPerson.Address = item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Address;
                        modifyItemPerson.Passport = item.Persons!.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Passport;
                    }

                // compaire existed item with new
                foreach (var itemPerson in item.Persons!)
                    if (!modifyItem.Persons.Any(s => s.Id == itemPerson.Id))
                    {
                        itemPerson.CreateUser = User!;
                        itemPerson.CreateTime = DateTime.Now;
                        db.Entry(itemPerson.CreateUser).State = EntityState.Unchanged;

                        db.Entry(itemPerson).State = EntityState.Added;
                        modifyItem.Persons.Add(itemPerson);
                    }

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
}
