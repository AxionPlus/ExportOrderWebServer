
namespace ExportOrderWebServer.Areas.Document.Provider;

public class DocumentProvider : IDocumentProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private AppObjectResponse appObjResponse = new();

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

            var document = await db.Documents.Include(d => d.Records.Where(r => r != null).OrderBy(r => r.Seq))
                                             .AsNoTracking().AsSplitQuery()
                                             .FirstOrDefaultAsync(d => d.Id == id);

            if (document is null)
                appObjResponse.ErrorAdd("ДТ не найдена.");
            else
                appObjResponse.Object = document;

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Documents.AsNoTracking().ToListAsync();
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var documents = await db.Documents.AsNoTracking()
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
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);
                if (User is null)
                {
                    appObjResponse.ErrorAdd("User NOT found.");
                    return appObjResponse;
                }

                var modifyItem = await db.Documents.Include(d => d.Records).AsSplitQuery().FirstOrDefaultAsync(d => d.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd($"{item.Name} NOT found.");
                    return appObjResponse;
                }
                
                bool isItemExist = db.Documents.Any(s => s.Id != item.Id &&
                                                    !string.IsNullOrWhiteSpace(s.Name) &&
                                                    s.Name == item.Name);

                if (isItemExist)
                {
                    appObjResponse.ErrorAdd($"Декларация уже существует: {item.Name}");
                    return appObjResponse;
                }

                modifyItem.CreateUser = User!;
                modifyItem.Status = item.Status;
                modifyItem.Name = item.Name;
                modifyItem.Type = item.Type;
                modifyItem.Description = item.Description;
                //modifyItem.ContarctNo = item.ContarctNo;
                modifyItem.Shipper = item.Shipper;
                modifyItem.Consignee = item.Consignee;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;                

                /// Compaire new items with an existed
                foreach (var modifyRecord in modifyItem.Records.ToArray())
                    if (!item.Records.Any(s => s.Id == modifyRecord.Id))
                    {
                        db.Entry(modifyRecord).State = EntityState.Deleted;
                        modifyItem.Records.Remove(modifyRecord);
                    }                        
                    else
                    {
                        var itemRecord = item.Records.FirstOrDefault(s => s.Id == modifyRecord.Id);
                        if (itemRecord is not null)
                        {
                            modifyRecord.Seq = itemRecord.Seq;
                            modifyRecord.CommodityName = itemRecord.CommodityName;
                            modifyRecord.CommodityEngName = itemRecord.CommodityEngName;
                            modifyRecord.CommodityHSCode = itemRecord.CommodityHSCode;
                            modifyRecord.IMO = itemRecord.IMO;
                            modifyRecord.UNNO = itemRecord.UNNO;
                            modifyRecord.IsIMO = itemRecord.IsIMO;
                            modifyRecord.NetWt = itemRecord.NetWt;
                            modifyRecord.GrossWt = itemRecord.GrossWt;

                            //db.Entry(modifyRecord).State = EntityState.Modified;
                        }
                    }

                /// Compaire existed items with a new 
                foreach (var itemRecord in item.Records.ToArray())
                    if (modifyItem.Records.Any(s => s.Id == itemRecord.Id))
                        item.Records.Remove(itemRecord);
                    else
                        db.Entry(itemRecord).State = EntityState.Added;

                modifyItem.Records.AddRange(item.Records.OrderBy(s => s.Seq));

                db.Entry(modifyItem).State = EntityState.Modified;

                //var bug = db.ChangeTracker.DebugView.LongView;

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

            try
            {
                /// check an Existing item
                bool isItemExist = db.Documents.Any(s => s.Name == item.Name);
                if (isItemExist)
                {
                    appObjResponse.ErrorAdd($"Декларация уже существует: {item.Name}");
                    return appObjResponse;
                }

                item.CreateUser = User;

                foreach (var record in item.Records)
                    db.Entry(record).State = EntityState.Added;

                db.Entry(item.CreateUser).State = EntityState.Unchanged;
                db.Entry(item).State = EntityState.Added;

                //var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                #region HISTORY
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
                #endregion
            }
            catch (Exception ex)
            {
                appObjResponse.ErrorAdd(ex.Message);
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

                var dbItem = await db.Documents.Where(s => s.Id == id).FirstOrDefaultAsync();

                if (dbItem is null)
                {
                    appObjResponse.ErrorAdd("Record wasn't deleted");
                    return appObjResponse;
                }

                foreach (var Record in dbItem.Records)
                    db.Entry(Record).State = EntityState.Deleted;

                db.Entry(dbItem).State = EntityState.Deleted;

                var bug = db.ChangeTracker.DebugView.LongView;
                await db.SaveChangesAsync();

                return appObjResponse;
            }
        }
        catch (Exception ex)
        {
            appObjResponse.ErrorAdd(ex.Message);
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
                                              //.Distinct()
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

    // RESERVED
    public async Task<AppObjectResponse> NewItemsUploadXmlAsync(List<ReadXmlDocumentRecordDTO> readDTOs, string? UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);
            if (User is null)
            {
                appObjResponse.ErrorAdd($"User not found{UserName}");
                return appObjResponse;
            }

            // ... write Entity
            try
            {                
                List<DocumentEntity> newItems = new();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return appObjResponse;
            }

            return appObjResponse;
        }
    }
}
