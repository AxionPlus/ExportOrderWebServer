namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public class ExportOrderProvider : IExportOrderProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse = new();

    public ExportOrderProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            try
            {
                var exportOrder = await db.ExportOrders.AsNoTracking().AsSplitQuery()
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
                                                       .FirstOrDefaultAsync(s => s.Id == id);

                if (exportOrder is null)
                {
                    appObjResponse.ErrorAdd("Export Order not found");
                    return appObjResponse;
                }

                appObjResponse.Object = exportOrder;
            }
            catch (Exception ex)
            {
                appObjResponse.ErrorAdd(ex.Message);
                return appObjResponse;
            }
        }

        return appObjResponse;
    }

    public async Task<IEnumerable<ManifestDTO>?> GetItemsManifestDTOAsync(long id, bool isImo)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var exportOrders = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Terminal).ThenInclude(ter => ter.Customs)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod!.Country)
                                            .Include(x => x.Person)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c!.Location).ThenInclude(lo => lo!.Country)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                            .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                            .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                            .Where(x => x.VesselCallDetail!.VesselCall!.Id == id)
                                            .Where(x => x.Status != EntityStatus.Cancelled)
                                            .ToListAsync();

                var DTOItems = manifestRecords(exportOrders, isImo);

                return DTOItems;
            }
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return Enumerable.Empty<ManifestDTO>();
        }
    }

    public Task<AppObjectResponse> GetItemsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            
            var exportOrders = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                                    .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel)
                                                    .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Terminal)
                                                    .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.POD)
                                                    .Include(eo => eo.Carrier)
                                                    .Include(eo => eo.Records).ThenInclude(r => r.Contents).ThenInclude(c => c.DocumentRecord)
                                                    .Where(s => filter.Dated.HasValue ? s.Dated == filter.Dated : true)
                                                    .Where(s => string.IsNullOrWhiteSpace(filter.CntrNum) ? true :
                                                                    s.Records.Any(eor => eor.CntrNum.Equals(filter.CntrNum)))
                                                    .Where(s => filter.Status != null ? s.Status == filter.Status :
                                                                    (s.Status == EntityStatus.New || s.Status == EntityStatus.Customs))
                                                    .Where(s => string.IsNullOrEmpty(filter.Num) ? true : s.Num == filter.Num)
                                                    .Where(s => string.IsNullOrEmpty(filter.Vessel) ? true : s.VesselCallDetail!.VesselCall.Vessel.Name == filter.Vessel)
                                                    .Where(s => string.IsNullOrEmpty(filter.Voyage) ? true : s.VesselCallDetail!.VesselCall.VoyageNo == filter.Voyage)
                                                    .Where(s => string.IsNullOrEmpty(filter.POD) ? true : s.VesselCallDetail!.POD!.Name == filter.POD)
                                                    .Where(s => string.IsNullOrEmpty(filter.Carrier) ? true :
                                                                    s.Carrier == null ? true : s.Carrier.NameEn == filter.Carrier)
                                                    .ToArrayAsync();
                           
            var ItemsDTO = exportOrders.Select(record=>
                new ExportOrderComponentDTO()
                {
                    Id = record.Id,
                    Num = record.Num,
                    Dated = record.Dated,
                    VesselCallId = record.VesselCallDetail!.VesselCall.Id,
                    Vessel = record.VesselCallDetail!.VesselCall!.Vessel.Name!,
                    VoyageNo = record.VesselCallDetail!.VesselCall.VoyageNo,
                    POD = record.VesselCallDetail is null ? string.Empty :
                            record.VesselCallDetail.POD is null ? string.Empty : record.VesselCallDetail.POD.Name,
                    Carrier = record.Carrier is null ? string.Empty : record.Carrier.NameEn,
                    Terminal = record.VesselCallDetail is null ? string.Empty :
                                record.VesselCallDetail.VesselCall.Terminal.Name,
                    IsImo = record.Records.Any(eor => eor.Contents.Any(c => c.DocumentRecord.IsIMO.Equals(true)).Equals(true)),
                    IsEmpty = record.Records.Any(eor => eor.Contents.Any(c => c.DocumentRecord.CommodityEngName.ToUpper().Contains("EMPTY")).Equals(true)),
                    Status = record.Status,
                    BlTemplate = record.Carrier != null ? record.Carrier.BlTemplate : BLTemplate.standard,
                    CreateTime = record.CreateTime,
                    VersionNo = record.VersionNo,
                }
            );

            if (!string.IsNullOrEmpty(filter.Voyage))
                ItemsDTO = ItemsDTO.OrderByDescending(s => s.Dated);
            else
                ItemsDTO = ItemsDTO.OrderByDescending(s => s.CreateTime);

            appObjResponse.Object = ItemsDTO.ToArray();            

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

    public async Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item, string UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {

            var db = await _db;
            db.Database.SetCommandTimeout(560);
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

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);

                if (User is null)
                {
                    appObjResponse.ErrorAdd($"user not found{UserName}");
                    return appObjResponse;
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

                //var dbBug = db.ChangeTracker.DebugView.LongView;

                /// RE-WRITE WITH A NEW ITEM
                modifyItem.CreateUser = User;
                modifyItem.Num = item.Num;
                modifyItem.Dated = item.Dated;
                modifyItem.Carrier = item.Carrier;
                modifyItem.Person = item.Person;
                modifyItem.VesselCallDetail = item.VesselCallDetail;
                modifyItem.CommodityShort = item.CommodityShort;
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
                //dbBug = db.ChangeTracker.DebugView.LongView;
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

    public async Task<AppObjectResponse> NewItemAsync(ExportOrderEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);
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
                db.Entry(item.VesselCallDetail!).State = EntityState.Modified;

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

    public async Task<AppObjectResponse> RemoveItemAsync(long id)
    {
        appObjResponse = new();

        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.AdminUser);

                // delete Export Order
                var deleteItem = await db.ExportOrders
                                                      .Include(eo => eo.VesselCallDetail)
                                                      .Include(eo => eo.Records).ThenInclude(d => d.Contents)
                                                      .FirstOrDefaultAsync(s => s.Id == id);

                if (deleteItem!.Records.Count() > 0)
                    foreach (var record in deleteItem.Records)
                    {
                        if (record.Contents.Count > 0)
                            foreach (var content in record.Contents)
                                db.Entry(content).State = EntityState.Deleted;

                        db.Entry(record).State = EntityState.Deleted;
                    }

                db.Entry(deleteItem).State = EntityState.Deleted;

                var Bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return appObjResponse;
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetVersionAsync(int version)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.ExportOrders.AsNoTracking()
                                                        .Include(eo => eo.Carrier)
                                                        .Include(eo => eo.Person)
                                                         .Include(eo => eo.VesselCallDetail)
                                                         .Include(eo => eo.Documents).ThenInclude(doc => doc.Records)
                                                         .Include(eo => eo.Records).ThenInclude(eor => eor.CntrType)
                                                         .Include(eo => eo.Records).ThenInclude(eor => eor.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                                         .FirstOrDefaultAsync(s => s.VersionNo == version);
            return appObjResponse;
        }
    }

    public async Task<IEnumerable<PersonEntity>> GetPersonsAsync()
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var Items = await db.Persons.AsNoTracking().ToListAsync();

                return Items!;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            return Enumerable.Empty<PersonEntity>();
        }
    }

    public async Task<AppObjectResponse> GetExportOrderRecordItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var eoRecords = await db.Set<ExportOrderRecord>().AsNoTracking()
                                                             .Include(eor => eor.ExportOrder).ThenInclude(eo => eo.VesselCallDetail)
                                                             .Include(eor => eor.CntrType)
                                                             .Include(eor => eor.Contents).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
                                                             .FirstOrDefaultAsync(s => s.Id == id);

            appObjResponse.Object = eoRecords;
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> SetNewVesselCall(long[] exportOrderIds, long vesselCallDetailId)
    {
        appObjResponse = new();

        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var VesselCallDetail = await db.Set<VesselCallDetail>().FirstOrDefaultAsync(s => s.Id == vesselCallDetailId);

                var ExportOrders = await db.ExportOrders.Where(s => exportOrderIds.Any(id => id == s.Id)).ToArrayAsync();

                if (VesselCallDetail is null)
                {
                    appObjResponse.ErrorAdd("Рейс не найден.");
                    return appObjResponse;
                }
                else if (ExportOrders is null || !ExportOrders.Any())
                {
                    appObjResponse.ErrorAdd("Поручения не найдены.");
                    return appObjResponse;
                }

                foreach (var exportOrder in ExportOrders)
                {
                    exportOrder.VesselCallDetail = VesselCallDetail;

                    //db.Entry(exportOrder.Carrier!).State = EntityState.Unchanged;
                    //db.Entry(exportOrder.Person!).State = EntityState.Unchanged;
                    //db.Entry(exportOrder.VesselCallDetail).State = EntityState.Unchanged;

                    db.Entry(exportOrder).State = EntityState.Modified;
                }

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                //int delayCounter = exportOrderIds.Length switch
                //{
                //    0 => 0,
                //    < 50 => 2000,
                //    < 100 => 5000,
                //    < 200 => 10000,
                //    _ => 15000
                //};

                //await Task.Delay(delayCounter);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            appObjResponse.ErrorAdd($"Voyage Change error: {ex.Message}");
            return appObjResponse;
        }

        return appObjResponse;
    }

    public async Task<IEnumerable<string>> GetDocumentExportOrderNums(string documentNum)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var nums = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                        .Include(s => s.Records).ThenInclude(er => er.Contents).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
                                        .Where(s => s.Records.SelectMany(er => er.Contents)
                                                             .Any(c => !string.IsNullOrWhiteSpace(c.DocumentRecord.Document.Name) &&
                                                                        c.DocumentRecord.Document.Name == documentNum))
                                        .Select(s => new string(string.Concat(s.Num, " (", s.Status.ToString(), ")")))
                                        .ToArrayAsync();

            return nums.Order();
        }            
    }

    public async Task<double[]> GetDocumentExportOrderTotalWeights(string documentNum)
    {
        /// Retrieves total NET & total GROSS weights of ExportOrders, which is used in Document
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;            

            var containerContents = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                              .Include(s => s.Records).ThenInclude(er => er.Contents).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)                                              
                                              .SelectMany(s => s.Records).SelectMany(eor => eor.Contents)
                                              .Where(con => !string.IsNullOrWhiteSpace(con.DocumentRecord.Document.Name) &&
                                                                              con.DocumentRecord.Document.Name == documentNum)
                                              .ToArrayAsync();

            double? net = containerContents.Sum(con => con.NetWt);
            double? gross = containerContents.Sum(con => con.GrossWt);

            double[] total = { net ?? 0, gross ?? 0 };

            return total;
        }
    }

    public async Task<IEnumerable<string>?> GetCntrNumsInVoyage(long eoId, long voyageId)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var cntrNums = await db.ExportOrders.Include(eo => eo.Records)
                                                .Where(eo => eo.VesselCallDetail != null)
                                                .Include(eo => eo.VesselCallDetail!.VesselCall)
                                                .AsNoTracking().AsSplitQuery()
                                                .Where(eo => eo.Id != eoId && eo.VesselCallDetail!.VesselCall.Id == voyageId)
                                                .SelectMany(eo => eo.Records).Select(eor => eor.CntrNum)
                                                .ToListAsync();    //ToArrayAsync

            if (cntrNums is not null && cntrNums.Any())
                return cntrNums;

            return null;
        }
    }

    public async Task<List<ExportOrderDTO>?> GetItemsOrderDTOAsync(IEnumerable<long> ids)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            //var myCompany = await db.MyCompany.OrderBy(c => c.Id).LastOrDefaultAsync();

            try
            {
                var dbItems = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                    .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
                                    .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Vessel).ThenInclude(vsl => vsl.Flag)
                                    .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
                                    .Include(x => x.Carrier).ThenInclude(c => c!.CarrierDetails)
                                    .Include(x => x.Person)
                                    .Include(x => x.Documents).ThenInclude(d => d.Records)
                                    .Include(x => x.Records).ThenInclude(r => r.CntrType)
                                    .Include(x => x.Records).ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                    .Include(x => x.Records).ThenInclude(r => r.Contents).ThenInclude(co => co.SupplementaryUnit)
                                    .Where(x => ids.Any(i => i == x.Id))
                                    .ToListAsync();    //ToArrayAsync

                if (dbItems is null || !dbItems.Any())
                    return null;

                var dtoItems = dbItems.Select(s => new ExportOrderDTO()
                {
                    Id = s.Id,
                    Num = s.Num,
                    BLNum = s.Num.Contains('_') ? s.Num[..s.Num.IndexOf("_")] : s.Num,
                    Dated = s.Dated.HasValue ? s.Dated.Value.ToString("dd.MM.yyyy") : null,
                    XmlDated = s.Dated.HasValue ? s.Dated.Value.ToString("dd.MM.yyyy hh:mm:ss") : null,
                    DateOfLoading = s.VesselCallDetail == null ? null :
                                        !s.VesselCallDetail.VesselCall.ETA.HasValue ? null :
                                            s.VesselCallDetail.VesselCall.ETA.Value.ToString("dd.MM.yyyy"),
                    BLDate = s.VesselCallDetail == null ? null :
                                !s.VesselCallDetail.VesselCall.ETS.HasValue ? null :
                                    s.VesselCallDetail.VesselCall.ETS.Value.ToString("dd.MM.yyyy"),
                    BLtemplate = s.Carrier != null ? s.Carrier.BlTemplate.ToString() : string.Empty,
                    Shippers = string.Join(";\n", s.Documents.GroupBy(d => d.Shipper!.Name).Select(g => g.Key).Distinct().ToArray()),  //ToList()
                    Consignees = string.Join(";\n", s.Documents.GroupBy(d => d.Consignee!.Name).Select(g => g.Key).Distinct().ToArray()),  //ToList()
                    Commodities = string.Join(";\n", s.Records.SelectMany(
                                            eor => eor.Contents.Select(rc => string.Concat(rc.DocumentRecord.CommodityName,
                                                rc.DocumentRecord.IsIMO ? string.Concat(" IMO:", rc.DocumentRecord.IMO, " UNNO:", rc.DocumentRecord.UNNO) : "")))
                                            .Distinct().ToArray()), //ToList()
                    CommodityShort = s.CommodityShort,
                    CustomsOfficeCode = s.VesselCallDetail?.VesselCall.Terminal.Customs?.Code,
                    CustomsOfficeNameShort = s.VesselCallDetail?.VesselCall.Terminal.Customs?.OfficeShort,
                    CarrierNameEn = s.Carrier?.NameEn,
                    TerminalName = s.VesselCallDetail == null ? string.Empty : s.VesselCallDetail.VesselCall.Terminal.Name,
                    VesselName = s.VesselCallDetail == null ? string.Empty :
                                        string.IsNullOrEmpty(s.VesselCallDetail.VesselCall.Vessel.Name) ? string.Empty :
                                            s.VesselCallDetail.VesselCall.Vessel.Name,
                    VesselFlag = s.VesselCallDetail == null ? string.Empty :
                                        s.VesselCallDetail.VesselCall.Vessel.Flag == null ? string.Empty :
                                            s.VesselCallDetail.VesselCall.Vessel.Flag.RUS,
                    VesselFlagEn = s.VesselCallDetail == null ? string.Empty :
                                        s.VesselCallDetail.VesselCall.Vessel.Flag == null ? string.Empty :
                                            s.VesselCallDetail.VesselCall.Vessel.Flag.ENG,
                    Voyage = s.VesselCallDetail == null ? string.Empty : s.VesselCallDetail.VesselCall.VoyageNo,
                    POD = s.VesselCallDetail == null ? string.Empty :
                                s.VesselCallDetail.POD == null ? string.Empty :
                                    s.VesselCallDetail.POD.Country == null ? string.Empty :
                                        string.Concat(s.VesselCallDetail.POD.Name, ", ", s.VesselCallDetail.POD.Country.RUS),
                    PODEn = s.VesselCallDetail == null ? string.Empty :
                                    s.VesselCallDetail.POD == null ? string.Empty :
                                        s.VesselCallDetail.POD.Country == null ? string.Empty :
                                            string.Concat(s.VesselCallDetail.POD.NameEn, ", ", s.VesselCallDetail.POD.Country.ENG),
                    PODwithCountryRus = s.VesselCallDetail == null ? string.Empty :
                                                s.VesselCallDetail.POD == null ? string.Empty :
                                                    s.VesselCallDetail.POD.Country == null ? string.Empty :
                                                        string.Concat(s.VesselCallDetail.POD.NameEn, ", ", s.VesselCallDetail.POD.Country.RUS),
                    FinalDestination = s.VesselCallDetail == null ? string.Empty :
                                                s.VesselCallDetail.FinalDestination == null ? string.Empty :
                                                    s.VesselCallDetail.FinalDestination.Country == null ? string.Empty :
                                                        string.Concat(s.VesselCallDetail.FinalDestination.NameEn, ", ", s.VesselCallDetail.FinalDestination.Country.ENG),
                    POLAgent = s.Carrier == null ? string.Empty :
                                    s.VesselCallDetail == null ? string.Empty :
                                        s.VesselCallDetail.VesselCall.Terminal.Name == null ? string.Empty :
                                            s.Carrier.CarrierDetails.FirstOrDefault(cd => cd.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.AgentPOL,
                    PODAgent = s.VesselCallDetail == null ? string.Empty : s.VesselCallDetail.AgentPOD,
                    TotalCntrCount = (uint)s.Records.Count,
                    TotalPackages = (uint?)s.Records.Sum(r => r.Contents.Sum(c => c.PackageQty ?? 0)),
                    TotalGrossWeight = s.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                    TotalNetWeight = s.Records.Sum(r => r.Contents.Sum(c => c.NetWt)),
                    TotalTareWeight = s.Records.Sum(r => r.CntrTareWt),
                    Contract = s.Carrier == null ? null :
                                    s.VesselCallDetail == null ? null :
                                        string.IsNullOrEmpty(s.VesselCallDetail.VesselCall.Terminal.Name) ? null :
                                            s.Carrier.CarrierDetails.FirstOrDefault(c => c.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.Contract,
                    ContractDate = s.Carrier == null ? null :
                                        s.VesselCallDetail == null ? null :
                                            string.IsNullOrEmpty(s.VesselCallDetail.VesselCall.Terminal.Name) ? null :
                                                s.Carrier.CarrierDetails.FirstOrDefault(c => c.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract.HasValue ?
                                                    s.Carrier.CarrierDetails.FirstOrDefault(c => c.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") : "",
                    //MyCompanyName = myCompany?.Name,
                    //MyCompanyEmail = myCompany?.Email,
                    MyCompanyName = s.Person?.CompanyName,
                    MyCompanyEmail = s.Person?.CompanyEmail,
                    Person = string.Concat(s.Person?.Name?[..1], ". ", s.Person?.SurName?[..1], ". ", s.Person?.FamilyName, "  т. ", s.Person?.Phone),
                    PersonXml = string.Concat(s.Person?.Name, " ", s.Person?.FamilyName, " телефон: ", s.Person?.Phone),
                    PersonPass = s.Person?.Passport,
                    PersonPhone = s.Person?.Phone,
                    ExportOrderRecordsDTO = orderRecords(s.Records)
                }).ToList();

                return dtoItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }        
    }

    public async Task<List<ExportOrderDTO>?> GetItemsBillDTOAsync(IEnumerable<long> ids)    //IEnumerable
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var dbItems = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                    .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
                                    .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Vessel).ThenInclude(vsl => vsl.Flag)
                                    .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
                                    .Include(x => x.Carrier).ThenInclude(c => c!.CarrierDetails)
                                    .Include(x => x.Person)
                                    .Include(x => x.Documents).ThenInclude(d => d.Records)
                                    .Include(x => x.Records).ThenInclude(r => r.CntrType)
                                    .Include(x => x.Records).ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                    .Include(x => x.Records).ThenInclude(r => r.Contents).ThenInclude(co => co.SupplementaryUnit)
                                    .Where(x => ids.Any(i => i == x.Id))
                                    .ToListAsync();    //ToArrayAsync

                if (dbItems is null || !dbItems.Any())
                    return null;

                var ItemsDTO = dbItems.Select(s => new ExportOrderDTO()
                {
                    Id = s.Id,
                    Num = s.Num,
                    BLNum = s.Num.Contains('_') ? s.Num[..s.Num.IndexOf('_')] : s.Num,  //IndexOf("_")
                    BLDate = s.VesselCallDetail == null ? null :
                                !s.VesselCallDetail.VesselCall.ETS.HasValue ? null :
                                    s.VesselCallDetail.VesselCall.ETS.Value.ToString("dd.MM.yyyy"),
                    BLDateOEL = s.VesselCallDetail == null ? null :
                                    !s.VesselCallDetail.VesselCall.ETS.HasValue ? null :
                                        DateToStr(s.VesselCallDetail.VesselCall.ETS.Value),
                    BLtemplate = s.Carrier == null ? BLTemplate.standard.ToString() : s.Carrier.BlTemplate.ToString(),
                    CarrierNameEn = s.Carrier?.NameEn,
                    TerminalName = s.VesselCallDetail?.VesselCall.Terminal.Name,
                    VesselName = s.VesselCallDetail == null ? string.Empty :
                                    string.IsNullOrWhiteSpace(s.VesselCallDetail.VesselCall.Vessel.Name) ? string.Empty :
                                        s.VesselCallDetail.VesselCall.Vessel.Name,
                    VesselFlagEn = s.VesselCallDetail == null ? string.Empty :
                                        s.VesselCallDetail.VesselCall.Vessel.Flag == null ? string.Empty :
                                            s.VesselCallDetail.VesselCall.Vessel.Flag.ENG,
                    Voyage = s.VesselCallDetail == null ? string.Empty : s.VesselCallDetail.VesselCall.VoyageNo,
                    Shippers = string.Join("; ", s.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.NameEn)).Distinct().ToArray()),
                    Consignees = string.Join("; ", s.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.NameEn)).Distinct().ToArray()),
                    Commodities = string.Join("; ",
                        s.Records.SelectMany(eor => eor.Contents.Select(rc => string.Concat(rc.DocumentRecord.CommodityEngName,
                                                                                            !rc.DocumentRecord.IsIMO ? "" :
                                                                                                string.Concat(" IMO:", rc.DocumentRecord.IMO,
                                                                                                              " UNNO:", rc.DocumentRecord.UNNO)).Trim()))
                        .Distinct().ToArray()),
                    PODEn = s.VesselCallDetail == null ? string.Empty :
                                s.VesselCallDetail.POD == null ? string.Empty :
                                    string.IsNullOrWhiteSpace(s.VesselCallDetail.POD.NameEn) ? string.Empty :
                                        s.VesselCallDetail.POD.NameEn,
                    PODwithCountryEn = s.VesselCallDetail == null ? null :
                                            s.VesselCallDetail.POD == null ? null :
                                                (string.IsNullOrWhiteSpace(s.VesselCallDetail.POD.NameEn) || s.VesselCallDetail.POD.Country == null) ? null :
                                                    string.Concat(s.VesselCallDetail.POD.NameEn, ", ", s.VesselCallDetail.POD.Country.ENG),
                    PODwithCountryRus = s.VesselCallDetail == null ? null :
                                            s.VesselCallDetail.POD == null ? null :
                                                (string.IsNullOrWhiteSpace(s.VesselCallDetail.POD.NameEn) || s.VesselCallDetail.POD.Country == null) ? null :
                                                    string.Concat(s.VesselCallDetail.POD.NameEn, ", ", s.VesselCallDetail.POD.Country.RUS),
                    FinalDestination = s.VesselCallDetail == null ? null :
                                            s.VesselCallDetail.FinalDestination == null ? null :
                                            (string.IsNullOrWhiteSpace(s.VesselCallDetail.FinalDestination.NameEn) || s.VesselCallDetail.FinalDestination.Country == null) ? null :
                                                string.Concat(s.VesselCallDetail.FinalDestination.NameEn, ", ", s.VesselCallDetail.FinalDestination.Country.ENG),
                    POLAgent = (s.Carrier == null || s.VesselCallDetail == null || string.IsNullOrWhiteSpace(s.VesselCallDetail.VesselCall.Terminal.Name)) ? null :
                                    s.Carrier.CarrierDetails.FirstOrDefault(cd => cd.TerminalName == s.VesselCallDetail.VesselCall.Terminal.Name)!.AgentPOL,
                    PODAgent = s.VesselCallDetail?.AgentPOD,
                    Measurement = s.Records.FirstOrDefault()!.Contents.FirstOrDefault()!.Volume > 0 ? "CBM" : "KG",

                    TotalCntrCount = (uint)s.Records.Count,
                    TotalPackages = (uint?)s.Records.Sum(r => r.Contents.Sum(c => c.PackageQty)),
                    TotalGrossWeight = s.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                    TotalNetWeight = s.Records.Sum(r => r.Contents.Sum(c => c.NetWt)),
                    TotalTareWeight = s.Records.Sum(r => r.CntrTareWt),
                    TotalGrossNTareWeight = s.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)) + s.Records.Sum(r => r.CntrTareWt),
                    ExportOrderRecordsDTO = billRecords(s.Records)
                }).ToList();

                return ItemsDTO;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }

    public Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item)
    {
        throw new NotImplementedException();
    }

    #region AUXILIARY

    private readonly Func<List<ExportOrderRecord>, List<ExportOrderRecordDTO>> orderRecords = (_eoRecords) =>
    {
        List<ExportOrderRecordDTO> eoRecordsDTO = new();
        ExportOrderRecordDTO eoRecordDTO = new();

        uint indexRec = 0;

        foreach (var record in _eoRecords)
        {
            eoRecordDTO.Seq = ++indexRec;
            eoRecordDTO.CntrType = record.CntrType is null ? string.Empty : record.CntrType.Normolize!;
            eoRecordDTO.CntrTypeISO = record.CntrType is null ? string.Empty : record.CntrType.ISO;
            eoRecordDTO.CntrTareWt = record.CntrTareWt;
            eoRecordDTO.Seal = record.Seal;

            foreach (var content in record.Contents)
            {
                eoRecordDTO.SeqContent = (uint)content.DocumentRecord.Seq;
                eoRecordDTO.Cntr = record.CntrNum;

                eoRecordDTO.PackageQty = content.PackageQty is null ? 0 : (uint)content.PackageQty;
                eoRecordDTO.PackageName = content.PackageName is null ? string.Empty : content.PackageName.ToUpper();
                eoRecordDTO.NetWt = content.NetWt is null ? 0 : content.NetWt;
                eoRecordDTO.GrossWt = content.GrossWt is null ? 0 : content.GrossWt;
                eoRecordDTO.Volume = content.Volume is null ? 0 : content.Volume;
                eoRecordDTO.GrossAndTare = record.CntrTareWt + (content.GrossWt.HasValue ? content.GrossWt : 0);
                
                if (content.SupplementaryUnit is not null && content.SupplementaryUnitQuantity.HasValue)
                {
                    eoRecordDTO.SupplementaryUnitQuantity = content.SupplementaryUnitQuantity;
                    eoRecordDTO.SupplementaryUnitShortName = content.SupplementaryUnit.ShortName;
                    eoRecordDTO.SupplementaryUnitCode = SupplementaryCodeToString(content.SupplementaryUnit.Code);
                }

                eoRecordDTO.DocumentName = string.IsNullOrEmpty(content.DocumentRecord.Document.Name) ? string.Empty : content.DocumentRecord.Document.Name;
                eoRecordDTO.DocumentType = content.DocumentRecord.Document.Type.ToString();
                eoRecordDTO.Shipper = content.DocumentRecord.Document.Shipper is null ? string.Empty : 
                                        string.IsNullOrEmpty(content.DocumentRecord.Document.Shipper.Name) ? string.Empty : content.DocumentRecord.Document.Shipper.Name;
                eoRecordDTO.ShipperEn = content.DocumentRecord.Document.Shipper is null ? string.Empty :
                                            string.IsNullOrEmpty(content.DocumentRecord.Document.Shipper.NameEn) ? string.Empty : content.DocumentRecord.Document.Shipper.NameEn;
                eoRecordDTO.Consignee = content.DocumentRecord.Document.Consignee is null ? string.Empty :
                                            string.IsNullOrEmpty(content.DocumentRecord.Document.Consignee.Name) ? string.Empty : content.DocumentRecord.Document.Consignee.Name;
                eoRecordDTO.ConsigneeEn = content.DocumentRecord.Document.Consignee is null ? string.Empty :
                                            string.IsNullOrEmpty(content.DocumentRecord.Document.Consignee.NameEn) ? string.Empty : content.DocumentRecord.Document.Consignee.NameEn;

                eoRecordDTO.Commodity = content.DocumentRecord.CommodityName;
                eoRecordDTO.CommodityEn = content.DocumentRecord.CommodityEngName;
                eoRecordDTO.HSCode = content.DocumentRecord.CommodityHSCode!;
                eoRecordDTO.IMO = content.DocumentRecord.IMO;
                eoRecordDTO.UNNO = content.DocumentRecord.UNNO;
                eoRecordDTO.IsIMO = content.DocumentRecord.IsIMO;

                eoRecordsDTO.Add(eoRecordDTO);
                eoRecordDTO = new ExportOrderRecordDTO();
            }
        }

        return eoRecordsDTO;
    };

    private readonly Func<List<ExportOrderRecord>, List<ExportOrderRecordDTO>> billRecords = (_eoRecords) =>
    {
        List<ExportOrderRecordDTO> eoRecordsDTO = new();

        uint indexRec = 0;

        foreach (var record in _eoRecords)
        {
            var eoRecordDTO = new ExportOrderRecordDTO()
            {
                Seq = ++indexRec,
                Cntr = record.CntrNum,
                CntrType = record.CntrType!.Normolize!,
                CntrTareWt = record.CntrTareWt,
                Seal = record.Seal,

                PackageQty = (uint)record.Contents.Sum(c => c.PackageQty)! > 0 ? (uint)record.Contents.Sum(c => c.PackageQty)! : null,
                PackageName = string.Join(", ", record.Contents.Select(rc => rc.PackageName is not null ? rc.PackageName.ToUpper() : "").Distinct().Order()),
                NetWt = record.Contents.Sum(c => c.NetWt),
                GrossWt = record.Contents.Sum(c => c.GrossWt),
                Volume = record.Contents.Sum(c => c.Volume),
                GrossAndTare = (record.Contents.Sum(c => c.GrossWt) + record.CntrTareWt),
                RecordCommoditiesEn = string.Join("; ", record.Contents.Select(rc =>
                    string.Concat(rc.DocumentRecord.CommodityEngName,
                                    rc.DocumentRecord.IsIMO ?
                                        string.Concat(" IMO: ", rc.DocumentRecord.IMO, " UNNO: ", rc.DocumentRecord.UNNO) : "")
                    )
                    .Distinct().Order().ToArray()),
            };

            eoRecordsDTO.Add(eoRecordDTO);
        }

        return eoRecordsDTO;
    };

    private readonly Func<List<ExportOrderEntity>, bool, IEnumerable<ManifestDTO>?> manifestRecords = (exportOrders, _isImo) =>
    {
        var ItemsDTO = new List<ManifestDTO>();

        try
        {
            foreach (var Item in exportOrders)
            {
                var _shippers = Item.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.NameEn!)).ToList();
                var _consignees = Item.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.NameEn!)).ToList();

                var _commodities = new List<string>();
                var _commoditiesEn = new List<string>();

                _commodities = Item.Records.SelectMany(eor => 
                                    eor.Contents.Select(con => 
                                        string.Concat(con.DocumentRecord.CommodityName,
                                                        con.DocumentRecord.IsIMO ?
                                                            string.Concat(" IMO:", con.DocumentRecord.IMO,
                                                                            " UNNO:", con.DocumentRecord.UNNO) : ""))
                                    ).Distinct().ToList();

                _commoditiesEn = Item.Records.SelectMany(eor => 
                                        eor.Contents.Select(con => 
                                            string.Concat(con.DocumentRecord.CommodityEngName,
                                                            con.DocumentRecord.IsIMO ?
                                                                string.Concat(" IMO:", con.DocumentRecord.IMO,
                                                                                " UNNO:", con.DocumentRecord.UNNO) : ""))
                                        ).Distinct().ToList();

                if (_isImo)
                {
                    _commodities = Item.Records.SelectMany(eor => eor.Contents.Where(con => con.DocumentRecord.IsIMO == true)
                                                                              .Select(con => string.Concat(con.DocumentRecord.CommodityName,
                                                                                                            " IMO:", con.DocumentRecord.IMO,
                                                                                                            " UNNO:", con.DocumentRecord.UNNO))
                                                                              ).Distinct().ToList();

                    _commoditiesEn = Item.Records.SelectMany(eor => eor.Contents.Where(con => con.DocumentRecord.IsIMO == true)
                                                                                .Select(con => string.Concat(con.DocumentRecord.CommodityEngName,
                                                                                                                " IMO:", con.DocumentRecord.IMO,
                                                                                                                " UNNO:", con.DocumentRecord.UNNO))
                                                                                ).Distinct().ToList();
                }

                uint indexRec = 0;

                var Records = Item.Records;

                if (_isImo)
                    Records = Item.Records.Where(r => r.Contents.Any(c => c.DocumentRecord.IsIMO.Equals(true))).ToList();

                foreach (var record in Records)
                {
                    var CntrContents = record.Contents;

                    if (_isImo)
                        CntrContents = record.Contents.Where(con => con.DocumentRecord.IsIMO.Equals(true)).ToList();

                    var ItemDTO = new ManifestDTO()
                    {
                        VesselCallId = Item.VesselCallDetail!.VesselCall!.Id,
                        ExpOrderNum = Item.Num,
                        BLNum = Item.Num.Contains('_') ? Item.Num[..Item.Num.IndexOf("_")] : Item.Num,
                        BLDate = Item.VesselCallDetail!.VesselCall!.ETS.HasValue ? Item.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy") : "---",
                        BLtemplate = Item.Carrier != null ? Item.Carrier.BlTemplate.ToString() : null,
                        Voyage = Item.VesselCallDetail!.VesselCall!.VoyageNo,
                        VesselName = Item.VesselCallDetail!.VesselCall!.Vessel.Name is null ? string.Empty : Item.VesselCallDetail!.VesselCall!.Vessel.Name,
                        VesselFlag = Item.VesselCallDetail!.VesselCall!.Vessel.Flag is null ? string.Empty : Item.VesselCallDetail!.VesselCall!.Vessel.Flag.RUS,
                        VesselFlagEn = Item.VesselCallDetail!.VesselCall!.Vessel.Flag is null ? string.Empty : Item.VesselCallDetail!.VesselCall!.Vessel.Flag.ENG,
                        CaptainFamily = Item.VesselCallDetail.VesselCall.Vessel.CaptainFamily,
                        CaptainName = Item.VesselCallDetail.VesselCall.Vessel.CaptainName,
                        PODEn = Item.VesselCallDetail!.POD is null ? string.Empty : Item.VesselCallDetail!.POD.NameEn,
                        PODandCountryEn = Item.VesselCallDetail!.POD is null ? string.Empty :
                                            Item.VesselCallDetail!.POD!.NameEn + ", " + Item.VesselCallDetail!.POD.Country!.ENG,
                        PODunlocode = Item.VesselCallDetail!.POD is null ? string.Empty : Item.VesselCallDetail!.POD.UnLocode,
                        CarrierNameEn = Item.Carrier is null ? string.Empty : Item.Carrier.NameEn,
                        CarrierLocation = Item.Carrier is null ? string.Empty : Item.Carrier.Location is null ? string.Empty : Item.Carrier.Location.Name,
                        CarrierCountryEn = Item.Carrier is null ? string.Empty :
                                                Item.Carrier.Location is null ? string.Empty :
                                                    Item.Carrier.Location.Country is null ? string.Empty : Item.Carrier.Location.Country.ENG,
                        CarrierContract = Item.Carrier is null ? string.Empty :
                                            Item.Carrier.CarrierDetails.Any(cd => cd.TerminalName.Equals(Item.VesselCallDetail.VesselCall.Terminal.Name)) == true ?
                                                Item.Carrier.CarrierDetails
                                                    .FirstOrDefault(s => s.TerminalName == Item.VesselCallDetail.VesselCall.Terminal.Name)!.Contract : "",
                        CarrierContractDate = Item.Carrier is null ? string.Empty :
                                                Item.Carrier.CarrierDetails.Any(cd => cd.TerminalName.Equals(Item.VesselCallDetail.VesselCall.Terminal.Name)) == true ?
                                                    Item.Carrier.CarrierDetails
                                                        .FirstOrDefault(s => s.TerminalName == Item.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") : "",
                        CustomsOfficeCode = Item.VesselCallDetail!.VesselCall.Terminal.Customs is null ? string.Empty :
                                                Item.VesselCallDetail!.VesselCall.Terminal.Customs.Code,
                        CustomsOfficeName = Item.VesselCallDetail!.VesselCall.Terminal.Customs is null ? string.Empty :
                                                Item.VesselCallDetail!.VesselCall.Terminal.Customs.Office,
                        CustomsOfficeShortName = Item.VesselCallDetail!.VesselCall.Terminal.Customs is null ? string.Empty :
                                                    Item.VesselCallDetail!.VesselCall.Terminal.Customs.OfficeShort,
                        CustomsDapartment = Item.VesselCallDetail!.VesselCall.Terminal.Customs is null ? string.Empty :
                                                Item.VesselCallDetail!.VesselCall.Terminal.Customs.Dapartment,

                        PersonFamily = Item.Person is null ? string.Empty : Item.Person.FamilyName,
                        PersonSurName = Item.Person is null ? string.Empty : Item.Person.Name + " " + Item.Person.SurName,
                        PersonBirthYear = Item.Person is null ? string.Empty : Item.Person.BirthYear,
                        PersonBirthPlace = Item.Person is null ? string.Empty : Item.Person.BirthPlace,
                        PersonCompany = Item.Person is null ? string.Empty : Item.Person.CompanyName,
                        PersonAddress = Item.Person is null ? string.Empty : Item.Person.Address,
                        PersonPass = Item.Person is null ? string.Empty : Item.Person.Passport,                        
                        PersonSign = string.Concat(Item.Person?.Name?[..1], ". ",                                                          
                                                   Item.Person?.SurName?[..1], ". ",
                                                   Item.Person?.FamilyName),
                        DateExplanation = string.Concat("\"____\" ", 
                                                        Item.VesselCallDetail!.VesselCall!.ETS.HasValue ?
                                                            Item.VesselCallDetail!.VesselCall!.ETS.Value.ToString("MMMM yyyy") : "_____________________",
                                                                "г."),
                        Shippers = string.Concat("S: ", string.Join("; ", _shippers.Order().Distinct())),
                        Consignees = string.Concat("C: ", string.Join("; ", _consignees.Order().Distinct())),
                        Commodities = string.Join("; ", _commodities.Order().Distinct()),
                        CommoditiesEn = string.Join("; ", _commoditiesEn.Order().Distinct()),

                        /// Cntr Records
                        Seq = ++indexRec,

                        Cntr = record.CntrNum,
                        CntrType = record.CntrType!.Normolize!,
                        Seal = record.Seal ?? "N/A",
                        CntrTareWt = record.CntrTareWt,

                        PackageQtys = (uint)CntrContents.Sum(c => c.PackageQty)!,
                        PackageNames = string.Join(", ", CntrContents.Select(c => c.PackageName is null ? "" : c.PackageName.ToUpper()).Distinct().Order()),
                        NetWeights = CntrContents.Sum(c => c.NetWt),
                        GrossWeights = CntrContents.Sum(c => c.GrossWt),
                        Volumes = CntrContents.Sum(c => c.Volume),
                        CntrTotalWeight = record.CntrTareWt + (double)CntrContents.Sum(c => c.GrossWt)!,

                        /// Customs Explanation & FillBill
                        IsIMO = record.Contents.Select(c => c.DocumentRecord.IsIMO).Any(imo => imo.Equals(true)),
                        IMO = string.Join(", ", record.Contents.Where(c => c.DocumentRecord.IsIMO).Select(c => c.DocumentRecord.IMO).Distinct()),
                        UNNO = string.Join(", ", record.Contents.Where(c => c.DocumentRecord.IsIMO).Select(c => c.DocumentRecord.UNNO).Distinct()),

                        RecordShippers = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.NameEn!).Distinct().ToList()),
                        RecordConsignees = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.NameEn!).Distinct().ToList()),
                        RecordShippersCountries = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.CountryENG!).Distinct().ToList()),
                        RecordConsigneesCountries = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.CountryENG!).Distinct().ToList()),

                        RecordCommodities = string.Join("; ",
                            record.Contents.Select(rc => string.Concat(rc.DocumentRecord.CommodityName,
                                                                        rc.DocumentRecord.IsIMO ?
                                                                            string.Concat(" IMO:", rc.DocumentRecord.IMO,
                                                                                            "UNNO:", rc.DocumentRecord.UNNO) : "")
                            ).Distinct().ToList()),
                    };

                    if (ItemDTO != null)
                    {
                        if (ItemDTO.BLtemplate != null && ItemDTO.BLtemplate.Equals("ametist"))
                            ItemDTO.BLNum = string.Concat("SV-", ItemDTO.BLNum);

                        ItemsDTO.Add(ItemDTO);                        
                    }
                }
            }

            return ItemsDTO;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    };

    private readonly Func<DateTime?, string?> DateToStr = (date) =>
    {
        if (date is not null)
            return string.Concat(date.Value.ToString("dd"), '/', date.Value.ToString("MM"), '/', date.Value.ToString("yyyy"));
        else
            return null;
    };

    private static readonly Func<uint?, string?> SupplementaryCodeToString = (code) =>
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
