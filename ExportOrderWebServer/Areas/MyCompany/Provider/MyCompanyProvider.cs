namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public class MyCompanyProvider : IMyCompanyProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public MyCompanyProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<AppObjectResponse> GetLastItemAsync()
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var myCompany = await db.MyCompany.Include(s => s.Persons.Where(p => p != null).OrderBy(p => p.FamilyName))
                                              .AsNoTracking().OrderBy(s => s.Id).LastOrDefaultAsync();

            if (myCompany is null)
                appObjResponse.ErrorAdd("My Company not found.");
            else
                appObjResponse.Object = myCompany;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item, string? UserName = "")
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

            /// check an Existing item
            bool isItemExist = db.MyCompany.Any(s => s.Name == item.Name);
            if (isItemExist)
            {
                appObjResponse.ErrorAdd($"Company exists already: {item.Name} ");
                return appObjResponse;
            }

            item.CreateUser = User;

            foreach (var person in item.Persons)
            {
                person.CreateUser = User;
                db.Entry(person).State = EntityState.Added;
            }            

            db.Entry(item).State = EntityState.Added;

            //var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(MyCompanyEntity item, string? UserName = "")
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

                var modifyItem = await db.MyCompany.Include(s => s.Persons).FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd("Item not found");
                    return appObjResponse;
                }

                /// Check an existing Item
                if (modifyItem.Name != item.Name)
                {
                    bool isItemExist = db.MyCompany.Any(s => !string.IsNullOrWhiteSpace(s.Name) &&
                                                             !string.IsNullOrWhiteSpace(item.Name) &&
                                                             s.Name.ToUpper() == item.Name.ToUpper());

                    if (isItemExist)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                modifyItem.CreateUser = User;
                modifyItem.Name = item.Name;
                modifyItem.NameEn = item.NameEn;
                modifyItem.Email = item.Email;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                /// compaire new item with existed
                foreach (var modifyItemPerson in modifyItem.Persons.ToArray())
                    if (!item.Persons.Any(s => s.Id == modifyItemPerson.Id))
                        modifyItem.Persons.Remove(modifyItemPerson);
                    else
                    {
                        modifyItemPerson.CreateUser = User;
                        modifyItemPerson.Name = item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Name;
                        modifyItemPerson.FamilyName = item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.FamilyName;
                        modifyItemPerson.SurName= item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.SurName;
                        modifyItemPerson.Phone= item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Phone;
                        modifyItemPerson.BirthYear= item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.BirthYear;
                        modifyItemPerson.BirthPlace= item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.BirthPlace;
                        modifyItemPerson.CompanyName= item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.CompanyName;
                        modifyItemPerson.CompanyEmail = item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.CompanyEmail;
                        modifyItemPerson.Address = item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Address;
                        modifyItemPerson.Passport = item.Persons.FirstOrDefault(s => s.Id == modifyItemPerson.Id)!.Passport;
                    }

                /// compaire existed item with a new
                foreach (var itemPerson in item.Persons.ToArray())
                    if (!modifyItem.Persons.Any(s => s.Id == itemPerson.Id))
                    {
                        itemPerson.CreateUser = User;
                        db.Entry(itemPerson.CreateUser).State = EntityState.Unchanged;

                        db.Entry(itemPerson).State = EntityState.Added;
                        modifyItem.Persons.Add(itemPerson);
                    }

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

    public async Task<AppObjectResponse> GetPersonItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Persons.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> ModifyPersonItemAsync(PersonEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);
            if (User is null)
            {
                appObjResponse.ErrorAdd($"User not found.");
                return appObjResponse;
            }

            /// check an Existing FamilyName
            bool isFamilyNameExist = db.Persons.Any(s => s.Id != item.Id && s.FamilyName == item.FamilyName && s.Name == item.Name);
            if (isFamilyNameExist)
            {
                appObjResponse.ErrorAdd($"Person exists already: {item.FamilyName} {item.Name}");
                return appObjResponse;
            }

            var modifyItem = await db.Persons.FirstOrDefaultAsync(s => s.Id == item.Id);

            var dbMyCompanyItem = await db.MyCompany.Include(s => s.Persons).AsNoTracking().AsSplitQuery()                                                    
                                                    .OrderBy(s => s.Id).LastOrDefaultAsync();
            if (dbMyCompanyItem is null)
            {
                appObjResponse.ErrorAdd($"My Company not found.");
                return appObjResponse;
            }
            
            if (modifyItem is null)
            {
                /// New Item
                item.CreateUser = User;
                db.Entry(item.CreateUser).State = EntityState.Unchanged;

                db.Entry(item).State = EntityState.Added;
                dbMyCompanyItem.Persons.Add(item);

                db.Entry(dbMyCompanyItem).State = EntityState.Modified;
            }
            else
            {
                /// Modify Item
                modifyItem.CreateUser = User;
                modifyItem.Name = item.Name;
                modifyItem.SurName = item.SurName;
                modifyItem.FamilyName = item.FamilyName;
                modifyItem.Phone = item.Phone;
                modifyItem.BirthYear = item.BirthYear;
                modifyItem.BirthPlace = item.BirthPlace;
                modifyItem.CompanyName = item.CompanyName;
                modifyItem.CompanyEmail = item.CompanyEmail;
                modifyItem.Address = item.Address;
                modifyItem.Passport = item.Passport;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                db.Entry(modifyItem).State = EntityState.Modified;
            }

            var bug = db.ChangeTracker.DebugView.LongView;
            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }
}
