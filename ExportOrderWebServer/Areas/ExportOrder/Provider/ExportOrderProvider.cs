namespace ExportOrderWebServer.Areas.ExportOrder.Provider;

public interface IExportOrderProvider_New
{
    Task<PagedResult<ExportOrderDto>> GetPagedResultAsync(ExportOrderPageFilter filter, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExportOrderFileDto>> GetFilesDto(ExportOrderPageFilter filter, long[]? ids = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExportOrderFileDto>> GetFileDto(long voyageId, FileType fileType, bool isIMO = false, CancellationToken cancellationToken = default);
    Task<ExportOrderEntity> GetItemAsync(long id, CancellationToken cancellationToken = default);
    Task<AppObjectResponse> NewItemAsync(ExportOrderEntity item, string? UserName = "");
    Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item, string? UserName = "");
    Task RemoveItemAsync(long id, CancellationToken cancellationToken = default);
    Task SetNewVesselCall(long[] ids, long vesselCallDetailId, CancellationToken cancellationToken = default);
    Task<VesselCallDetail?> GetVesselCallDetailAsync(long? vcdId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetDocumentExportOrderNums(string documentNum, CancellationToken cancellationToken = default);
    Task<double[]> GetDocumentExportOrderTotalWeights(string documentNum, CancellationToken cancellationToken = default);
    Task<IEnumerable<PersonEntity>> GetPersonsAsync(CancellationToken cancellationToken = default);
    Task<string[]?> GetCntrNumsInVoyage(long eoId, long voyageId, CancellationToken cancellationToken = default);
}

public class ExportOrderProvider_New : IExportOrderProvider_New
{
    protected readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    protected readonly IHttpContextAccessor _httpContextAccessor;    
    protected readonly IHttpClientFactory _http;
    private AppObjectResponse appObjResponse = new();

    public ExportOrderProvider_New(IDbContextFactory<ApplicationDbContext> dbContext, IHttpContextAccessor httpContextAccessor, IHttpClientFactory http)    
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _http = http;
    }

    public async Task<PagedResult<ExportOrderDto>> GetPagedResultAsync(ExportOrderPageFilter filter, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        var query = GetQueryableItems(db, filter);

        /// Получаем общее количество до пагинации
        var totalCount = await query.CountAsync(cancellationToken);

        /// Применяем пагинацию
        var _items = await query.OrderByDescending(q => q.Dated).ThenByDescending(q => q.Num)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        var items = _items.Select(s => EntityToPageDto(s)).ToArray();

        return new PagedResult<ExportOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }

    public async Task<IEnumerable<ExportOrderFileDto>> GetFilesDto(ExportOrderPageFilter filter, long[]? ids = null, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        var query = GetQueryableItems(db, filter);

        ExportOrderEntity[] dbItems = await query.OrderBy(q => q.Num).ToArrayAsync(cancellationToken);
        
        if (ids is not null && ids.Length != 0)
            dbItems = dbItems
                .Where(s => ids.Any(id => id == s.Id)).ToArray();

        return dbItems.Select(s => EntityToFileDto(s, filter.FileType));
    }

    public async Task<IEnumerable<ExportOrderFileDto>> GetFileDto(long voyageId, FileType fileType, bool isIMO = false, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);
        
        var dbItems = await db.Set<ExportOrderEntity>().AsNoTracking()
            .Include(s => s.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Vessel).ThenInclude(vsl => vsl.Flag)
            .Include(s => s.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
            .Include(s => s.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
            .Include(s => s.Person)
            .Include(s => s.Carrier).ThenInclude(c => c!.CarrierDetails)
            .Include(s => s.Carrier).ThenInclude(c => c!.Location).ThenInclude(lo => lo!.Country)
            .Include(s => s.Records.OrderBy(r => r.Id)).ThenInclude(r => r.Contents.OrderBy(c => c.Id)).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
            .Include(x => x.Records).ThenInclude(r => r.Contents).ThenInclude(c => c.SupplementaryUnit)
            .Include(s => s.Records).ThenInclude(r => r.CntrType)            
            .Where(s => s.VesselCallDetail != null && s.VesselCallDetail.VesselCall.Id == voyageId)
            .Where(s => s.Status != EntityStatus.Cancelled)
            .OrderBy(s => s.Num)
            .AsSplitQuery()
            .ToListAsync(cancellationToken)
            ?? throw new ArgumentException($"Export Orders not found for Vessel Call Id: '{voyageId}'.");
        
        if (isIMO)
            dbItems = dbItems.Where(s => s.Records.SelectMany(r => r.Contents).Where(c => c.DocumentRecord.IsIMO == true).Any()).ToList();

        return dbItems.Select(s => EntityToFileDto(s, fileType, isIMO)).ToArray();
    }    

    public async Task SetNewVesselCall(long[] ids, long vesselCallDetailId, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

            var vesselCallDetail = await db.Set<VesselCallDetail>().FirstOrDefaultAsync(s => s.Id == vesselCallDetailId, cancellationToken)
                ?? throw new ArgumentException("Рейс не найден.");

            var exportOrders = await db.ExportOrders.Where(s => ids.Any(id => id == s.Id)).ToArrayAsync(cancellationToken)
                ?? throw new ArgumentException("Поручения не найдены.");

            foreach (var exportOrder in exportOrders)
            {
                exportOrder.VesselCallDetail = vesselCallDetail;

                db.Entry(exportOrder).State = EntityState.Modified;
            }

            //var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync(cancellationToken);

            db.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            throw new Exception($"Voyage Change error: {ex.Message}");
        }
    }

    public async Task<VesselCallDetail?> GetVesselCallDetailAsync(long? vcdId, CancellationToken cancellationToken = default)
    {
        if (vcdId is null) return null;

        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        return await db.Set<VesselCallDetail>().AsNoTracking().AsSplitQuery()
                                                .Include(vcd => vcd.VesselCall).ThenInclude(vc => vc.Terminal)
                                                .Include(vcd => vcd.VesselCall).ThenInclude(vc => vc.Vessel)
                                                .Include(vcd => vcd.POD)
                                                //.Include(vcd => vcd.FinalDestination)
                                                .FirstOrDefaultAsync(vcd => vcd.Id == vcdId)
            ?? throw new ArgumentException($"Voyage not found for vcdId={vcdId}");
    }

    private static IQueryable<ExportOrderEntity> GetQueryableItems(ApplicationDbContext db, ExportOrderPageFilter filter)
    {
        var query = db.Set<ExportOrderEntity>().AsNoTracking()
            .Include(s => s.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Vessel).ThenInclude(vsl => vsl.Flag)
            .Include(s => s.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
            .Include(s => s.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
            .Include(s => s.Person)
            .Include(s => s.Carrier).ThenInclude(c => c!.CarrierDetails)
            .Include(s => s.Records.OrderBy(r => r.Id)).ThenInclude(r => r.Contents.OrderBy(c => c.Id)).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
            .Include(x => x.Records).ThenInclude(r => r.Contents).ThenInclude(c => c.SupplementaryUnit)
            .Include(s => s.Records).ThenInclude(r => r.CntrType)
            .AsSplitQuery().AsQueryable()
            ?? throw new ArgumentException($"No Db records requested.");

        // Номер Поручения
        if (!string.IsNullOrWhiteSpace(filter.Num))
            if (filter.Num.Contains("select", StringComparison.CurrentCultureIgnoreCase))
                query = query.Where(s => filter.Nums.Any(x => x == s.Num));
            else
                query = query.Where(s => s.Num == filter.Num);
        
        // Номер контейнера
        if (!string.IsNullOrWhiteSpace(filter.ContainerNum))
            if (filter.ContainerNum.Contains("select", StringComparison.CurrentCultureIgnoreCase))
                query = query.Where(s => s.Records.Any(cr => filter.ContainerNums.Any(x => x == cr.CntrNum)));
            else
                query = query.Where(s => s.Records.Any(cr => cr.CntrNum == filter.ContainerNum));

        // Клиент
        if (!string.IsNullOrWhiteSpace(filter.Carrier))
            query = query.Where(s => s.Carrier != null && s.Carrier.NameEn == filter.Carrier);

        // Судно
        if (!string.IsNullOrWhiteSpace(filter.VesselName))
            query = query.Where(s => s.VesselCallDetail != null &&
                                        s.VesselCallDetail.VesselCall.Vessel.Name != null &&
                                        s.VesselCallDetail.VesselCall.Vessel.Name == filter.VesselName);

        // Судозаход
        if (!string.IsNullOrWhiteSpace(filter.VesselVoyage))
            query = query.Where(s => s.VesselCallDetail != null &&
                                     s.VesselCallDetail.VesselCall.VoyageNo == filter.VesselVoyage);

        // Порт
        if (!string.IsNullOrWhiteSpace(filter.PortOfDischarge))
            query = query.Where(s => s.VesselCallDetail != null &&
                                        s.VesselCallDetail.POD != null &&
                                        s.VesselCallDetail.POD.Name == filter.PortOfDischarge);

        // Диапазон дат - From
        if (filter.DateFrom.HasValue)
            query = query.Where(s => s.Dated.HasValue && s.Dated >= filter.DateFrom);
        // Диапазон дат - To
        if (filter.DateTo.HasValue)
            query = query.Where(s => s.Dated.HasValue && s.Dated <= filter.DateTo);

        // IMO
        if (filter.IsIMO == true)
            query = query.Where(s => s.Records.Any(r => r.Contents.Any(c => c.DocumentRecord.IsIMO)));

        return query;
    }

    public async Task<ExportOrderEntity> GetItemAsync(long id, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        return await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                    .Where(eo => eo.Carrier != null)
                                    .Where(eo => eo.VesselCallDetail != null)
                                    .Include(eo => eo.Carrier!).ThenInclude(c => c.CarrierDetails)
                                    .Include(eo => eo.Person)
                                    .Include(eo => eo.Documents).ThenInclude(d => d.Records)
                                    .Include(eo => eo.Records).ThenInclude(r => r.CntrType)
                                    .Include(eo => eo.Records).ThenInclude(r => r.Contents).ThenInclude(c => c.SupplementaryUnit)
                                    .Include(eo => eo.Records).ThenInclude(r => r.Contents).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
                                    .Include(eo => eo.VesselCallDetail!).ThenInclude(vcd => vcd.POD)
                                    .Include(eo => eo.VesselCallDetail!).ThenInclude(vcd => vcd.VesselCall).ThenInclude(vc => vc.Vessel)
                                    .Include(eo => eo.VesselCallDetail!).ThenInclude(vcd => vcd.VesselCall).ThenInclude(vc => vc.Terminal)
                                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
        ?? throw new ArgumentException($"Export Order not found for Id = {id}");
    }

    public async Task<AppObjectResponse> NewItemAsync(ExportOrderEntity item, string? UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName); //ApplicationParameter.ApplicationUser
            if (User is null)
            {
                appObjResponse.ErrorAdd("User not found.");
                return appObjResponse;
            }

            try
            {
                /// check an Existing item
                bool isItemExist = db.Set<ExportOrderEntity>().Any(s => s.Num.ToUpper() == item.Num.ToUpper());
                if (isItemExist)
                {
                    appObjResponse.ErrorAdd($"Export Order {item.Num} exists already.");
                    return appObjResponse;
                }

                item.CreateUser = User;

                db.Entry(item.CreateUser).State = EntityState.Unchanged;
                db.Entry(item.Carrier!).State = EntityState.Unchanged;
                db.Entry(item.Person!).State = EntityState.Unchanged;
                db.Entry(item.VesselCallDetail!).State = EntityState.Unchanged;

                db.Entry(item).State = EntityState.Added;

                /// Documents
                foreach (var document in item.Documents)
                    db.Entry(document).State = EntityState.Unchanged;

                /// Records
                foreach (var record in item.Records!)
                {
                    db.Entry(record).State = EntityState.Added;
                    db.Entry(record.CntrType!).State = EntityState.Detached;

                    foreach (var content in record.Contents)
                        db.Entry(content).State = EntityState.Added;
                }

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                #region HISTORY

                //db.ChangeTracker.Clear();

                //bool IsItemExists = await db.ExportOrders.AnyAsync(vc => vc.Id == item.Id);
                //if (IsItemExists) { appObjResponse.ErrorAdd($"History not saved"); }
                //else
                //{
                //    // documents
                //    var historyItemDocuments = new List<DocumentHistory>();

                //    foreach (var document in item.Documents)
                //    {
                //        historyItemDocuments.Add(new DocumentHistory()
                //        {
                //            Name = document.Name,
                //            Type = document.Type,
                //            ShipperName = document.Shipper!.NameEn,
                //            ConsigneeName = document.Consignee!.NameEn,
                //        });
                //    }

                //    // records
                //    var historyItemRecords = new List<ExportOrderRecordHistory>();

                //    foreach (var record in item.Records)
                //    {
                //        var historyItemRecordContents = new List<ContainerContentHistory>();

                //        foreach (var content in record.Contents)
                //        {
                //            historyItemRecordContents.Add(new ContainerContentHistory()
                //            {
                //                Id = content.Id,
                //                PackageQty = content.PackageQty,
                //                PackageName = content.PackageName,
                //                NetWt = content.NetWt,
                //                GrossWt = content.GrossWt,
                //                Volume = content.Volume,
                //                Document = content.DocumentRecord.Document.Name,
                //                SeqDocument = content.DocumentRecord.Seq,
                //                CommodityName = content.DocumentRecord.CommodityName,
                //                CommodityEngName = content.DocumentRecord.CommodityEngName,
                //                IsIMO = content.DocumentRecord.IsIMO,
                //                IMO = content.DocumentRecord.IMO,
                //                UNNO = content.DocumentRecord.UNNO,
                //            });
                //        }

                //        historyItemRecords.Add(new ExportOrderRecordHistory()
                //        {
                //            Id = record.Id,
                //            CntrNum = record.CntrNum,
                //            CntrType = record.CntrType!.Normolize,
                //            CntrTareWt = record.CntrTareWt,
                //            Seal = record.Seal,
                //            Contents = historyItemRecordContents
                //        });
                //    }

                //    var historyItem = new ExportOrderHistory()
                //    {
                //        Status = item.Status,
                //        CreateUser = User,
                //        CreateTime = DateTime.Now,
                //        Mode = HistoryEventMode.New,

                //        Num = item.Num,
                //        Dated = item.Dated,
                //        Carrier = item.Carrier!.NameEn,
                //        Person = item.Person!.FamilyName,
                //        VoyageNo = item.VesselCallDetail.VesselCall.VoyageNo,
                //        VesselName = item.VesselCallDetail.VesselCall.Vessel.Name,
                //        CommodityShort = item.CommodityShort,
                //        POD = item.VesselCallDetail.POD!.NameEn,
                //        Documents = historyItemDocuments,
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
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item, string? UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            db.Database.SetCommandTimeout(560);

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);
            if (User is null)
            {
                appObjResponse.ErrorAdd($"User not found{UserName}");
                return appObjResponse;
            }

            try
            {
                var modifyItem = await db.ExportOrders.AsTracking().AsSplitQuery()
                                                      .Include(eo => eo.Documents)
                                                      .Include(eo => eo.Records).ThenInclude(d => d.Contents).ThenInclude(c => c.DocumentRecord)
                                                      .FirstOrDefaultAsync(s => s.Id == item.Id);
                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd("Export Order is not exists.");
                    return appObjResponse;
                }

                /// check CntrNum duplicate
                if (modifyItem.Num != item.Num)
                {
                    bool isItemExist = db.ExportOrders.Any(s => s.Num.ToUpper() == item.Num.ToUpper());

                    if (isItemExist)
                    {
                        appObjResponse.ErrorAdd($"Export Order {item.Num} dtd {item.Dated!.Value.ToShortDateString()} exists already");
                        return appObjResponse;
                    }
                }

                /// DELETE AN EXISTING modifyItem.Records
                if (modifyItem.Records.Count > 0)
                {
                    foreach (var record in modifyItem.Records)
                    {
                        foreach (var content in record.Contents)
                            db.Entry(content).State = EntityState.Deleted;

                        db.Entry(record).State = EntityState.Deleted;
                    }

                    modifyItem.Records.Clear();
                }

                /// DOCUMENTS
                foreach (var modifyItemDocument in modifyItem.Documents.ToArray())
                    if (!item.Documents.Any(s => s.Id == modifyItemDocument.Id))
                        modifyItem.Documents.Remove(modifyItemDocument);

                foreach (var itemDocument in item.Documents)
                    if (!modifyItem.Documents.Any(s => s.Id == itemDocument.Id))
                        modifyItem.Documents.Add(itemDocument);

                /// RE-WRITE WITH A NEW ITEM
                modifyItem.CreateUser = User;
                modifyItem.Num = item.Num;
                modifyItem.Dated = item.Dated;
                modifyItem.Carrier = item.Carrier;
                modifyItem.Person = item.Person;
                modifyItem.VesselCallDetail = item.VesselCallDetail;
                modifyItem.CommodityShort = item.CommodityShort;
                modifyItem.CommodityShortEn = item.CommodityShortEn;
                modifyItem.Status = item.Status;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;
                db.Entry(modifyItem.Carrier!).State = EntityState.Unchanged;
                db.Entry(modifyItem.Person!).State = EntityState.Unchanged;
                db.Entry(modifyItem.VesselCallDetail!).State = EntityState.Unchanged;

                /// RECORDS
                foreach (var record in item.Records)
                {
                    db.Entry(record).State = EntityState.Added;
                    modifyItem.Records.Add(record);

                    foreach (var content in record.Contents)
                        db.Entry(content).State = EntityState.Added;
                }

                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                #region HISTORY                
                //db.ChangeTracker.Clear();

                //// documents
                //var historyItemDocuments = new List<DocumentHistory>();

                //foreach (var document in item.Documents)
                //{
                //    historyItemDocuments.Add(new DocumentHistory()
                //    {
                //        Name = document.Name,
                //        Type = document.Type,
                //        ShipperName = document.Shipper!.NameEn,
                //        ConsigneeName = document.Consignee!.NameEn,
                //    });
                //}

                //// records
                //var historyItemRecords = new List<ExportOrderRecordHistory>();

                //foreach (var record in item.Records)
                //{
                //    var historyItemRecordContents = new List<ContainerContentHistory>();

                //    foreach (var content in record.Contents)
                //    {
                //        historyItemRecordContents.Add(new ContainerContentHistory()
                //        {
                //            Id = content.Id,
                //            PackageQty = content.PackageQty,
                //            PackageName = content.PackageName,
                //            NetWt = content.NetWt,
                //            GrossWt = content.GrossWt,
                //            Volume = content.Volume,
                //            Document = content.DocumentRecord.Document.Name,
                //            SeqDocument = content.DocumentRecord.Seq,
                //            CommodityName = content.DocumentRecord.CommodityName,
                //            CommodityEngName = content.DocumentRecord.CommodityEngName,
                //            IsIMO = content.DocumentRecord.IsIMO,
                //            IMO = content.DocumentRecord.IMO,
                //            UNNO = content.DocumentRecord.UNNO,
                //        });
                //    }

                //    historyItemRecords.Add(new ExportOrderRecordHistory()
                //    {
                //        Id = record.Id,
                //        CntrNum = record.CntrNum,
                //        CntrType = record.CntrType!.Normolize,
                //        CntrTareWt = record.CntrTareWt,
                //        Seal = record.Seal,
                //        Contents = historyItemRecordContents
                //    });
                //}

                //var historyItem = new ExportOrderHistory()
                //{
                //    Status = item.Status,
                //    CreateUser = User,
                //    CreateTime = DateTime.Now,
                //    Mode = HistoryEventMode.Modify,

                //    Num = item.Num,
                //    Dated = item.Dated,
                //    Carrier = item.Carrier!.NameEn,
                //    Person = item.Person!.FamilyName,
                //    VoyageNo = item.VesselCallDetail!.VesselCall.VoyageNo,
                //    VesselName = item.VesselCallDetail.VesselCall.Vessel.Name,
                //    CommodityShort = item.CommodityShort,
                //    POD = item.VesselCallDetail.POD!.NameEn,
                //    Documents = historyItemDocuments,
                //    Records = historyItemRecords
                //};

                //db.Entry(historyItem).State = EntityState.Added;

                //await db.SaveChangesAsync();
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                appObjResponse.ErrorAdd(ex.Message);
                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public async Task RemoveItemAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var _db = _dbContext.CreateDbContextAsync();
            
            var db = await _db;

            /// Get Existing item
            var dbItem = await db.ExportOrders.Include(eo => eo.Records).ThenInclude(d => d.Contents)
                                              .Include(eo => eo.Documents)
                                              .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
                ?? throw new ArgumentException("Item NOT found.");

            /// RECORDS
            if (dbItem.Records.Any())
                foreach (var record in dbItem.Records)
                {
                    if (record.Contents.Any())
                        foreach (var content in record.Contents)
                            db.Entry(content).State = EntityState.Deleted;

                    db.Entry(record).State = EntityState.Deleted;
                }
            
            /// DELETE
            db.Entry(dbItem).State = EntityState.Deleted;
            //var bug = db.ChangeTracker.DebugView.LongView;
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw new Exception(ex.Message);
        }
    }

    public async Task<IEnumerable<PersonEntity>> GetPersonsAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        return await db.Persons.AsNoTracking().ToListAsync(cancellationToken)
            ?? throw new ArgumentException("Persons not found");
    }

    public async Task<IEnumerable<string>> GetDocumentExportOrderNums(string documentNum, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        return await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                        .Include(s => s.Records).ThenInclude(er => er.Contents).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
                                        .Where(s => s.Records.SelectMany(er => er.Contents)
                                                             .Any(c => !string.IsNullOrWhiteSpace(c.DocumentRecord.Document.Name) &&
                                                                        c.DocumentRecord.Document.Name == documentNum))
                                        .Select(s => new string(string.Concat(s.Num, " (", s.Status.ToString(), ")")))
                                        .Order()
                                        .ToArrayAsync(cancellationToken);

    }

    public async Task<double[]> GetDocumentExportOrderTotalWeights(string documentNum, CancellationToken cancellationToken = default)
    {
        /// Retrieves total NET & total GROSS weights of ExportOrders, which are used in Document
        
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        var containerContents = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                              .Include(s => s.Records).ThenInclude(er => er.Contents).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
                                              .SelectMany(s => s.Records).SelectMany(eor => eor.Contents)
                                              .Where(con => !string.IsNullOrWhiteSpace(con.DocumentRecord.Document.Name) &&
                                                con.DocumentRecord.Document.Name == documentNum)
                                              .ToArrayAsync(cancellationToken);

        double? net = containerContents.Sum(con => con.NetWt);
        double? gross = containerContents.Sum(con => con.GrossWt);

        double[] total = { net ?? 0, gross ?? 0 };

        return total;        
    }

    public async Task<string[]?> GetCntrNumsInVoyage(long eoId, long voyageId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

        var cntrNums = await db.ExportOrders.Include(eo => eo.Records)
                                                .Where(eo => eo.VesselCallDetail != null)
                                                .Include(eo => eo.VesselCallDetail!.VesselCall)
                                                .AsNoTracking().AsSplitQuery()
                                                .Where(eo => eo.Id != eoId && eo.VesselCallDetail!.VesselCall.Id == voyageId)
                                                .SelectMany(eo => eo.Records).Select(eor => eor.CntrNum)
                                                .ToArrayAsync(cancellationToken);    //ToListAsync()

        if (cntrNums is not null && cntrNums.Any())
            return cntrNums;

        return null;
    }

    #region AUXILAIRY

    private static ExportOrderDto EntityToPageDto (ExportOrderEntity entity)
    {
        return new ExportOrderDto()
        {
            Id = entity.Id,
            Num = entity.Num,
            Dated = entity.Dated,
            VesselCallId = entity.VesselCallDetail!.VesselCall.Id,
            VesselName = entity.VesselCallDetail?.VesselCall.Vessel.Name,
            VesselVoyage = entity.VesselCallDetail?.VesselCall.VoyageNo,
            PortOfDischarge = entity.VesselCallDetail?.POD?.Name,
            CarrierId = entity.Carrier == null ? 0 : entity.Carrier.Id,
            CarrierNameEn = entity.Carrier?.NameEn,
            TerminalName = entity.VesselCallDetail?.VesselCall.Terminal.Name,
            IsIMO = entity.Records.Any(eor => eor.Contents.Any(c => c.DocumentRecord.IsIMO.Equals(true)).Equals(true)),
            IsEmpty = entity.Records.Any(eor => eor.Contents.Any(c => c.DocumentRecord.CommodityEngName.ToUpper().Contains("EMPTY")).Equals(true)),
            Status = entity.Status
        };
    }

    private static ExportOrderFileDto EntityToFileDto(ExportOrderEntity s, FileType fileType, bool isIMO = false)
    {
        try
        {
            return new ExportOrderFileDto()
            {
                Id = s.Id,
                Num = s.Num,
                Dated = s.Dated,
                BLNum = s.Num.Contains('_') ? s.Num[..s.Num.IndexOf("_")] : s.Num,
                BLtemplate = s.Carrier?.BlTemplate ?? BLTemplate.Standard,
                BLDate = s.VesselCallDetail?.VesselCall.ETS?.ToString("dd.MM.yyyy"),
                LoadingDate = s.VesselCallDetail?.VesselCall.ETA?.ToString("dd.MM.yyyy"),
                
                CarrierNameEn = s.Carrier?.NameEn,
                CarrierLocation = s.Carrier?.Location?.Name,
                CarrierCountryEn = s.Carrier?.Location?.Country?.ENG,
                CarrierContract = s.Carrier?.CarrierDetails?.Any(cd => cd.TerminalName == s.VesselCallDetail?.VesselCall.Terminal.Name) == true
                    ? s.Carrier?.CarrierDetails?.FirstOrDefault(cd => cd.TerminalName == s.VesselCallDetail?.VesselCall.Terminal.Name)!.Contract
                    : string.Empty,
                CarrierContractDate = s.Carrier?.CarrierDetails?.Any(cd => cd.TerminalName == s.VesselCallDetail?.VesselCall.Terminal.Name) == true
                    ? s.Carrier?.CarrierDetails?.FirstOrDefault(cd => cd.TerminalName == s.VesselCallDetail?.VesselCall.Terminal.Name)!.DateContract?.ToString("dd.MM.yyyy")
                    : string.Empty,

                VesselCallId = s.VesselCallDetail != null ? s.VesselCallDetail.VesselCall.Id : 0,
                VoyageNum = s.VesselCallDetail?.VesselCall.VoyageNo,
                VesselName = s.VesselCallDetail?.VesselCall.Vessel.Name,
                VesselFlag = s.VesselCallDetail?.VesselCall.Vessel.Flag?.RUS,
                VesselFlagEn = s.VesselCallDetail?.VesselCall.Vessel.Flag?.ENG,
                CaptainFamily = s.VesselCallDetail?.VesselCall.Vessel.CaptainFamily,
                CaptainName = s.VesselCallDetail?.VesselCall.Vessel.CaptainName,

                Shippers = s.Records.SelectMany(r => r.Contents)
                                    .Where(c => c.DocumentRecord.Document.Shipper != null && !string.IsNullOrWhiteSpace(c.DocumentRecord.Document.Shipper.Name))
                                    .Select(c => c.DocumentRecord.Document.Shipper!.Name!)
                                    .GroupBy(x => x).Select(g => g.Key).ToArray(),
                ShippersEn = s.Records.SelectMany(r => r.Contents)
                                    .Where(c => c.DocumentRecord.Document.Shipper != null && !string.IsNullOrWhiteSpace(c.DocumentRecord.Document.Shipper.NameEn))
                                    //.Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
                                    .Select(c => c.DocumentRecord.Document.Shipper!.NameEn!)
                                    .GroupBy(x => x).Select(g => g.Key).ToArray(),

                Consignees = s.Records.SelectMany(r => r.Contents)
                                      .Where(c => c.DocumentRecord.Document.Consignee != null && !string.IsNullOrWhiteSpace(c.DocumentRecord.Document.Consignee.Name))
                                      .Select(c => c.DocumentRecord.Document.Consignee!.Name!)
                                      .GroupBy(x => x).Select(g => g.Key).ToArray(),
                ConsigneesEn = s.Records.SelectMany(r => r.Contents)
                                      .Where(c => c.DocumentRecord.Document.Consignee != null && !string.IsNullOrWhiteSpace(c.DocumentRecord.Document.Consignee.NameEn))
                                      //.Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
                                      .Select(c => c.DocumentRecord.Document.Consignee!.NameEn!)
                                      .GroupBy(x => x).Select(g => g.Key).ToArray(),

                Commodities = s.Records.SelectMany(r => r.Contents)
                                       .Select(c => c.DocumentRecord)                                    
                                       .GroupBy(dr => dr.CommodityName).Select(g => 
                                           string.Concat(
                                               g.Key,
                                               g.FirstOrDefault()!.IsIMO ? string.Concat(" IMO:", g.FirstOrDefault()!.IMO, " UNNO:", g.FirstOrDefault()!.UNNO) : "")
                                       ).ToArray(),

                CommoditiesEn = s.Records.SelectMany(r => r.Contents)
                                         .Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
                                         .Select(c => c.DocumentRecord)                                         
                                         .GroupBy(dr => dr.CommodityEngName).Select(g =>
                                             string.Concat(
                                                 g.Key,
                                                 g.FirstOrDefault()!.IsIMO ? string.Concat(" IMO:", g.FirstOrDefault()!.IMO, " UNNO:", g.FirstOrDefault()!.UNNO) : "")
                                         ).ToArray(),
                
                CommodityShort = s.CommodityShort,
                CommodityShortEn = s.CommodityShortEn,
                
                CustomsOfficeCode = s.VesselCallDetail?.VesselCall.Terminal.Customs?.Code,
                CustomsOfficeName = s.VesselCallDetail?.VesselCall.Terminal.Customs?.Office,
                CustomsOfficeNameShort = s.VesselCallDetail?.VesselCall.Terminal.Customs?.OfficeShort,
                CustomsDapartment = s.VesselCallDetail?.VesselCall.Terminal.Customs?.Dapartment,                
                TerminalName = s.VesselCallDetail == null ? string.Empty : s.VesselCallDetail.VesselCall.Terminal.Name,

                //VesselName = s.VesselCallDetail == null ? string.Empty :
                //                        string.IsNullOrEmpty(s.VesselCallDetail.VesselCall.Vessel.Name) ? string.Empty :
                //                            s.VesselCallDetail.VesselCall.Vessel.Name,
                //VesselFlag = s.VesselCallDetail == null ? string.Empty :
                //                        s.VesselCallDetail.VesselCall.Vessel.Flag == null ? string.Empty :
                //                            s.VesselCallDetail.VesselCall.Vessel.Flag.RUS,
                //VesselFlagEn = s.VesselCallDetail == null ? string.Empty :
                //                        s.VesselCallDetail.VesselCall.Vessel.Flag == null ? string.Empty :
                //                            s.VesselCallDetail.VesselCall.Vessel.Flag.ENG,
                //VoyageNum = s.VesselCallDetail == null ? string.Empty : s.VesselCallDetail.VesselCall.VoyageNo,
                PortOfDischarge = string.Concat(s.VesselCallDetail?.POD?.Name, ", ", s.VesselCallDetail?.POD?.Country?.RUS),
                PortOfDischargeEn = string.Concat(s.VesselCallDetail?.POD?.NameEn, ", ", s.VesselCallDetail?.POD?.Country?.ENG),
                PortOfDischargeEnCountryRus = string.Concat(s.VesselCallDetail?.POD?.NameEn, ", ", s.VesselCallDetail?.POD?.Country?.RUS),
                PortOfDischargeUnlocode = s.VesselCallDetail?.POD?.UnLocode,
                //PODEnCountryEn = string.Concat(s.VesselCallDetail?.POD?.NameEn, ", ", s.VesselCallDetail?.POD?.Country?.ENG),
                //PlaceReceipt = string.Empty,
                //PlaceDelivery =string.Empty,
                //FinalDestination = s.VesselCallDetail == null ? string.Empty :
                //                                s.VesselCallDetail.FinalDestination == null ? string.Empty :
                //                                    s.VesselCallDetail.FinalDestination.Country == null ? string.Empty :
                //                                        string.Concat(s.VesselCallDetail.FinalDestination.NameEn, ", ", s.VesselCallDetail.FinalDestination.Country.ENG),
                POLAgent = s.Carrier == null ? string.Empty :
                                    s.VesselCallDetail == null ? string.Empty :
                                        s.VesselCallDetail.VesselCall.Terminal.Name == null ? string.Empty :
                                            s.Carrier.CarrierDetails.FirstOrDefault(cd => cd.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.AgentPOL,
                PODAgent = s.VesselCallDetail == null ? string.Empty : s.VesselCallDetail.AgentPOD,

                Contract = s.Carrier == null ? null :
                                    s.VesselCallDetail == null ? null :
                                        string.IsNullOrEmpty(s.VesselCallDetail.VesselCall.Terminal.Name) ? null :
                                            s.Carrier.CarrierDetails.FirstOrDefault(c => c.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.Contract,
                ContractDate = s.Carrier == null ? null :
                                        s.VesselCallDetail == null ? null :
                                            string.IsNullOrEmpty(s.VesselCallDetail.VesselCall.Terminal.Name) ? null :
                                                s.Carrier.CarrierDetails.FirstOrDefault(c => c.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract.HasValue ?
                                                    s.Carrier.CarrierDetails.FirstOrDefault(c => c.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") : "",
                MyCompanyName = s.Person?.CompanyName,
                MyCompanyEmail = s.Person?.CompanyEmail,
                
                PersonFamily = s.Person?.FamilyName,
                PersonNameSurname = string.Concat(s.Person?.Name, " ", s.Person?.SurName),
                PersonPass = s.Person?.Passport,
                PersonBirthYear = s.Person?.BirthYear,
                PersonBirthPlace = s.Person?.BirthPlace,
                PersonAddress = s.Person?.Address,
                PersonCompany = s.Person?.CompanyName,
                PersonSign = string.Concat(s.Person?.Name?[..1], ". ", s.Person?.SurName?[..1], ". ", s.Person?.FamilyName),
                PersonXml = string.Concat(s.Person?.Name, " ", s.Person?.FamilyName, " телефон: ", s.Person?.Phone),

                Records = fileType switch
                {                    
                    FileType.Bill => s.Records.Select(rec => EntityRecordToFileBillDto(rec)).ToList(),
                    FileType.Customs => s.Records.Select(rec => EntityRecordToFileManifestDto(rec)).ToList(),                    
                    FileType.Manifest => s.Records.Select(rec => EntityRecordToFileManifestDto(rec)).ToList(),
                    _ => EntityRecordToFileOrderDto(s.Records)
                }
            };
        }
        catch(NullReferenceException ex)
        {
            throw new NullReferenceException($"One of the Item's reference is null. Error: {ex.Message}");
        }
        catch(Exception ex)
        {
            throw new Exception($"{ex.Message}");
        }        
    }
       
    private static ExportOrderRecordFileDto EntityRecordToFileBillDto(ExportOrderRecord record)
    {
        return new ExportOrderRecordFileDto()
        {
            Id = record.Id,
            ContainerNum = record.CntrNum,
            CntrType = record.CntrType!.Normolize!,
            CntrTareWt = record.CntrTareWt,
            Seal = record.Seal,

            PackageQty = (uint)record.Contents.Sum(c => c.PackageQty)! > 0 ? (uint)record.Contents.Sum(c => c.PackageQty)! : null,
            PackageNames = string.Join(", ", record.Contents.Select(rc => rc.PackageName is not null ? rc.PackageName.ToUpper() : "").Distinct().Order()),
            NetWt = record.Contents.Sum(c => c.NetWt),
            GrossWt = record.Contents.Sum(c => c.GrossWt),
            Volume = record.Contents.Sum(c => c.Volume),
            Measurement = record.Contents.Sum(c => c.Volume) > 0 ? "cbm" : "kg",

            CommodityEn = string.Join("; ", record.Contents.GroupBy(c => c.DocumentRecord.CommodityEngName).Select(g => g.Key).ToArray())
        };
    }

    private static ExportOrderRecordFileDto EntityRecordToFileManifestDto(ExportOrderRecord record) //, bool isIMO = false
    {
        return new ExportOrderRecordFileDto()
        {
            Id = record.Id,
            ContainerNum = record.CntrNum,
            CntrType = record.CntrType!.Normolize!,
            CntrTareWt = record.CntrTareWt,
            Seal = record.Seal,

            PackageQty = (uint)record.Contents.Sum(c => c.PackageQty)! > 0 ? (uint)record.Contents.Sum(c => c.PackageQty)! : null,
            //PackageQty = (uint)record.Contents.Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
            //                                  .Sum(c => c.PackageQty)! > 0
            //    ? (uint)record.Contents.Where(c => c.DocumentRecord.IsIMO == true)
            //                           .Sum(c => c.PackageQty)!
            //    : null,
            //PackageNames = string.Join(", ",
            //    record.Contents.Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
            //                   .Select(rc => rc.PackageName is not null ? rc.PackageName.ToUpper() : "").Distinct().Order()),
            PackageNames = string.Join(", ", record.Contents.GroupBy(c => c.PackageName).Select(g => g.Key).Order()),
            //GrossWt = record.Contents.Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
            //                         .Sum(c => c.GrossWt),
            GrossWt = record.Contents.Sum(c => c.GrossWt),
            //GrossAndTare = record.CntrTareWt +
            //    record.Contents.Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
            //                   .Sum(c => c.GrossWt),
            GrossAndTare = record.CntrTareWt + record.Contents.Sum(c => c.GrossWt),
            Commodity = string.Join(", ",
                record.Contents
                      //.Where(c => isIMO ? c.DocumentRecord.IsIMO == true : true)
                      .Select(c => c.DocumentRecord)
                      .GroupBy(dr => dr.CommodityName)
                      .Select(g =>
                          string.Concat(
                              g.Key,
                              g.FirstOrDefault()!.IsIMO ? string.Concat(" IMO:", g.FirstOrDefault()!.IMO, " UNNO:", g.FirstOrDefault()!.UNNO) : "")
                      ).ToArray().Order()
            ),
            //ShipperEn = string.Join("; ", record.Contents.Select(c => c.DocumentRecord.Document.Shipper?.NameEn).Distinct().ToArray()),
            //ShipperCountryEn = string.Join("; ", record.Contents.Select(c => c.DocumentRecord.Document.Shipper?.CountryENG).Distinct().ToArray()),
            //ConsigneeEn = string.Join("; ", record.Contents.Select(c => c.DocumentRecord.Document.Consignee?.NameEn).Distinct().ToArray()),
            //ConsigneeCountryEn = string.Join("; ", record.Contents.Select(c => c.DocumentRecord.Document.Consignee?.CountryENG).Distinct().ToArray()),

            ShipperEn = string.Join("; ", record.Contents.GroupBy(c => c.DocumentRecord.Document.Shipper?.NameEn).Select(g => g.Key).ToArray()),
            ShipperCountryEn = string.Join("; ", record.Contents.GroupBy(c => c.DocumentRecord.Document.Shipper?.CountryENG).Select(g => g.Key).ToArray()),
            ConsigneeEn = string.Join("; ", record.Contents.GroupBy(c => c.DocumentRecord.Document.Consignee?.NameEn).Select(g => g.Key).ToArray()),
            ConsigneeCountryEn = string.Join("; ", record.Contents.GroupBy(c => c.DocumentRecord.Document.Consignee?.CountryENG).Select(g => g.Key).ToArray()),

            IMO = string.Join(", ", record.Contents.Where(c => c.DocumentRecord.IsIMO).Select(c => c.DocumentRecord.IMO).Distinct()),
            UNNO = string.Join(", ", record.Contents.Where(c => c.DocumentRecord.IsIMO).Select(c => c.DocumentRecord.UNNO).Distinct()),
        };
    }

    private static List<ExportOrderRecordFileDto> EntityRecordToFileOrderDto(List<ExportOrderRecord> records)
    {
        List<ExportOrderRecordFileDto> dtoRecords = new();

        uint indexRec = 0;

        foreach (ExportOrderRecord record in records)
        {
            ExportOrderRecordFileDto dtoRecord = new()
            {
                Id = record.Id,
                Seq = ++indexRec,
                CntrType = record.CntrType?.Normolize,
                CntrTareWt = record.CntrTareWt,
                Seal = record.Seal
            };

            int contentIndex = 1;

            foreach (var content in record.Contents)
            {
                //dtoRecord.ContentId = content.Id;
                dtoRecord.SeqContent = (uint)content.DocumentRecord.Seq;
                dtoRecord.ContainerNum = record.CntrNum;
                dtoRecord.PackageQty = content.PackageQty;
                dtoRecord.NetWt = content.NetWt;
                dtoRecord.GrossWt = content.GrossWt;
                //dtoRecord.GrossAndTare = record.CntrTareWt + (content.GrossWt.HasValue ? content.GrossWt : 0);
                dtoRecord.GrossAndTare = (content.GrossWt.HasValue ? content.GrossWt : 0) + (contentIndex == 1 ? record.CntrTareWt : 0);

                dtoRecord.DocumentName = content.DocumentRecord.Document.Name;
                dtoRecord.DocumentType = content.DocumentRecord.Document.Type.ToString();
                dtoRecord.Commodity = content.DocumentRecord.CommodityName;
                dtoRecord.HSCode = content.DocumentRecord.CommodityHSCode!;
                dtoRecord.IMO = content.DocumentRecord.IMO;
                dtoRecord.UNNO = content.DocumentRecord.UNNO;

                if (content.SupplementaryUnit is not null && content.SupplementaryUnitQuantity.HasValue)
                {
                    dtoRecord.SupplementaryUnitQuantity = content.SupplementaryUnitQuantity;
                    dtoRecord.SupplementaryUnitShortName = content.SupplementaryUnit.ShortName;
                    dtoRecord.SupplementaryUnitCode = content.SupplementaryUnit.Code?.ToString("000");
                }

                ++contentIndex;

                dtoRecords.Add(dtoRecord);
                dtoRecord = new();
            }
        }

        return dtoRecords;
    }    

    private static readonly Func<uint?, string?> FuncSupplementaryCodeToString = (code) =>
    {
        if (!code.HasValue) return null;

        string unitCode = code.Value.ToString();

        return unitCode.Length switch
        {
            0 => "000",
            1 => unitCode.Insert(0, "00"),
            2 => unitCode.Insert(0, "0"),
            3 => unitCode,
            _ => unitCode[..3]
        };
    };
    #endregion
}
