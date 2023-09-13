using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public class ExportOrderProvider : IExportOrderProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public ExportOrderProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> AddUploadedFileItemsAsync(IEnumerable<ExportOrderRecord> items)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            try
            {
                var db = await _db;

                //var cntrTypes = await db.ContainerTypeSize.AsNoTracking().ToArrayAsync();
                

                foreach (var exportOrderRecord in items)
                {
                    //var cntrType = cntrTypes.FirstOrDefault(s => s.Normolize!.Replace(" ", "") == exportOrderRecord.CntrType.Normolize);

                    db.Entry(exportOrderRecord).State = EntityState.Added;

                    foreach (var cntrContent in exportOrderRecord.Contents)
                        db.Entry(cntrContent).State = EntityState.Added;

                    var bug = db.ChangeTracker.DebugView.LongView;

                    await db.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.ExportOrders.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public Task<AppObjectResponse> GetItemsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var items = await db.ExportOrders.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.ExportOrderNum))
                    items = items.Where(s => s.Num == filter.ExportOrderNum).ToList();

                if (!string.IsNullOrEmpty(filter.CntrNum))
                    items = items.Where(s => s.Records!.FirstOrDefault()!.CntrNum == filter.CntrNum).ToList();

                if (!string.IsNullOrEmpty(filter.Carrier))
                    items = items.Where(s => s.Carrier.ShortName == filter.Carrier).ToList();


                appObjResponse.Object = items.ToArray();
            }

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

    public Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(ExportOrderEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            try
            { 
                // check an Existing item
                var itemExistCheck = await db.Set<ExportOrderEntity>().Where(s => s.Num == item.Num).FirstOrDefaultAsync();
                if (itemExistCheck != null)
                {
                    appObjResponse.ErrorAdd($"Document {item.Num} is exists already.");
                    return appObjResponse;
                }

                item.CreateUser = User!;

                db.Entry(item).State = EntityState.Added;
                db.Entry(item.Carrier).State = EntityState.Unchanged;
                db.Entry(item.VesselCall!).State = EntityState.Unchanged;

                // Documents
                foreach (var document in item.Documents!)
                    db.Entry(document).State = EntityState.Unchanged;

                // Records
                foreach (var record in item.Records!)
                {
                    db.Entry(record).State = EntityState.Added;
                    db.Entry(record.CntrType).State = EntityState.Detached;

                    foreach (var recordContent in record.Contents)
                        db.Entry(recordContent).State = EntityState.Added;
                }

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return appObjResponse;
            }
        }

        return appObjResponse;
    }

    public Task<AppObjectResponse> RemoveItemAsync(ExportOrderEntity item)
    {
        throw new NotImplementedException();
    }
}
