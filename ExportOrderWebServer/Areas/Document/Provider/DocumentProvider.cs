using ExportOrderWebServer.DataSet;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<DocumentEntity?> GetDocumentAsync(string Num)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var result = await db.Documents.AsNoTracking().Include(s => s.Records).FirstOrDefaultAsync(s => s.Name == Num);

            if (result is not null)
                return result;
            else
                return null;
        }
    }

    public async Task<DocumentRecord> GetDocumentRecordAsync(string Num, int index)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;            
                
            var result =await db.Set<DocumentRecord>().Include(s => s.Document).AsNoTracking()
                                                 .Where(s => s.Document.Name == Num)
                                                 .FirstOrDefaultAsync(s => s.Seq == index);
            return result;
        }
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Documents.AsNoTracking().Include(d => d.Records).FirstOrDefaultAsync(d => d.Id == id);
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

            var documents = await db.Documents.Include(d => d.Records).ToListAsync();

            if (parameters.GetType() == typeof(string))
                if (!string.IsNullOrWhiteSpace(parameters as string))
                    appObjResponse.Object = documents.Where(s => s.Name == parameters as string);

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Document))
                    documents = documents.Where(s => s.Name == filter.Document).ToList();

                if (!string.IsNullOrEmpty(filter.Shipper))
                    documents = documents.Where(s => s.Shipper!.Name == filter.Shipper).ToList();

                if (!string.IsNullOrEmpty(filter.Consignee))
                    documents = documents.Where(s => s.Consignee!.Name == filter.Consignee).ToList();

                if (!string.IsNullOrEmpty(filter.CargoDescriptionShort))
                    documents = documents.Where(s => s.Description == filter.CargoDescriptionShort).ToList();

                if (filter.Status >= 0)
                    documents = documents.Where(s => s.Status == filter.Status).ToList();
                else
                    documents = documents.Where(s => s.Status == EntityStatus.New).ToList();

                documents = documents.OrderByDescending(x => x.CreateTime).ToList();

                appObjResponse.Object = documents.ToArray();
            }

            return appObjResponse;
        }
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Documents.Select(s => s.Name!).ToListAsync();
        }
    }

    public Task<IEnumerable<string>> GetNamesEn()
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> ModifyItemAsync(DocumentEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.Documents.Include(d => d.Records).FirstOrDefaultAsync(d => d.Id == item.Id);

                if (modifyItem!.Name != item.Name)
                {
                    var itemExistCheck = await db.Documents.Where(s => s.Name!.ToUpper() == item.Name!.ToUpper()).FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.Status = item.Status;
                modifyItem!.Name = item.Name;
                modifyItem!.Type = item.Type;
                modifyItem!.Description = item.Description;
                modifyItem!.ContarctNo = item.ContarctNo;
                modifyItem!.Shipper = item.Shipper;
                modifyItem!.Consignee = item.Consignee;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;                

                // compaire new item with existed
                foreach (var modifyRecord in modifyItem.Records)
                    if (!item.Records.Any(s => s.Id == modifyRecord.Id))
                    {
                        db.Entry(modifyRecord).State = EntityState.Deleted;
                        //modifyItem.Records.Remove(modifyRecord);
                    }                        
                    else
                    {
                        modifyRecord.Id = item.Records.FirstOrDefault(s => s.Id == modifyRecord.Id)!.Id;
                        modifyRecord.Seq = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.Seq;
                        modifyRecord.CommodityName = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.CommodityName;
                        modifyRecord.CommodityEngName = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.CommodityEngName;
                        modifyRecord.CommodityHSCode = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.CommodityHSCode;
                        modifyRecord.IMO= item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.IMO;
                        modifyRecord.UNNO = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.UNNO;
                        modifyRecord.IsIMO = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.IsIMO;
                        modifyRecord.NetWt = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.NetWt;
                        modifyRecord.GrossWt = item.Records!.FirstOrDefault(s => s.Id == modifyRecord.Id)!.GrossWt;
                    }

                // add a new records wich Id == 0 
                foreach (var itemRecord in item.Records.Where(dr => dr.Id == 0))
                {
                    db.Entry(itemRecord).State = EntityState.Added;
                    modifyItem.Records.Add(itemRecord);
                }

                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                // HISTORY
                //db.ChangeTracker.Clear();

                //var historyItemRecords = new List<DocumentRecordHistory>();

                //foreach (var record in item.Records)
                //{
                //    historyItemRecords.Add(new DocumentRecordHistory
                //    {
                //        Id = record.Id,
                //        Seq = record.Seq,
                //        CommodityName = record.CommodityName,
                //        CommodityEngName = record.CommodityEngName,
                //        CommodityHSCode = record.CommodityHSCode,
                //        IsIMO = record.IsIMO,
                //        IMO = record.IMO,
                //        UNNO = record.UNNO,
                //        NetWt = record.NetWt,
                //        GrossWt = record.GrossWt,
                //        Volume = record.Volume
                //    });
                //}

                //var historyItem = new DocumentHistory()
                //{
                //    Status = item.Status,
                //    CreateUser = User,
                //    CreateTime = DateTime.Now,
                //    Mode = HistoryEventMode.Modify,

                //    Id = item.Id,
                //    Name = item.Name,
                //    Type = item.Type,
                //    Description = item.Description,
                //    ContarctNo = item.ContarctNo,
                //    ShipperName = item.Shipper!.NameEn,
                //    ConsigneeName = item.Consignee!.NameEn,
                //    Records = historyItemRecords
                //};

                //db.Entry(historyItem).State = EntityState.Added;

                //await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(msg);

                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(DocumentEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);
            try
            {
                // check an Existing item
                var itemExistCheck = await db.Documents.Where(s => s.Name == item.Name).FirstOrDefaultAsync();
                if (itemExistCheck != null)
                {
                    appObjResponse.ErrorAdd($"Document: \n {item.Name} \n is exists already.");
                    return appObjResponse;
                }

                item.CreateUser = User!;

                db.Entry(item).State = EntityState.Added;

                foreach (var record in item.Records)
                    db.Entry(record).State = EntityState.Added;


                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                // HISTORY
                //db.ChangeTracker.Clear();

                //bool IsItemExists = await db.Documents.AnyAsync(vc => vc.Id == item.Id);
                //if (IsItemExists) { appObjResponse.ErrorAdd($"History not saved"); }
                //else
                //{
                //    var historyItemRecords = new List<DocumentRecordHistory>();

                //    foreach (var record in item.Records)
                //    {
                //        historyItemRecords.Add(new DocumentRecordHistory
                //        {
                //            Id = record.Id,
                //            Seq = record.Seq,
                //            CommodityName = record.CommodityName,
                //            CommodityEngName = record.CommodityEngName,
                //            CommodityHSCode = record.CommodityHSCode,
                //            IsIMO = record.IsIMO,
                //            IMO = record.IMO,
                //            UNNO = record.UNNO,
                //            NetWt = record.NetWt,
                //            GrossWt = record.GrossWt,
                //            Volume = record.Volume
                //        });
                //    }

                //    var historyItem = new DocumentHistory()
                //    {
                //        Status = item.Status,
                //        CreateUser = User,
                //        CreateTime = DateTime.Now,
                //        Mode = HistoryEventMode.New,

                //        Id = item.Id,
                //        Name = item.Name,
                //        Type = item.Type,
                //        Description = item.Description,
                //        ContarctNo = item.ContarctNo,
                //        ShipperName = item.Shipper!.NameEn,
                //        ConsigneeName = item.Consignee!.NameEn,
                //        Records = historyItemRecords
                //    };

                //    db.Entry(historyItem).State = EntityState.Added;

                //    await db.SaveChangesAsync();
                //}
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                appObjResponse.ErrorAdd(error);
                return appObjResponse;
            }

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
                var existedItem = await db.Documents.Where(s => s.Id == id).FirstOrDefaultAsync();

                if (existedItem is null)
                {
                    appObjResponse.ErrorAdd("Record wasn't deleted");
                    return appObjResponse;
                }

                foreach (var Record in existedItem.Records)
                    db.Entry(Record).State = EntityState.Deleted;

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

    public async Task<IEnumerable<DocumentEntity>> GetDocumentItemsAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var result = await db.Documents.AsNoTracking().ToListAsync();
            return result;
        }
    }
}
