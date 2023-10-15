using ExportOrderEntites.MyCompany;
using Microsoft.EntityFrameworkCore;
using static MudBlazor.CategoryTypes;


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
                appObjResponse.Object = await db.ExportOrders.Include(eo => eo.Person)
                                                             .Include(eo => eo.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                                             .Include(eo => eo.Documents)!.ThenInclude(d => d.Records)
                                                             .Include(eo => eo.Records)!.ThenInclude(r => r.Contents)
                                                                                        .ThenInclude(c => c.DocumentRecord)
                                                                                        .ThenInclude(dr => dr.Document)
                                                             .AsNoTracking()
                                                             .FirstOrDefaultAsync(s => s.Id == id);
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return appObjResponse;
            }
        }

        return appObjResponse;
    }

    public async Task<ExportOrderDTO> GetItemDTOAsync(long eoId)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var myCompany = await db.MyCompany.AsNoTracking().FirstOrDefaultAsync();

            //var BLitem = await db.ExportOrders.Include(x => x.Records)!.ThenInclude(r => r.CntrType).AsNoTracking().FirstOrDefaultAsync(s => s.Id == eoId);

            var Item = await db.ExportOrders.Include(x => x.VesselCall).ThenInclude(vc => vc!.Terminal)
                                            .Include(x => x.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCall).ThenInclude(vc => vc!.Details).ThenInclude(d => d.POD).ThenInclude(p => p!.Country)
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
                                                CarrierNameEn = source.Carrier!.NameEn,
                                                VesselName = source.VesselCall!.Vessel.Name!,
                                                VesselFlag = source.VesselCall.Vessel.Flag!.RUS,
                                                VesselFlagEn = source.VesselCall.Vessel.Flag.ENG,
                                                Voyage = source.VesselCall.VoyageNo,
                                                DateOfLoading = source.VesselCall.ETA!.Value.ToString("dd.MM.yyyy"),
                                                BLDate = source.VesselCall.ETS!.Value.ToString("dd.MM.yyyy"),
                                                POD = source.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == source.VesselCall.Id)!.POD!.Name! + ", " +
                                                        source.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == source.VesselCall.Id)!.POD!.Country.RUS,
                                                PODEn = source.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == source.VesselCall.Id)!.POD!.NameEn! + ", " +
                                                        source.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == source.VesselCall.Id)!.POD!.Country.ENG,
                                                PODAgent = source.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == source.VesselCall.Id)!.AgentPOD,
                                                Measurement = source.Records.FirstOrDefault()!.Contents.FirstOrDefault()!.Volume > 0 ? "CBM" : "KG",
                                                TotalCntrCount = source.Records.Count,
                                                TotalGrossWeight = source.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                                                TotalTareWeight = source.Records.Sum(r => r.CntrTareWt),
                                                //Contract = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name).Contract != null ?
                                                //          source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name).Contract : null,
                                                Contract = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.Terminal.Name)!.Contract,
                                                //ContractDate = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name)!.DateContract != null ?
                                                //          source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") : "",
                                                ContractDate = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy"),
                                                MyCompanyName = myCompany!.Name!,
                                                Person = source.Person!.Name + " т. " + source.Person.Phone,
                                                exportOrderRecordsDTO = eoRecords(source.Records!)
                                            })
                                            .FirstOrDefaultAsync(x => x.Id == eoId);

            return Item!;
        }
    }

    public async Task<IEnumerable<BLDTO>> GetManifestItemsAsync(long vslCallId, long carrierId)
    {
        try { 
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Items = await db.ExportOrders.Include(x => x.VesselCall).ThenInclude(vc => vc!.Terminal)
                                 .Include(x => x.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                 //.Include(x => x.VesselCall).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod.Country)
                                 .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                 .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                 .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                 .AsNoTracking()
                                 .Where(x => x.VesselCall!.Id == vslCallId)
                                 .Where(x => x.Carrier!.Id == carrierId)
                                 .AsSplitQuery()
                                 .ToListAsync();
            
            var ManifestItems = mRecords(Items);

            return ManifestItems;
        }

        }
        catch (Exception ex)
        {
            string message = ex.Message;
            return Enumerable.Empty<BLDTO>();
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
                                                    .Include(eo => eo.VesselCall).ThenInclude(vc => vc!.Vessel)
                                                    .Include(eo => eo.VesselCall).ThenInclude(vc => vc!.Terminal)
                                                    .Include(eo => eo.VesselCall).ThenInclude(vc => vc!.Details).ThenInclude(d => d.POD)
                                                    .Include(eo => eo.Carrier)
                                                    .Where(eo => filter.Dated.HasValue ? eo.Dated == filter.Dated: true)
                                                    .AsNoTracking()
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
                item.CreateTime = DateTime.Now;
                item.Status = EntityStatus.Pending;

                db.Entry(item).State = EntityState.Added;

                db.Entry(item.Carrier!).State = EntityState.Unchanged;
                db.Entry(item.VesselCall!).State = EntityState.Unchanged;
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


    #region AUXIALARY

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
                eoRecordDTO.CommodityEngName = content.DocumentRecord.CommodityEngName;
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

    Func<IEnumerable<ExportOrderEntity>, IEnumerable<BLDTO>> mRecords = (_mRecords) =>
    {
        var BLDTO_List = new List<BLDTO>();
        var bLDTO = new BLDTO();

        foreach (var Item in _mRecords)
        {
            bLDTO.BLDate = Item.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy");
            bLDTO.Voyage = Item.VesselCall!.VoyageNo;
            bLDTO.VesselName = Item.VesselCall!.Vessel.Name!;
            bLDTO.VesselFlagEn = Item.VesselCall.Vessel.Flag!.ENG;
            //bLDTO.PODEn = Item.VesselCall.POD.NameEn + ", " + Item.VesselCall.POD.Country.ENG;
            bLDTO.PODEn = Item.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == Item.VesselCall.Id)!.POD!.NameEn + ", " +
                            Item.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == Item.VesselCall.Id)!.POD!.Country.ENG;
            bLDTO.TotalCntrCount = Item.Records.Count;
            bLDTO.TotalGrossWeight = Item.Records.Sum(r => r.Contents.Sum(c => c.GrossWt));
            bLDTO.TotalTareWeight = Item.Records.Sum(r => r.CntrTareWt);

            uint indexRec = 0;

            foreach (var record in Item.Records)
            {
                bLDTO.Seq = ++indexRec;
                bLDTO.Seal = record.Seal;
                bLDTO.CntrTareWt = record.CntrTareWt;
                
                foreach (var content in record.Contents)
                {
                    bLDTO.BLNum = Item.Num;
                    bLDTO.Cntr = record.CntrNum;
                    bLDTO.CntrType = record.CntrType!.Normolize!;       // used for calculation of Totals in Report's Parameters

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

    Func<IEnumerable<ExportOrderEntity>, IEnumerable<ExportOrderComponentDTO>> eoComponentRecord = (Records) =>
    {
        var RecordsDTO = new List<ExportOrderComponentDTO>();        

        foreach (var record in Records)
        {
            foreach (var detail in record.VesselCall!.Details)
            {
                var recordDTO = new ExportOrderComponentDTO()
                {
                    Id = record.Id,
                    Num = record.Num,
                    Dated = record.Dated.ToShortDateString(),
                    Vessel = record.VesselCall!.Vessel.Name!,
                    Voyage = record.VesselCall.VoyageNo,
                    POD = record.VesselCall.Details.FirstOrDefault(d => d.VesselCall!.Id == record.VesselCall.Id)!.POD!.Name!,
                    Carrier = record.Carrier!.NameEn,
                    Status = record.Status,
                };

                RecordsDTO.Add(recordDTO);
            }            
        }

        return RecordsDTO;
    };

    #endregion

}
