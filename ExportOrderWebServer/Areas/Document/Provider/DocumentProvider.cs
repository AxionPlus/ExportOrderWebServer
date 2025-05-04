namespace ExportOrderWebServer.Areas.Document.Provider;

public class DocumentProvider : IDocumentProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private AppObjectResponse appObjResponse;

    public DocumentProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
        appObjResponse = new();
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        //appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Documents.Include(d => d.Records).FirstOrDefaultAsync(d => d.Id == id);    //.AsNoTracking().AsSplitQuery()
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        //appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Documents.AsNoTracking().ToListAsync();
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        //appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var documents = await db.Documents.AsNoTracking()//.Include(d => d.Records)
                                                .Where(s => !string.IsNullOrEmpty(s.Name))
                                                .Where(s => string.IsNullOrEmpty(filter.Document) ? true : s.Name == filter.Document)
                                                .Where(s => string.IsNullOrEmpty(filter.Shipper) ? true :
                                                                s.Shipper == null ? true : s.Shipper.Name == filter.Shipper)
                                                .Where(s => string.IsNullOrEmpty(filter.Consignee) ? true :
                                                                s.Consignee == null ? true : s.Consignee.Name == filter.Consignee)
                                                .Where(s => string.IsNullOrEmpty(filter.CargoDescriptionShort) ? true :
                                                                s.Description == null ? true : s.Description == filter.CargoDescriptionShort)
                                                .Where(s => filter.Status.Equals(null) ? true : s.Status.Equals(filter.Status))
                                                .OrderByDescending(s => s.CreateTime)
                                                .ToListAsync();

            if (documents is null)
                appObjResponse.ErrorAdd("Documents not found.");

            appObjResponse.Object = documents;
            
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
        //appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.Documents.Include(d => d.Records).AsSplitQuery().FirstOrDefaultAsync(d => d.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd($" {item.Name} NOT found.");
                    return appObjResponse;
                }

                if (modifyItem!.Name != item.Name)
                {
                    //var itemExistCheck = await db.Documents.Where(s => s.Name!.ToUpper() == item.Name!.ToUpper()).FirstOrDefaultAsync();

                    //if (itemExistCheck is not null)
                    //{
                    //    appObjResponse.ErrorAdd($" {item.Name} exists already");
                    //    return appObjResponse;
                    //}

                    bool isItemExist = db.Documents.Any(s => s.Name!.ToUpper() == item.Name!.ToUpper());

                    if (isItemExist)
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

                // compaire new item with an existed
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
                appObjResponse.ErrorAdd(ex.Message);
                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(DocumentEntity item)
    {
        //appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);
            try
            {
                // check an Existing item
                bool isItemExist = db.Documents.Any(s => s.Name == item.Name);
                if (isItemExist)
                {
                    appObjResponse.ErrorAdd($"Document: \n {item.Name} \n is exists already.");
                    return appObjResponse;
                }

                item.CreateUser = User!;

                foreach (var record in item.Records)
                    db.Entry(record).State = EntityState.Added;

                db.Entry(item).State = EntityState.Added;

                //var bug = db.ChangeTracker.DebugView.LongView;

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
        //appObjResponse = new();

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

    public async Task<IEnumerable<string>> GetDocumentNames(bool isSelectable)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var documents = await db.Documents.AsNoTracking()
                                            .Where(s => !string.IsNullOrEmpty(s.Name))
                                            .Where(s => isSelectable ? s.Status.Equals(EntityStatus.New) : true)
                                            .Select(s => s.Name!)
                                            .OrderBy(s => s)
                                            .Distinct()                                            
                                            .ToArrayAsync();

            if (documents != null && documents.Any())
                return documents;

            return Enumerable.Empty<string>();            
        }
    }

    public Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        throw new NotImplementedException();        
    }

    public async Task<IEnumerable<DocumentEntity>?> GetItemsAsync(long[] ids)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var documents = await db.Documents.AsNoTracking()
                                              .Include(d => d.Records)
                                              .Where(d => ids.Any(x => x == d.Id))
                                              .ToArrayAsync();

            return documents;
        }
    }

    public async Task<IEnumerable<DocumentEntity>?> GetItemsAsync(string[]? nums)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var documents = await db.Documents.Include(d => d.Records)
                                              .Where(d => nums != null && nums.Any(x => x == d.Name))
                                              .ToListAsync();  //ToArrayAsync

            return documents;
        }
    }
}
