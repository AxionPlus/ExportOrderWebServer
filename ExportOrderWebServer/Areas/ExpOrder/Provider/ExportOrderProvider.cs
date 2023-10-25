using ExportOrderEntites.MyCompany;
using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic;

namespace ExportOrderWebServer.Areas.ExpOrder.Provider;

public class ExportOrderProvider : IExportOrderProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

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
                appObjResponse.Object = await db.ExportOrders.Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal)
                                                             .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Vessel)
                                                             .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.POD)
                                                             .Include(eo => eo.Person)
                                                             .Include(eo => eo.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                                             .Include(eo => eo.Documents)!.ThenInclude(d => d.Records)
                                                             .Include(eo => eo.Records)!.ThenInclude(r => r.CntrType)
                                                             .Include(eo => eo.Records)!.ThenInclude(r => r.Contents).ThenInclude(c => c.DocumentRecord).ThenInclude(dr => dr.Document)
                                                             .AsNoTracking()
                                                             .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(msg);
                return appObjResponse;
            }
        }

        return appObjResponse;
    }
        
    public async Task<ExportOrderDTO> GetItemDTOAsync(long Id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var myCompany = await db.MyCompany.AsNoTracking().FirstOrDefaultAsync();

            //var vesselName = Voyage.Split('&')[0];
            //var voyageNo = Voyage.Split('&')[1];
            //var pod = Voyage.Split('&')[2];

            var Item = await db.ExportOrders.Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                            .Include(x => x.Person)
                                            .Include(x => x.Documents)!.ThenInclude(d => d.Records)
                                            .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                            .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                            .AsNoTracking()
                                            .Select(source => new ExportOrderDTO()
                                            {
                                                Id = source.Id,
                                                Num = source.Num,
                                                Dated = source.Dated.ToString("dd.MM.yyyy"),
                                                BLtemplate = source.Carrier!.BlTemplate.ToString(),
                                                CarrierNameEn = source.Carrier!.NameEn,
                                                VesselName = source.VesselCallDetail!.VesselCall!.Vessel.Name!,
                                                VesselFlag = source.VesselCallDetail!.VesselCall!.Vessel.Flag!.RUS,
                                                VesselFlagEn = source.VesselCallDetail!.VesselCall!.Vessel.Flag.ENG,
                                                Voyage = source.VesselCallDetail!.VesselCall!.VoyageNo,
                                                DateOfLoading = source.VesselCallDetail!.VesselCall!.ETA!.Value.ToString("dd.MM.yyyy"),
                                                BLDate = source.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy"),
                                                POD = source.VesselCallDetail!.POD!.Name + ", " + source.VesselCallDetail!.POD!.Country!.RUS,
                                                PODEn = source.VesselCallDetail!.POD!.NameEn! + ", " + source.VesselCallDetail!.POD!.Country.ENG,
                                                PODAgent = source.VesselCallDetail!.AgentPOD,
                                                Measurement = source.Records.FirstOrDefault()!.Contents.FirstOrDefault()!.Volume > 0 ? "CBM" : "KG",
                                                TotalCntrCount = source.Records.Count,
                                                TotalGrossWeight = source.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                                                TotalTareWeight = source.Records.Sum(r => r.CntrTareWt),
                                                //Contract = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name).Contract != null ?
                                                //          source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name).Contract : null,
                                                Contract = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.Contract,
                                                //ContractDate = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name)!.DateContract != null ?
                                                //          source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") : "",
                                                ContractDate = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy"),
                                                MyCompanyName = myCompany!.Name!,
                                                Person = source.Person!.Name + " т. " + source.Person.Phone,
                                                exportOrderRecordsDTO = eoRecords(source.Records!)
                                            })
                                            .FirstOrDefaultAsync(x => x.Id == Id);

            return Item!;
        }
    }

    public async Task<IEnumerable<ManifestDTO>> GetManifestAsync(string voyage)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var vesselName = voyage.Split('*')[0];
                var voyageNo = voyage.Split('*')[1];

                var Items = await db.ExportOrders
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Terminal)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod!.Country)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                            .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                            .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                            .AsNoTracking()
                                            .Where(x => x.VesselCallDetail!.VesselCall!.Vessel.Name == vesselName && x.VesselCallDetail.VesselCall.VoyageNo == voyageNo)
                                            .AsSplitQuery()
                                            .ToListAsync();

                var ManifestItems = mRecords(Items);

                return ManifestItems;
            }
        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return Enumerable.Empty<ManifestDTO>();
        }
    }

    //DELETE
    public async Task<IEnumerable<BLDTO>> GetManifestItemsAsync(long vslCallId, long carrierId)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var Items = await db.ExportOrders
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Terminal)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod!.Country)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                            .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                            .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                            .AsNoTracking()
                                            .Where(x => x.VesselCallDetail!.VesselCall!.Id == vslCallId)
                                            .Where(x => x.Carrier!.Id == carrierId)
                                            .AsSplitQuery()
                                            .ToListAsync();

                var ManifestItems = blRecords(Items);

                return ManifestItems;
            }

        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return Enumerable.Empty<BLDTO>();
        }
    }

    //DELETE
    public async Task<IEnumerable<ManifestDTO>> GetManifestItemsAsync(List<long> vesselCallDetailIds)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var Items = new List<ExportOrderEntity>();

                foreach (long vcdId in vesselCallDetailIds)
                {
                    var items = await db.ExportOrders
                                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Terminal)
                                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                                .Include(x => x.VesselCallDetail).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod!.Country)
                                                .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                                .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                                .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                                .AsNoTracking()
                                                .Where(x => x.VesselCallDetail!.Id! == vcdId)
                                                .AsSplitQuery()
                                                .ToListAsync();
                    Items.AddRange(items);
                }

                var ManifestItems = mRecords(Items);

                return ManifestItems;
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

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                var exportOrders = await db.ExportOrders
                                                        .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel)
                                                        .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Terminal)
                                                        .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.POD)
                                                        .Include(eo => eo.Carrier)
                                                        .Where(eo => filter.Dated.HasValue ? eo.Dated == filter.Dated: true)
                                                        .ToListAsync();

                var ItemsDTO = eoComponentRecord(exportOrders);


                if (!string.IsNullOrEmpty(filter.Num))
                    ItemsDTO = ItemsDTO.Where(s => s.Num == filter.Num).ToList();

                //if (!string.IsNullOrEmpty(filter.CntrNum))
                //    items = items.Where(s => s.Records!.FirstOrDefault()!.CntrNum == filter.CntrNum).ToList();

                if (!string.IsNullOrEmpty(filter.Vessel))
                    ItemsDTO = ItemsDTO.Where(s => s.Vessel == filter.Vessel).ToList();

                if (!string.IsNullOrEmpty(filter.Voyage))
                    ItemsDTO = ItemsDTO.Where(s => s.Voyage == filter.Voyage).ToList();

                if (!string.IsNullOrEmpty(filter.POD))
                    ItemsDTO = ItemsDTO.Where(s => s.POD== filter.POD).ToList();

                if (!string.IsNullOrEmpty(filter.Carrier))
                    ItemsDTO = ItemsDTO.Where(s => s.Carrier == filter.Carrier).ToList();

                if (filter.Status >= 0)
                    ItemsDTO = ItemsDTO.Where(s => s.Status == filter.Status).ToList();

                appObjResponse.Object = ItemsDTO.ToArray();
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

    public async Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.ExportOrders
                                                    .Include(eo => eo.Carrier)
                                                    .Include(eo => eo.Person)
                                                    .Include(eo => eo.Documents).ThenInclude(d => d.Records)    //.ThenInclude(dr => dr.Document)
                                                    .Include(eo => eo.Records).ThenInclude(d => d.Contents).ThenInclude(c => c.DocumentRecord)
                                                    .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.POD)
                                                    .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Vessel)
                                                    .Include(eo => eo.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal)
                                                    .AsTracking()
                                                    .FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem!.Num != item.Num)
                {
                    var itemExistCheck = await db.ExportOrders.Where(s => s.Num!.ToUpper() == item.Num!.ToUpper()).FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($"Export Order No. {item.Num} dtd {item.Dated.ToShortDateString} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                // DELETE AN EXISTED modifyItem

                foreach (var modifyRecord in modifyItem.Records)
                    db.Entry(modifyRecord).State = EntityState.Deleted;

                //foreach (var modifyDocument in modifyItem.Documents)
                //{
                //    db.Entry(modifyDocument).State = EntityState.Unchanged;
                //    modifyItem.Documents.Remove(modifyDocument);
                //}

                modifyItem.Documents.Clear();

                var dbBug = db.ChangeTracker.DebugView.LongView;
                await db.SaveChangesAsync();

                db.ChangeTracker.Clear();

                // ADD A NEW ITEM

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.Num = item.Num;
                modifyItem!.Dated = item.Dated;
                modifyItem.Carrier = item.Carrier;
                modifyItem!.Person = item.Person;
                modifyItem.VesselCallDetail = item.VesselCallDetail;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;
                db.Entry(modifyItem.Carrier!).State = EntityState.Unchanged;
                db.Entry(modifyItem.Person!).State = EntityState.Unchanged;
                db.Entry(modifyItem.VesselCallDetail!).State = EntityState.Unchanged;

                // DOCUMENTS              
                foreach (var itemDocument in item.Documents!)
                {
                    itemDocument!.CreateUser = User!;
                    db.Entry(itemDocument.CreateUser).State = EntityState.Unchanged;

                    //db.Entry(itemDocument).State = EntityState.Added;
                    db.Entry(itemDocument).State = EntityState.Unchanged;
                    modifyItem.Documents.Add(itemDocument);

                    foreach (var record in itemDocument.Records)
                    {
                        db.Entry(record).State = EntityState.Unchanged;
                    }
                }

                // EXPORT ORDER RECORDS
                foreach (var itemRecord in item.Records)
                {
                    db.Entry(itemRecord).State = EntityState.Added;
                    modifyItem.Records.Add(itemRecord);

                    //db.Entry(itemRecord.CntrType!).State = EntityState.Detached;
                    //db.Entry(itemRecord.CntrType!).State = EntityState.Unchanged;

                    foreach (var itemRecordContent in itemRecord.Contents)
                    {
                        db.Entry(itemRecordContent).State = EntityState.Added;
                    }
                }

                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
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
                item.CreateTime = DateTime.Now;
                item.Status = EntityStatus.Pending;
                item.VesselCallDetail!.Status = EntityStatus.Pending;   // added

                db.Entry(item).State = EntityState.Added;

                db.Entry(item.Carrier!).State = EntityState.Unchanged;
                db.Entry(item.VesselCallDetail!).State = EntityState.Modified; // Unchanged
                db.Entry(item.Person!).State = EntityState.Unchanged;

                // Documents
                foreach (var document in item.Documents!)
                    db.Entry(document).State = EntityState.Unchanged;

                // Records
                foreach (var record in item.Records!)
                {
                    db.Entry(record).State = EntityState.Added;
                    db.Entry(record.CntrType!).State = EntityState.Detached;

                    foreach (var recordContent in record.Contents)
                        db.Entry(recordContent).State = EntityState.Added;
                }

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(msg);
                return appObjResponse;
            }
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> RemoveItemAsync(long id)
    {
        appObjResponse = new();
        return appObjResponse;
    }

    public async Task<IEnumerable<PersonEntity>> GetPersonAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Item = db.Persons.AsNoTracking().ToList();

            return Item!;
        }
    }


    // DELETE
    public async Task<AppObjectResponse> NewExportOrderRecordAsync(ExportOrderRecord item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                // check an Existing item
                var itemExistCheck = await db.Set<ExportOrderRecord>().Where(s => s.CntrNum == item.CntrNum).FirstOrDefaultAsync();
                if (itemExistCheck != null)
                {
                    appObjResponse.ErrorAdd($"Container {item.CntrNum} is exists already.");
                    return appObjResponse;
                }
                
                db.Entry(item.CntrType!).State = EntityState.Unchanged;
                db.Entry(item.ExportOrder!).State = EntityState.Modified;

                foreach (var content in item.Contents)
                {
                    db.Entry(content).State = EntityState.Added;
                    db.Entry(content.DocumentRecord).State = EntityState.Unchanged;
                    db.Entry(content.DocumentRecord.Document).State = EntityState.Unchanged;
                }

                db.Entry(item).State = EntityState.Added;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(msg);
                return appObjResponse;
            }
        }

        return appObjResponse;
    }

    #region AUXIALARY

    //Func<IEnumerable<ExportOrderRecord>, IEnumerable<ExportOrderRecordDTO>> eoRecords = (_eoRecords) =>
    //{
    //    var eoRecordsDTO = new List<ExportOrderRecordDTO>();
    //    var eoRecordDTO = new ExportOrderRecordDTO();

    //    uint indexRec = 0;

    //    foreach (var record in _eoRecords)
    //    {
    //        ++indexRec;
    //        eoRecordDTO.Seq = indexRec.ToString();
    //        eoRecordDTO.CntrTareWt = record.CntrTareWt;
    //        eoRecordDTO.Seal = record.Seal;
    //        eoRecordDTO.CntrType = record.CntrType!.Normolize!;

    //        foreach (var content in record.Contents)
    //        {
    //            eoRecordDTO.Cntr = record.CntrNum;
                
    //            eoRecordDTO.PackageQty = content.PackageQty;
    //            eoRecordDTO.PackageName = content.PackageName is not null ? content.PackageName.ToUpper() : "";
    //            eoRecordDTO.NetWt = content.NetWt;
    //            eoRecordDTO.GrossWt = content.GrossWt;
    //            eoRecordDTO.Volume = content.Volume;

    //            eoRecordDTO.DocumentName = content.DocumentRecord.Document.Name!;
    //            eoRecordDTO.Shipper = content.DocumentRecord.Document.Shipper!.Name!;
    //            eoRecordDTO.ShipperEn = content.DocumentRecord.Document.Shipper!.NameEn!;
    //            eoRecordDTO.Consignee = content.DocumentRecord.Document.Consignee!.Name!;
    //            eoRecordDTO.ConsigneeEn = content.DocumentRecord.Document.Consignee!.NameEn!;

    //            eoRecordDTO.CommodityName = content.DocumentRecord.CommodityName;
    //            eoRecordDTO.CommodityEngName = content.DocumentRecord.CommodityEngName;
    //            eoRecordDTO.HSCode = content.DocumentRecord.CommodityHSCode!;
    //            eoRecordDTO.IMO = content.DocumentRecord.IsIMO ? "IMO:" + content.DocumentRecord.IMO! : "";
    //            eoRecordDTO.UNNO = content.DocumentRecord.IsIMO ? "UN:" + content.DocumentRecord.UNNO! : "";
    //            eoRecordDTO.IsIMO = content.DocumentRecord.IsIMO;

    //            eoRecordsDTO.Add(eoRecordDTO);
    //            eoRecordDTO = new ExportOrderRecordDTO();
    //        }
    //    }

    //    return eoRecordsDTO.ToArray();
    //};

    //DELETE

    Func<IEnumerable<ExportOrderEntity>, IEnumerable<BLDTO>> blRecords = (_mRecords) =>
    {
        var BLDTO_List = new List<BLDTO>();
        var bLDTO = new BLDTO();

        foreach (var Item in _mRecords)
        {
            bLDTO.BLDate = Item.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy");
            bLDTO.Voyage = Item.VesselCallDetail!.VesselCall!.VoyageNo;
            bLDTO.VesselName = Item.VesselCallDetail!.VesselCall!.Vessel.Name!;
            bLDTO.VesselFlagEn = Item.VesselCallDetail!.VesselCall!.Vessel.Flag!.ENG;
            bLDTO.PODEn = Item.VesselCallDetail!.POD!.NameEn + ", " + Item.VesselCallDetail!.POD.Country!.ENG;
            bLDTO.TotalCntrCount = Item.Records.Count;
            bLDTO.TotalGrossWeight = Item.Records.Sum(r => r.Contents.Sum(c => c.GrossWt));
            bLDTO.TotalTareWeight = Item.Records.Sum(r => r.CntrTareWt);

            uint indexRec = 0;

            foreach (var record in Item.Records)
            {
                bLDTO.Seq = ++indexRec;
                bLDTO.BLNum = Item.Num;
                bLDTO.Seal = record.Seal;
                bLDTO.Cntr = record.CntrNum;
                bLDTO.CntrType = record.CntrType!.Normolize!;       // used for calculation of Totals in Report's Parameters
                bLDTO.CntrTareWt = record.CntrTareWt;

                foreach (var content in record.Contents)
                {
                    bLDTO.Commodity = content.DocumentRecord.CommodityEngName;
                    bLDTO.PackageQty = content.PackageQty;
                    bLDTO.PackageName = content.PackageName is not null ? content.PackageName.ToUpper() : "";

                    bLDTO.NetWt = content.NetWt;
                    bLDTO.GrossWt = content.GrossWt;
                    bLDTO.Volume = content.Volume;
                    bLDTO.IMO = content.DocumentRecord.IMO!;
                    bLDTO.UNNO = content.DocumentRecord.UNNO!;
                    bLDTO.IsIMO = content.DocumentRecord.IsIMO!;

                    bLDTO.Shipper = content.DocumentRecord.Document.Shipper!.NameEn!;
                    bLDTO.Consignee = content.DocumentRecord.Document.Consignee!.NameEn!;

                    BLDTO_List.Add(bLDTO);
                    bLDTO = new BLDTO();
                }
            }
        }

        return BLDTO_List;
    };

    Func<IEnumerable<ExportOrderRecord>, IEnumerable<ExportOrderRecordDTO>> eoRecords = (_eoRecords) =>
    {
        var eoRecordsDTO = new List<ExportOrderRecordDTO>();
        var eoRecordDTO = new ExportOrderRecordDTO();

        uint indexRec = 0;

        foreach (var record in _eoRecords)
        {
            ++indexRec;
            eoRecordDTO.Seq = indexRec.ToString();
            eoRecordDTO.CntrTareWt = record.CntrTareWt;
            eoRecordDTO.Seal = record.Seal;
            eoRecordDTO.CntrType = record.CntrType!.Normolize!;

            foreach (var content in record.Contents)
            {
                eoRecordDTO.Cntr = record.CntrNum;

                eoRecordDTO.PackageQty = content.PackageQty;
                eoRecordDTO.PackageName = content.PackageName is not null ? content.PackageName.ToUpper() : "";
                eoRecordDTO.NetWt = content.NetWt;
                eoRecordDTO.GrossWt = content.GrossWt;
                eoRecordDTO.Volume = content.Volume;

                eoRecordDTO.DocumentName = content.DocumentRecord.Document.Name!;
                eoRecordDTO.Shipper = content.DocumentRecord.Document.Shipper!.Name!;
                eoRecordDTO.ShipperEn = content.DocumentRecord.Document.Shipper!.NameEn!;
                eoRecordDTO.Consignee = content.DocumentRecord.Document.Consignee!.Name!;
                eoRecordDTO.ConsigneeEn = content.DocumentRecord.Document.Consignee!.NameEn!;

                eoRecordDTO.CommodityName = content.DocumentRecord.CommodityName;
                eoRecordDTO.CommodityNameEn = content.DocumentRecord.CommodityEngName;
                eoRecordDTO.HSCode = content.DocumentRecord.CommodityHSCode!;
                eoRecordDTO.IMO = content.DocumentRecord.IsIMO ? "IMO:" + content.DocumentRecord.IMO! : "";
                eoRecordDTO.UNNO = content.DocumentRecord.IsIMO ? "UN:" + content.DocumentRecord.UNNO! : "";
                eoRecordDTO.IsIMO = content.DocumentRecord.IsIMO;

                eoRecordsDTO.Add(eoRecordDTO);
                eoRecordDTO = new ExportOrderRecordDTO();
            }
        }

        return eoRecordsDTO.ToArray();
    };

    Func<IEnumerable<ExportOrderEntity>, IEnumerable<ManifestDTO>> mRecords = (_mRecords) =>
    {
        var ItemsDTO = new List<ManifestDTO>();
        var itemDTO = new ManifestDTO();

        foreach (var Item in _mRecords)
        {
            itemDTO.BLDate = Item.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy");
            itemDTO.Voyage = Item.VesselCallDetail!.VesselCall!.VoyageNo;
            itemDTO.VesselName = Item.VesselCallDetail!.VesselCall!.Vessel.Name!;
            itemDTO.VesselFlagEn = Item.VesselCallDetail!.VesselCall!.Vessel.Flag!.ENG;
            itemDTO.PODEn = Item.VesselCallDetail!.POD!.NameEn + ", " + Item.VesselCallDetail!.POD.Country!.ENG;
            itemDTO.TotalCntrCount = Item.Records.Count;
            itemDTO.TotalGrossWeight = Item.Records.Sum(r => r.Contents.Sum(c => c.GrossWt));
            itemDTO.TotalTareWeight = Item.Records.Sum(r => r.CntrTareWt);

            uint indexRec = 0;

            foreach (var record in Item.Records)
            {
                itemDTO.Seq = ++indexRec;
                itemDTO.Seal = record.Seal;
                itemDTO.CntrTareWt = record.CntrTareWt;

                foreach (var content in record.Contents)
                {
                    itemDTO.BLNum = Item.Num;
                    itemDTO.Cntr = record.CntrNum;
                    itemDTO.CntrType = record.CntrType!.Normolize!;       // used for calculation of Totals in Report's Parameters

                    itemDTO.Commodity = content.DocumentRecord.CommodityEngName;
                    itemDTO.PackageQty = content.PackageQty;
                    itemDTO.PackageName = content.PackageName is not null ? content.PackageName.ToUpper() : "";

                    itemDTO.NetWt = content.NetWt;
                    itemDTO.GrossWt = content.GrossWt;
                    itemDTO.Volume = content.Volume;
                    itemDTO.IMO = content.DocumentRecord.IMO!;
                    itemDTO.UNNO = content.DocumentRecord.UNNO!;
                    itemDTO.IsIMO = content.DocumentRecord.IsIMO!;

                    itemDTO.Shipper = content.DocumentRecord.Document.Shipper!.NameEn!;
                    itemDTO.Consignee = content.DocumentRecord.Document.Consignee!.NameEn!;

                    ItemsDTO.Add(itemDTO);
                    itemDTO = new ManifestDTO();
                }
            }
        }

        return ItemsDTO;
    };

    Func<IEnumerable<ExportOrderEntity>, IEnumerable<ExportOrderComponentDTO>> eoComponentRecord = (Records) =>
    {
        var RecordsDTO = new List<ExportOrderComponentDTO>();

        foreach (var record in Records)
        {

            var recordDTO = new ExportOrderComponentDTO()
            {
                Id = record.Id,
                Num = record.Num,
                Dated = record.Dated.ToShortDateString(),
                Vessel = record.VesselCallDetail!.VesselCall!.Vessel.Name!,
                Voyage = record.VesselCallDetail!.VesselCall.VoyageNo,
                POD = record.VesselCallDetail!.POD!.Name!,
                Carrier = record.Carrier!.NameEn,
                Status = record.Status,
            };

            RecordsDTO.Add(recordDTO);
        }

        return RecordsDTO;
    };


    #endregion

}
