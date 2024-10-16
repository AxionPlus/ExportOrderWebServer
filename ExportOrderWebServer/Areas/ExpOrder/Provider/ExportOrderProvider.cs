public class ExportOrderProvider : IExportOrderProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public ExportOrderProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
        appObjResponse = new();
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
                string msg = ex.Message;
                appObjResponse.ErrorAdd(msg);
                return appObjResponse;
            }
        }

        return appObjResponse;
    }

    public async Task<ExportOrderDTO> GetExportOrderDTOAsync(long Id)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var myCompany = await db.MyCompany.OrderBy(c => c.Id).LastOrDefaultAsync();

                //var vesselName = Voyage.Split('&')[0];
                //var voyageNo = Voyage.Split('&')[1];
                //var pod = Voyage.Split('&')[2];

                var Item = await db.ExportOrders.AsNoTracking().AsSplitQuery()
                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Vessel).ThenInclude(vsl => vsl.Flag)
                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
                                .Include(x => x.Carrier).ThenInclude(c => c!.CarrierDetails)
                                .Include(x => x.Person)
                                .Include(x => x.Documents).ThenInclude(d => d.Records)
                                .Include(x => x.Records).ThenInclude(r => r.CntrType)
                                .Include(x => x.Records).ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                .Select(source => new ExportOrderDTO()
                                {
                                    Id = source.Id,
                                    Num = source.Num,
                                    BLNum = source.Num.IndexOf("_") == -1 ? source.Num : source.Num.Substring(0, source.Num.IndexOf("_")),
                                    Dated = source.Dated.HasValue ? source.Dated.Value.ToString("dd.MM.yyyy") : DateTime.Today.ToString("dd.MM.yyyy"),
                                    xmlDated = source.Dated.HasValue ? source.Dated.Value.ToString("dd.MM.yyyy hh:mm:ss") : DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"),
                                    DateOfLoading = source.VesselCallDetail!.VesselCall!.ETA!.Value.ToString("dd.MM.yyyy"),
                                    BLDate = source.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy"),
                                    BLtemplate = source.Carrier!.BlTemplate.ToString(),
                                    Shippers = string.Join(";\n", source.Documents.GroupBy(d => d.Shipper!.Name).Select(g => g.Key).Distinct().ToList()),
                                    Consignees = string.Join(";\n", source.Documents.GroupBy(d => d.Consignee!.Name).Select(g => g.Key).Distinct().ToList()),
                                    Commodities = string.Join(";\n", source.Records.SelectMany(
                                        eor => eor.Contents.Select(
                                            rc => rc.DocumentRecord.CommodityName +
                                            (rc.DocumentRecord.IsIMO ? " IMO:" + rc.DocumentRecord.IMO + " UNNO:" + rc.DocumentRecord.UNNO : ""))
                                        ).Distinct().ToList()),
                                    CommodityShort = source.CommodityShort,
                                    CustomsOfficeCode = source.VesselCallDetail!.VesselCall.Terminal.Customs!.Code,
                                    CustomsOfficeNameShort = source.VesselCallDetail.VesselCall.Terminal.Customs.OfficeShort,
                                    CarrierNameEn = source.Carrier!.NameEn,
                                    TerminalName = source.VesselCallDetail == null ? string.Empty : source.VesselCallDetail.VesselCall.Terminal.Name,
                                    VesselName = source.VesselCallDetail == null ? string.Empty :
                                                    string.IsNullOrEmpty(source.VesselCallDetail.VesselCall.Vessel.Name) ? string.Empty :
                                                        source.VesselCallDetail.VesselCall.Vessel.Name,
                                    VesselFlag = source.VesselCallDetail == null ? string.Empty :
                                                    source.VesselCallDetail.VesselCall.Vessel.Flag == null ? string.Empty :
                                                        source.VesselCallDetail.VesselCall.Vessel.Flag.RUS,
                                    VesselFlagEn = source.VesselCallDetail == null ? string.Empty :
                                                    source.VesselCallDetail.VesselCall.Vessel.Flag == null ? string.Empty :
                                                        source.VesselCallDetail.VesselCall.Vessel.Flag.ENG,
                                    Voyage = source.VesselCallDetail == null ? string.Empty :
                                                source.VesselCallDetail.VesselCall.VoyageNo,
                                    POD = source.VesselCallDetail == null ? string.Empty :
                                            source.VesselCallDetail.POD == null ? string.Empty :
                                                source.VesselCallDetail.POD.Country == null ? string.Empty :
                                                source.VesselCallDetail.POD.Name + ", " + source.VesselCallDetail.POD.Country.RUS,
                                    PODEn = source.VesselCallDetail == null ? string.Empty :
                                                source.VesselCallDetail.POD == null ? string.Empty :
                                                    source.VesselCallDetail.POD.Country == null ? string.Empty :
                                                        source.VesselCallDetail.POD.NameEn + ", " + source.VesselCallDetail.POD.Country.ENG,
                                    PODwithCountryRus = source.VesselCallDetail == null ? string.Empty :
                                                            source.VesselCallDetail.POD == null ? string.Empty :
                                                            source.VesselCallDetail.POD.Country == null ? string.Empty :
                                                                source.VesselCallDetail.POD.NameEn + ", " + source.VesselCallDetail.POD.Country.RUS,
                                    FinalDestination = source.VesselCallDetail == null ? string.Empty :
                                                        source.VesselCallDetail.FinalDestination == null ? string.Empty :
                                                            source.VesselCallDetail.FinalDestination.Country == null ? string.Empty :
                                                                source.VesselCallDetail.FinalDestination.NameEn + ", " + source.VesselCallDetail.FinalDestination.Country.ENG,
                                    POLAgent = source.Carrier == null ? string.Empty :
                                               source.VesselCallDetail == null ? string.Empty :
                                                    source.VesselCallDetail.VesselCall.Terminal.Name == null ? string.Empty :
                                                    source.Carrier.CarrierDetails.FirstOrDefault(cd => cd.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.AgentPOL,
                                    PODAgent = source.VesselCallDetail == null ? string.Empty : source.VesselCallDetail.AgentPOD,
                                    Measurement = source.Records.FirstOrDefault()!.Contents.FirstOrDefault()!.Volume > 0 ? "CBM" : "KG",
                                    TotalCntrCount = (uint)source.Records.Count,
                                    TotalPackages = (uint)source.Records.Sum(r => r.Contents.Sum(c => c.PackageQty))!,
                                    TotalGrossWeight = source.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                                    TotalNetWeight = source.Records.Sum(r => r.Contents.Sum(c => c.NetWt)),
                                    TotalTareWeight = source.Records.Sum(r => r.CntrTareWt),
                                    Contract = source.Carrier == null ? null :
                                                source.VesselCallDetail == null ? null :
                                                    string.IsNullOrEmpty(source.VesselCallDetail.VesselCall.Terminal.Name) ? null :
                                                        source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.Contract,
                                    ContractDate = source.Carrier == null ? null :
                                                    source.VesselCallDetail == null ? null :
                                                    string.IsNullOrEmpty(source.VesselCallDetail.VesselCall.Terminal.Name) ? null :
                                                        source.Carrier.CarrierDetails
                                                            .FirstOrDefault(s => s.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract.HasValue ?
                                                        source.Carrier.CarrierDetails
                                                            .FirstOrDefault(s => s.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") : "",
                                    MyCompanyName = myCompany == null ? null : myCompany.Name,
                                    Person = source.Person == null ? string.Empty :
                                                source.Person.Name == null ? string.Empty :
                                                    source.Person.Name.Substring(0, 1) + ". " +
                                                    (source.Person.SurName == null ? string.Empty : source.Person.SurName.Substring(0, 1) + ". ") +
                                                    source.Person.FamilyName +
                                                    "  т. " + source.Person.Phone,
                                    PersonXml = source.Person == null ? string.Empty :
                                                    source.Person.Name + " " + source.Person.FamilyName + " телефон: " + source.Person.Phone,
                                    PersonPass = source.Person == null ? string.Empty : source.Person.Passport,
                                    PersonPhone = source.Person == null ? string.Empty : source.Person.Phone,
                                    exportOrderRecordsDTO = eoRecords(source.Records)
                                })
                                .FirstOrDefaultAsync(x => x.Id == Id);

                return Item!;
            }
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return new ExportOrderDTO();
        }
    }

    public async Task<ExportOrderDTO> GetBLDTOAsync(long Id)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var Item = await db.ExportOrders.Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
                                .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                .Include(x => x.Carrier)
                                .Include(x => x.Person)
                                .Include(x => x.Documents)!.ThenInclude(d => d.Records)
                                .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                .AsNoTracking()
                                .Select(source => new ExportOrderDTO()
                                {
                                    Id = source.Id,
                                    Num = source.Num,
                                    BLNum = source.Num.IndexOf("_") == -1 ? source.Num : source.Num.Substring(0, source.Num.IndexOf("_")),
                                    BLDate = source.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy"),
                                    BLDateOEL = source.VesselCallDetail!.VesselCall!.ETS!.HasValue ? DateToStr(source.VesselCallDetail!.VesselCall!.ETS!.Value) : null,
                                    BLtemplate = source.Carrier!.BlTemplate.ToString(),
                                    CarrierNameEn = source.Carrier!.NameEn,
                                    TerminalName = source.VesselCallDetail!.VesselCall.Terminal.Name,
                                    VesselName = source.VesselCallDetail!.VesselCall!.Vessel.Name!,
                                    VesselFlagEn = source.VesselCallDetail!.VesselCall!.Vessel.Flag!.ENG,
                                    Voyage = source.VesselCallDetail!.VesselCall!.VoyageNo,
                                    Shippers = string.Join("; ", source.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.NameEn)).Distinct().ToList()),
                                    Consignees = string.Join("; ", source.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.NameEn)).Distinct().ToList()),
                                    Commodities = string.Join("; ", source.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.CommodityEngName +
                                                                                                                                (rc.DocumentRecord.IsIMO ?
                                                                                                                                    " IMO:" + rc.DocumentRecord.IMO +
                                                                                                                                    " UNNO:" + rc.DocumentRecord.UNNO : "")))
                                                                                                                 .Distinct().ToList()),
                                    PODEn = source.VesselCallDetail!.POD!.NameEn!,
                                    PODwithCountryEn = source.VesselCallDetail!.POD!.NameEn! + ", " + source.VesselCallDetail!.POD!.Country!.ENG,
                                    PODwithCountryRus = source.VesselCallDetail!.POD!.NameEn! + ", " + source.VesselCallDetail!.POD!.Country.RUS,
                                    FinalDestination = source.VesselCallDetail!.FinalDestination!.NameEn! + ", " + source.VesselCallDetail!.FinalDestination!.Country!.ENG,
                                    POLAgent = source.Carrier!.CarrierDetails.FirstOrDefault(cd => cd.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.AgentPOL!,
                                    PODAgent = source.VesselCallDetail!.AgentPOD,
                                    Measurement = source.Records.FirstOrDefault()!.Contents.FirstOrDefault()!.Volume > 0 ? "CBM" : "KG",

                                    TotalCntrCount = (uint)source.Records.Count,
                                    TotalPackages = (uint)source.Records.Sum(r => r.Contents.Sum(c => c.PackageQty))!,
                                    TotalGrossWeight = source.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                                    TotalNetWeight = source.Records.Sum(r => r.Contents.Sum(c => c.NetWt)),
                                    TotalTareWeight = source.Records.Sum(r => r.CntrTareWt),
                                    TotalGrossNTareWeight = source.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)) + source.Records.Sum(r => r.CntrTareWt),
                                    exportOrderRecordsDTO = blRecords(source.Records!)
                                })
                                .FirstOrDefaultAsync(x => x.Id == Id);

                if (Item != null)
                {
                    if (Item.BLtemplate.Equals("ametist"))
                        Item.BLNum = string.Concat("SV-", Item.BLNum);

                    return Item;
                }
                else
                    return new ExportOrderDTO();
            }
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
            Console.WriteLine(msg);
            return new ExportOrderDTO();
        }
    }

    public async Task<IEnumerable<VoyageManifestDTO>> GetVoyageManifestDTOAsync(long id, bool isImo)
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
            return Enumerable.Empty<VoyageManifestDTO>();
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
                                                        .Where(s => string.IsNullOrEmpty(filter.Carrier) ? true : s.Carrier!.NameEn == filter.Carrier)
                                                        .ToArrayAsync();
                               
               var ItemsDTO = exportOrders.Select(record=>
                    new ExportOrderComponentDTO()
                    {
                        Id = record.Id,
                        Num = record.Num,
                        Dated = record.Dated.HasValue ? record.Dated.Value.ToString("dd.MM.yy") : "---",
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
                        BlTemplate = record.Carrier!.BlTemplate,
                        CreateTime = record.CreateTime,
                        VersionNo = record.VersionNo,
                    });

                if (!string.IsNullOrEmpty(filter.Voyage))
                    ItemsDTO = ItemsDTO.OrderByDescending(s => s.Dated);
                else
                    ItemsDTO = ItemsDTO.OrderByDescending(s => s.CreateTime);

                appObjResponse.Object = ItemsDTO.ToArray();
                //appObjResponse.Object = ItemsDTO;
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

    public async Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item, string UserName = "")
    {
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

                /// check CntrNum duplicate
                if (modifyItem!.Num != item.Num)
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
                modifyItem.CreateUser = User!;
                modifyItem.CreateTime = DateTime.Now;
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
                    {
                        db.Entry(content).State = EntityState.Added;
                    }

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

            try
            {
                /// check an Existing item
                bool isItemExist = db.Set<ExportOrderEntity>().Any(s => s.Num.ToUpper() == item.Num.ToUpper());
                if (isItemExist)
                {
                    appObjResponse.ErrorAdd($"Export Order {item.Num} exists already.");
                    return appObjResponse;
                }

                item.CreateUser = User!;

                db.Entry(item).State = EntityState.Added;

                db.Entry(item.Carrier!).State = EntityState.Unchanged;
                db.Entry(item.Person!).State = EntityState.Unchanged;
                db.Entry(item.VesselCallDetail!).State = EntityState.Modified;

                /// Documents
                foreach (var document in item.Documents!)
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

    public async Task<int> LastVersionAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var last = await db.ExportOrders.AsNoTracking().OrderByDescending(s => s.CreateTime).Select(s => s.VersionNo).FirstOrDefaultAsync();

            return last == 0 ? 1 : last;
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
            string msg = ex.Message;
            appObjResponse.ErrorAdd(msg);
            Console.WriteLine($"{msg}");
            return Enumerable.Empty<PersonEntity>();
        }
    }

    public async Task<AppObjectResponse> GetExportOrderRecordItemAsync(long id)
    {


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

    public async Task<AppObjectResponse> SetNewVesselCall(IEnumerable<long> exportOrderIds, long vesselCallDetailId)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var VesselCallDetail = await db.Set<VesselCallDetail>().FirstOrDefaultAsync(s => s.Id == vesselCallDetailId);

                var ExportOrders = await db.ExportOrders.AsTracking()     // AsNoTracking
                                                        .Where(s => exportOrderIds.Any(i => i == s.Id))
                                                        .ToArrayAsync();

                if (VesselCallDetail is null)
                {
                    appObjResponse.ErrorAdd("Рейс не найден.");
                    return appObjResponse;
                }

                foreach (var exportOrder in ExportOrders)
                {
                    exportOrder.VesselCallDetail = VesselCallDetail;

                    //db.Entry(exportOrder.Carrier!).State = EntityState.Unchanged;
                    //db.Entry(exportOrder.Person!).State = EntityState.Unchanged;
                    db.Entry(exportOrder.VesselCallDetail).State = EntityState.Unchanged;

                    db.Entry(exportOrder).State = EntityState.Modified;
                }

                //db.Entry(VesselCallDetail).State = EntityState.Unchanged;   // excluded out of foreach

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
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

    public Task<AppObjectResponse> ModifyItemAsync(ExportOrderEntity item)
    {
        throw new NotImplementedException();
    }

    //public async Task<IEnumerable<VoyageExportOrderDTO>> GetExportOrdersAsync(IEnumerable<long> Ids)
    //{
    //    try
    //    {
    //        using (var _db = _dbContext.CreateDbContextAsync())
    //        {
    //            var db = await _db;

    //            string? myCompanyName = await db.MyCompany.OrderBy(c => c.Id).Select(c => c.Name).LastOrDefaultAsync();

    //            //var DTOItems = new();

    //            var Items = await db.ExportOrders.AsNoTracking().AsSplitQuery()
    //                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
    //                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
    //                                .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.POD).ThenInclude(p => p!.Country)
    //                                .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
    //                                .Include(x => x.Person)
    //                                //.Include(x => x.Documents)!.ThenInclude(d => d.Records)
    //                                .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
    //                                .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
    //                                .Where(x => Ids.Contains(x.Id))
    //                                //.Where(x => Ids.Any(hs => hs.Equals(x.Id)))
    //                                .ToListAsync();

    //            if (Items is null || !Items.Any()) return Enumerable.Empty<VoyageExportOrderDTO>();

    //            var DTOItems = FuncExportOrdersDTO(Items, myCompanyName);

    //            return DTOItems;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        string msg = ex.Message;
    //        Console.WriteLine(msg);
    //        return Enumerable.Empty<VoyageExportOrderDTO>();
    //    }
    //}


    #region AUXILIARY

    Func<IEnumerable<ExportOrderRecord>, IEnumerable<ExportOrderRecordDTO>> eoRecords = (_eoRecords) =>
    {
        var eoRecordsDTO = new List<ExportOrderRecordDTO>();
        var eoRecordDTO = new ExportOrderRecordDTO();

        uint indexRec = 0;

        foreach (var record in _eoRecords)
        {
            eoRecordDTO.Seq = ++indexRec;
            eoRecordDTO.CntrType = record.CntrType is null ? string.Empty : record.CntrType.Normolize!;
            eoRecordDTO.CntrTypeISO = record.CntrType is null ? string.Empty : record.CntrType.ISO;
            eoRecordDTO.CntrTareWt = record.CntrTareWt;
            eoRecordDTO.Seal = record.Seal is null ? string.Empty : record.Seal;

            //uint indexContent = 0;
            foreach (var content in record.Contents)
            {
                eoRecordDTO.SeqContent = (uint)content.DocumentRecord.Seq;
                eoRecordDTO.Cntr = record.CntrNum;

                eoRecordDTO.PackageQty = content.PackageQty is null ? 0 : (uint)content.PackageQty;
                eoRecordDTO.PackageName = content.PackageName is null ? string.Empty : content.PackageName.ToUpper();
                eoRecordDTO.NetWt = content.NetWt is null ? 0 : content.NetWt;
                eoRecordDTO.GrossWt = content.GrossWt is null ? 0 : content.GrossWt;
                eoRecordDTO.Volume = content.Volume is null ? 0 : content.Volume;
                eoRecordDTO.GrossAndTare = record.CntrTareWt + (double)content.GrossWt!;

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

        return eoRecordsDTO.ToArray();
    };

    Func<IEnumerable<ExportOrderRecord>, IEnumerable<ExportOrderRecordDTO>> blRecords = (_eoRecords) =>
    {
        var eoRecordsDTO = new List<ExportOrderRecordDTO>();

        uint indexRec = 0;

        foreach (var record in _eoRecords)
        {
            var eoRecordDTO = new ExportOrderRecordDTO()
            {
                Seq = ++indexRec,
                Cntr = record.CntrNum,
                CntrType = record.CntrType!.Normolize!,
                CntrTareWt = record.CntrTareWt,
                Seal = record.Seal is not null ? record.Seal : string.Empty,

                PackageQty = (uint)record.Contents.Sum(c => c.PackageQty)! > 0 ? (uint)record.Contents.Sum(c => c.PackageQty)! : null,
                PackageName = string.Join(", ", record.Contents.Select(rc => rc.PackageName is not null ? rc.PackageName.ToUpper() : "").Distinct().Order()),
                NetWt = record.Contents.Sum(c => c.NetWt),
                GrossWt = record.Contents.Sum(c => c.GrossWt),
                Volume = record.Contents.Sum(c => c.Volume),
                GrossAndTare = (record.Contents.Sum(c => c.GrossWt) + record.CntrTareWt),
                RecordCommoditiesEn = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.CommodityEngName +
                                                                                    (rc.DocumentRecord.IsIMO ? " IMO: " + rc.DocumentRecord.IMO +
                                                                                                                " UNNO: " + rc.DocumentRecord.UNNO : ""))
                                                                        .Distinct().Order().ToList()),
            };

            eoRecordsDTO.Add(eoRecordDTO);
        }

        return eoRecordsDTO.ToArray();
    };

    Func<IEnumerable<ExportOrderEntity>, bool, IEnumerable<VoyageManifestDTO>> manifestRecords = (exportOrders, _isImo) =>
    {
        var ItemsDTO = new List<VoyageManifestDTO>();

        try
        {
            foreach (var Item in exportOrders)
            {
                var _shippers = Item.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.NameEn!)).ToList();
                var _consignees = Item.Records.SelectMany(eor => eor.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.NameEn!)).ToList();

                var _commodities = new List<string>();
                var _commoditiesEn = new List<string>();

                _commodities = Item.Records.SelectMany(eor => eor.Contents.Select(con => con.DocumentRecord.CommodityName +
                                                                                  (con.DocumentRecord.IsIMO ?
                                                                                     " IMO:" + con.DocumentRecord.IMO +
                                                                                     " UNNO:" + con.DocumentRecord.UNNO : "")))
                                                                          .Distinct().ToList();

                _commoditiesEn = Item.Records.SelectMany(eor => eor.Contents.Select(con => con.DocumentRecord.CommodityEngName +
                                                                                    (con.DocumentRecord.IsIMO ?
                                                                                        " IMO:" + con.DocumentRecord.IMO +
                                                                                        " UNNO:" + con.DocumentRecord.UNNO : "")))
                                                                            .Distinct().ToList();

                if (_isImo)
                {
                    _commodities = Item.Records.SelectMany(eor => eor.Contents.Where(con => con.DocumentRecord.IsIMO == true)
                                                                              .Select(con => con.DocumentRecord.CommodityName +
                                                                                       " IMO:" + con.DocumentRecord.IMO +
                                                                                       " UNNO:" + con.DocumentRecord.UNNO))
                                                                              .Distinct().ToList();

                    _commoditiesEn = Item.Records.SelectMany(eor => eor.Contents.Where(con => con.DocumentRecord.IsIMO == true)
                                                                                .Select(con => con.DocumentRecord.CommodityEngName +
                                                                                         " IMO:" + con.DocumentRecord.IMO +
                                                                                         " UNNO:" + con.DocumentRecord.UNNO))
                                                                                .Distinct().ToList();
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

                    var ItemDTO = new VoyageManifestDTO()
                    {
                        VesselCallId = Item.VesselCallDetail!.VesselCall!.Id,
                        ExpOrderNum = Item.Num,
                        BLNum = Item.Num.IndexOf("_") == -1 ? Item.Num : Item.Num.Substring(0, Item.Num.IndexOf("_")),
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
                        PersonCompany = Item.Person is null ? string.Empty : Item.Person.Company,
                        PersonAddress = Item.Person is null ? string.Empty : Item.Person.Address,
                        PersonPass = Item.Person is null ? string.Empty : Item.Person.Passport,
                        PersonSign = Item.Person is null ? string.Empty :
                                        Item.Person.Name is null ? string.Empty :
                                            Item.Person.Name.Substring(0, 1) + ". " +
                                            (Item.Person.SurName is null ? string.Empty : Item.Person.SurName.Substring(0, 1) + ". ") +
                                            Item.Person.FamilyName,
                        DateExplanation = "\"____\" " +
                                            (Item.VesselCallDetail!.VesselCall!.ETS.HasValue ? 
                                                Item.VesselCallDetail!.VesselCall!.ETS.Value.ToString("MMMM yyyy") : "_____________________") + "г.",
                        Shippers = "S: " + string.Join("; ", _shippers.Order().Distinct()),
                        Consignees = "C: " + string.Join("; ", _consignees.Order().Distinct()),
                        Commodities = string.Join("; ", _commodities.Order().Distinct()),
                        CommoditiesEn = string.Join("; ", _commoditiesEn.Order().Distinct()),

                        /// Cntr Records
                        Seq = ++indexRec,

                        Cntr = record.CntrNum,
                        CntrType = record.CntrType!.Normolize!,
                        Seal = record.Seal != string.Empty ? record.Seal : "N/A",
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

                        RecordCommodities = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.CommodityName +
                                                                                            (rc.DocumentRecord.IsIMO ? " IMO:" +
                                                                                                rc.DocumentRecord.IMO + "UNNO:" +
                                                                                                rc.DocumentRecord.UNNO : ""))
                                                                             .Distinct().ToList()),
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
            string msg = ex.Message;
            return Enumerable.Empty<VoyageManifestDTO>();
        }
    };

    Func<IEnumerable<ExportOrderEntity>, string?, IEnumerable<VoyageExportOrderDTO>> FuncExportOrdersDTO = (exportOrders, _MyCoName) =>
    {
        var ItemsDTO = new List<VoyageExportOrderDTO>();

        try
        {
            foreach (var item in exportOrders)
            {
                uint indexRec = 0;

                foreach (var record in item.Records)
                {
                    /// ExportOrder
                    var itemDTO = new VoyageExportOrderDTO()
                    {
                        VesselCallId = item.VesselCallDetail!.VesselCall.Id,
                        BLNum = item.Num.IndexOf("_") == -1 ? item.Num : item.Num.Substring(0, item.Num.IndexOf("_")),
                        Dated = item.Dated.HasValue ? item.Dated.Value.ToString("dd.MM.yyyy") : DateTime.Today.ToString("dd.MM.yyyy"),
                        xmlDated = item.Dated.HasValue ? item.Dated.Value.ToString("dd.MM.yyyy hh:mm:ss") : DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"),
                        DateOfLoading = item.VesselCallDetail!.VesselCall!.ETA.HasValue ? item.VesselCallDetail!.VesselCall!.ETA!.Value.ToString("dd.MM.yyyy") : string.Empty,
                        BLDate = item.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy"),
                        BLtemplate = item.Carrier!.BlTemplate.ToString(),
                        CustomsOfficeCode = item.VesselCallDetail!.VesselCall.Terminal.Customs!.Code,
                        CustomsOfficeNameShort = item.VesselCallDetail.VesselCall.Terminal.Customs.OfficeShort,
                        CarrierNameEn = item.Carrier!.NameEn,
                        TerminalName = item.VesselCallDetail!.VesselCall.Terminal.Name,
                        VesselName = item.VesselCallDetail!.VesselCall!.Vessel.Name!,
                        VesselFlag = item.VesselCallDetail!.VesselCall!.Vessel.Flag!.RUS,
                        VesselFlagEn = item.VesselCallDetail!.VesselCall!.Vessel.Flag.ENG,
                        Voyage = item.VesselCallDetail!.VesselCall!.VoyageNo,
                        POD = item.VesselCallDetail!.POD!.Name + ", " + item.VesselCallDetail!.POD!.Country!.RUS,
                        PODEn = item.VesselCallDetail!.POD!.NameEn! + ", " + item.VesselCallDetail!.POD!.Country.ENG,
                        PODwithCountryRus = item.VesselCallDetail!.POD!.NameEn! + ", " + item.VesselCallDetail!.POD!.Country.RUS,
                        FinalDestination = item.VesselCallDetail!.FinalDestination is null ? string.Empty :
                                           item.VesselCallDetail!.FinalDestination!.NameEn! + ", " + item.VesselCallDetail!.FinalDestination!.Country!.ENG,
                        POLAgent = item.Carrier!.CarrierDetails.Any(cd => !cd.TerminalName.Equals(item.VesselCallDetail.VesselCall.Terminal.Name)) ? string.Empty :
                                    item.Carrier!.CarrierDetails.FirstOrDefault(cd => cd.TerminalName == item.VesselCallDetail.VesselCall.Terminal.Name)!.AgentPOL!,
                        PODAgent = item.VesselCallDetail!.AgentPOD,
                        Measurement = item.Records.FirstOrDefault()!.Contents.FirstOrDefault()!.Volume > 0 ? "CBM" : "KG",
                        TotalCntrCount = (uint)item.Records.Count,
                        TotalPackages = (uint)item.Records.Sum(r => r.Contents.Sum(c => c.PackageQty))!,
                        TotalGrossWeight = item.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                        TotalNetWeight = item.Records.Sum(r => r.Contents.Sum(c => c.NetWt)),
                        TotalTareWeight = item.Records.Sum(r => r.CntrTareWt),
                        Contract = item.Carrier.CarrierDetails.Any(cd => !cd.TerminalName.Equals(item.VesselCallDetail.VesselCall.Terminal.Name)) ? string.Empty :
                                    item.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == item.VesselCallDetail.VesselCall.Terminal.Name)!.Contract,
                        ContractDate = item.Carrier.CarrierDetails.Any(cd => !cd.TerminalName.Equals(item.VesselCallDetail.VesselCall.Terminal.Name)) ? string.Empty :
                                        item.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == item.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract.HasValue ?
                                        item.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == item.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") :
                                        string.Empty,
                        MyCompanyName = _MyCoName,
                        Person = item.Person!.Name!.Substring(0, 1) + ". " +
                                            item.Person!.SurName!.Substring(0, 1) + ". " +
                                            item.Person!.FamilyName + "  т. " +
                                            item.Person.Phone,
                        PersonXml = item.Person!.Name + " " + item.Person!.FamilyName + " телефон: " + item.Person.Phone,

                        /// ExportOrder_Records
                        Seq = ++indexRec,
                        CntrType = record.CntrType!.Normolize!,
                        CntrTareWt = record.CntrTareWt,
                        Seal = record.Seal is not null ? record.Seal : string.Empty,
                    };

                    /// ExportOrderRecord_Contents
                    foreach (var content in record.Contents)
                    {
                        itemDTO.Id = item.Id;
                        itemDTO.Num = item.Num;
                        itemDTO.Cntr = record.CntrNum;

                        itemDTO.PackageQty = content.PackageQty is not null ? (uint)content.PackageQty : 0;
                        itemDTO.PackageName = content.PackageName is not null ? content.PackageName.ToUpper() : "";
                        itemDTO.NetWt = content.NetWt;
                        itemDTO.GrossWt = content.GrossWt;
                        itemDTO.Volume = content.Volume;

                        itemDTO.DocumentName = content.DocumentRecord.Document.Name!;
                        itemDTO.SeqContent = (uint)content.DocumentRecord.Seq;
                        itemDTO.Shipper = content.DocumentRecord.Document.Shipper!.Name!;
                        itemDTO.ShipperEn = content.DocumentRecord.Document.Shipper!.NameEn!;
                        itemDTO.Consignee = content.DocumentRecord.Document.Consignee!.Name!;
                        itemDTO.ConsigneeEn = content.DocumentRecord.Document.Consignee!.NameEn!;

                        itemDTO.Commodity = content.DocumentRecord.CommodityName;
                        itemDTO.CommodityEn = content.DocumentRecord.CommodityEngName;
                        itemDTO.HSCode = content.DocumentRecord.CommodityHSCode!;
                        itemDTO.IMO = content.DocumentRecord.IMO!;
                        itemDTO.UNNO = content.DocumentRecord.UNNO!;
                        itemDTO.IsIMO = content.DocumentRecord.IsIMO;

                        ItemsDTO.Add(itemDTO);
                        itemDTO = new VoyageExportOrderDTO();
                    }
                }
            }

            return ItemsDTO;
        }
        catch (Exception ex)
        {
            string msg = ex.Message; Console.WriteLine(msg);
            return Enumerable.Empty<VoyageExportOrderDTO>();
        }
    };

    Func<DateTime?, string?> DateToStr = (date) =>
    {
        if (date is not null)
            return date.Value.ToString("dd") + "/" + date.Value.ToString("MM") + "/" + date.Value.ToString("yyyy");
        else
            return null;
    };

    #endregion
}
