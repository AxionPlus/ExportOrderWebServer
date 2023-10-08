using ExportOrderEntites.MyCompany;
using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public class MyCompanyProvider : IMyCompanyProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public MyCompanyProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async  Task<AppObjectResponse> GetItemAsync(uint id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.MyCompany.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }


    public async Task<bool> IsMyCompanyItemNullAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var com = await db.MyCompany.ToListAsync();

            return com.Count() == 0;
        }
    }

    public async Task<uint> LastVersionMyCompanyAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var last = await db.MyCompany.AsNoTracking().LastOrDefaultAsync();
            
            if (last is null)
                return 1;

            return last.Id;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            
            // check an Existing item
            var itemExistCheck = await db.MyCompany.Where(s => s.Name == item!.Name).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Company exists already: {item!.Name} ");
                return appObjResponse;
            }

            foreach (var person in item.Persons!)
                db.Entry(person).State = EntityState.Added;

            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }
}
