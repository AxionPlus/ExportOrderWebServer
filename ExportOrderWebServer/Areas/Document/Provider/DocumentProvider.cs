using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Document.Provider;

public class DocumentProvider : IDocumentProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public DocumentProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Documents.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
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

            var documents = await db.Documents.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Document))
                    documents = documents.Where(s => s.Name == filter.Document).ToList();

                if (!string.IsNullOrEmpty(filter.Shipper))
                    documents = documents.Where(s => s.Shipper!.Name == filter.Shipper).ToList();

                if (!string.IsNullOrEmpty(filter.Consignee))
                    documents = documents.Where(s => s.Consignee!.Name == filter.Consignee).ToList();

                if (!string.IsNullOrEmpty(filter.Commodity))
                    documents = documents.Where(s => s.Records.FirstOrDefault()!.CommodityName == filter.Commodity).ToList();


                appObjResponse.Object = documents.ToArray();
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

    public Task<AppObjectResponse> ModifyItemAsync(DocumentEntity item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(DocumentEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.Documents.Where(s => s.Name == item.Name).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Document {item.Name} is exists already.");
                return appObjResponse;
            }

            item.CreateUser = User!;

            //db.Entry(item.Shipper).State = EntityState.Unchanged;
            //db.Entry(item.Consignee).State = EntityState.Unchanged;
            //db.Entry(item.Type).State = EntityState.Unchanged;
            db.Entry(item).State = EntityState.Added;

            foreach (var records in item.Records)
                db.Entry(records).State = EntityState.Added;

            //foreach (var exportOrder in item.ExportOrders!)
            //    db.Entry(exportOrder).State = EntityState.Unchanged;


            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public Task<AppObjectResponse> RemoveItemAsync(DocumentEntity item)
    {
        throw new NotImplementedException();
    }
}
