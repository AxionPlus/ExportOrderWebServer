using ExportOrderEntites.MyCompany;
using Microsoft.EntityFrameworkCore;
using System;

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

            foreach (var person in item.Persons!)
            {
                person.CreateUser = User!;
                db.Entry(person).State = EntityState.Added;
            }

            item.CreateUser = User!;

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
                var modiftyItem = await db.MyCompany.Include(s => s.Persons).FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modiftyItem is null)
                {
                    appObjResponse.ErrorAdd("Item not found");
                    return appObjResponse;
                }

                if (modiftyItem.Name != item.Name)
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

                db.ChangeTracker.Clear();

                modiftyItem = await db.MyCompany.Include(s => s.Persons).FirstOrDefaultAsync(s => s.Id == item.Id);
                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modiftyItem!.CreateUser = User!;
                modiftyItem!.Name = item.Name;

                foreach (var modiftyItemPerson in modiftyItem.Persons!)
                    if (!item.Persons!.Any(s => s.Id == modiftyItemPerson.Id))
                        modiftyItem.Persons!.Remove(modiftyItemPerson);
                    else
                    {
                        modiftyItemPerson.Name = item.Persons!.FirstOrDefault(s => s.Id == modiftyItemPerson.Id)!.Name;
                        modiftyItemPerson.Phone= item.Persons!.FirstOrDefault(s => s.Id == modiftyItemPerson.Id)!.Phone;
                        modiftyItemPerson.Document = item.Persons!.FirstOrDefault(s => s.Id == modiftyItemPerson.Id)!.Document;
                    }

                foreach (var itemPerson in item.Persons!)
                    if (!modiftyItem.Persons.Any(s => s.Id == itemPerson.Id))
                    {
                        itemPerson.CreateUser = User!;
                        //db.Entry(itemPerson.CreateUser).State = EntityState.Unchanged;

                        db.Entry(itemPerson).State = EntityState.Added;
                        modiftyItem.Persons.Add(itemPerson);
                    }

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
