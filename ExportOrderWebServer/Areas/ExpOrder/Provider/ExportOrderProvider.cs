
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

            var myCompany = await db.MyCompany.OrderBy(c => c.Id).LastOrDefaultAsync();

            //var vesselName = Voyage.Split('&')[0];
            //var voyageNo = Voyage.Split('&')[1];
            //var pod = Voyage.Split('&')[2];

            var Item = await db.ExportOrders.Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc.Terminal).ThenInclude(t => t.Customs)
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
                                                Dated = source.Dated.HasValue ? source.Dated.Value.ToString("dd.MM.yyyy") : DateTime.Today.ToString("dd.MM.yyyy"),
                                                xmlDated = source.Dated != null ? source.Dated.Value.ToString("dd.MM.yyyy hh:mm:ss") : DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss"),
                                                BLtemplate = source.Carrier!.BlTemplate.ToString(),
                                                CustomsOfficeCode = source.VesselCallDetail!.VesselCall.Terminal.Customs!.Code,
                                                CustomsOfficeNameShort = source.VesselCallDetail.VesselCall.Terminal.Customs.OfficeShort,
                                                CarrierNameEn = source.Carrier!.NameEn,
                                                TerminalName = source.VesselCallDetail!.VesselCall.Terminal.Name,
                                                VesselName = source.VesselCallDetail!.VesselCall!.Vessel.Name!,
                                                VesselFlag = source.VesselCallDetail!.VesselCall!.Vessel.Flag!.RUS,
                                                VesselFlagEn = source.VesselCallDetail!.VesselCall!.Vessel.Flag.ENG,
                                                Voyage = source.VesselCallDetail!.VesselCall!.VoyageNo,
                                                DateOfLoading = source.VesselCallDetail!.VesselCall!.ETA!.Value.ToString("dd.MM.yyyy"),
                                                BLDate = source.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy"),
                                                POD = source.VesselCallDetail!.POD!.Name + ", " + source.VesselCallDetail!.POD!.Country!.RUS,
                                                PODEn = source.VesselCallDetail!.POD!.NameEn! + ", " + source.VesselCallDetail!.POD!.Country.ENG,
                                                PODwithCountryRus = source.VesselCallDetail!.POD!.NameEn! + ", " + source.VesselCallDetail!.POD!.Country.RUS,
                                                FinalDestination = source.VesselCallDetail!.FinalDestination!.NameEn! + ", " + source.VesselCallDetail!.FinalDestination!.Country!.ENG,
                                                PODAgent = source.VesselCallDetail!.AgentPOD,
                                                Measurement = source.Records.FirstOrDefault()!.Contents.FirstOrDefault()!.Volume > 0 ? "CBM" : "KG",
                                                TotalCntrCount = source.Records.Count,
                                                TotalPackages = source.Records.Sum(r => r.Contents.Sum(c => c.PackageQty)),
                                                TotalGrossWeight = source.Records.Sum(r => r.Contents.Sum(c => c.GrossWt)),
                                                TotalNetWeight = source.Records.Sum(r => r.Contents.Sum(c => c.NetWt)),
                                                TotalTareWeight = source.Records.Sum(r => r.CntrTareWt),
                                                //Contract = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name).Contract != null ?
                                                //          source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name).Contract : null,
                                                Contract = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.Contract,
                                                //ContractDate = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name)!.DateContract != null ?
                                                //          source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy") : "",
                                                ContractDate = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy"),
                                                MyCompanyName = myCompany!.Name,
                                                Person = source.Person!.Name!.Substring(0, 1) + ". " + 
                                                        source.Person!.SurName!.Substring(0, 1) + ". " + 
                                                        source.Person!.FamilyName + "  т. " + 
                                                        source.Person.Phone,
                                                PersonXml = source.Person!.Name + " " + source.Person!.FamilyName + " телефон: " + source.Person.Phone,
                                                exportOrderRecordsDTO = eoRecords(source.Records!)
                                            })
                                            .FirstOrDefaultAsync(x => x.Id == Id);

            return Item!;
        }
    }
    
    public async Task<IEnumerable<ManifestDTO>> GetManifestAsync(long id)
    {
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var Items = await db.ExportOrders
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Terminal).ThenInclude(ter => ter.Customs)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vcd => vcd!.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCallDetail).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod!.Country)
                                            .Include(x => x.Person)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c!.Location).ThenInclude(lo => lo!.Country)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c!.CarrierDetails)
                                            .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                            .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)                                            
                                            .AsNoTracking()
                                            .Where(x => x.VesselCallDetail!.VesselCall!.Id == id)
                                            .AsSplitQuery()
                                            .ToListAsync();

                var DTOItems = mRecords(Items);

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
                                                        .Where(eo => filter.Dated.HasValue ? eo.Dated == filter.Dated : true)
                                                        .ToListAsync();

                var ItemsDTO = eoComponentRecord(exportOrders);

                //if (!string.IsNullOrEmpty(filter.Num))
                //    ItemsDTO = ItemsDTO.Where(s => s.Cntr == filter.CntrNum).ToList();

                if (!string.IsNullOrEmpty(filter.Num))
                    ItemsDTO = ItemsDTO.Where(s => s.Num == filter.Num).ToList();

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
                                                    .Include(eo => eo.Documents).ThenInclude(d => d.Records)
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
                        appObjResponse.ErrorAdd($"Export Order No. {item.Num} dtd {item.Dated!.Value.ToShortDateString} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                // DELETE AN EXISTED modifyItem

                foreach (var modifyRecord in modifyItem.Records)
                    db.Entry(modifyRecord).State = EntityState.Deleted;

                modifyItem.Documents.Clear();

                var dbBug = db.ChangeTracker.DebugView.LongView;
                await db.SaveChangesAsync();

                db.ChangeTracker.Clear();

                // RE-WRITE WITH A NEW ITEM

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

                    db.Entry(itemDocument).State = EntityState.Unchanged;
                    modifyItem.Documents.Add(itemDocument);

                    foreach (var record in itemDocument.Records)
                        db.Entry(record).State = EntityState.Unchanged;
                }

                // EXPORT ORDER RECORDS
                foreach (var itemRecord in item.Records)
                {
                    db.Entry(itemRecord).State = EntityState.Added;
                    modifyItem.Records.Add(itemRecord);

                    foreach (var itemRecordContent in itemRecord.Contents)
                        db.Entry(itemRecordContent).State = EntityState.Added;
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


    #region AUXIALARY

    Func<IEnumerable<ExportOrderRecord>, IEnumerable<ExportOrderRecordDTO>> eoRecords = (_eoRecords) =>
    {
        var eoRecordsDTO = new List<ExportOrderRecordDTO>();
        var eoRecordDTO = new ExportOrderRecordDTO();

        uint indexRec = 0;

        foreach (var record in _eoRecords)
        {
            eoRecordDTO.Seq = ++indexRec;
            eoRecordDTO.CntrTareWt = record.CntrTareWt;
            eoRecordDTO.Seal = record.Seal is not null ? record.Seal : string.Empty;
            eoRecordDTO.CntrType = record.CntrType!.Normolize!;

            foreach (var content in record.Contents)
            {
                eoRecordDTO.Cntr = record.CntrNum;

                eoRecordDTO.PackageQty = content.PackageQty is not null ? (uint)content.PackageQty : 0;
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

    Func<IEnumerable<ExportOrderEntity>, IEnumerable<ManifestDTO>> mRecords = (exportOrders) =>
    {
        var ItemsDTO = new List<ManifestDTO>();        

        foreach (var Item in exportOrders)
        {
            uint indexRec = 0;

            foreach (var record in Item.Records)
            {
                var itemDTO = new ManifestDTO()
                {
                    VesselCallId = Item.VesselCallDetail!.VesselCall!.Id,

                    Seq = ++indexRec,
                    BLNum = Item.Num,
                    BLDate = Item.VesselCallDetail!.VesselCall!.ETS.HasValue ? Item.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("dd.MM.yyyy") : "---",
                    Voyage = Item.VesselCallDetail!.VesselCall!.VoyageNo,
                    VesselName = Item.VesselCallDetail!.VesselCall!.Vessel.Name!,
                    VesselFlag = Item.VesselCallDetail!.VesselCall!.Vessel.Flag!.RUS,
                    VesselFlagEn = Item.VesselCallDetail!.VesselCall!.Vessel.Flag!.ENG,
                    CaptainFamily = Item.VesselCallDetail.VesselCall.Vessel.CaptainFamily,
                    CaptainName = Item.VesselCallDetail.VesselCall.Vessel.CaptainName,
                    PODEn = Item.VesselCallDetail!.POD!.NameEn,
                    PODnCountryEn = Item.VesselCallDetail!.POD!.NameEn + ", " + Item.VesselCallDetail!.POD.Country!.ENG,
                    PODunlocode = Item.VesselCallDetail!.POD.UnLocode,

                    CarrierNameEn = Item.Carrier!.NameEn,
                    CarrierLocation = Item.Carrier.Location!.Name,
                    CarrierCountryEn = Item.Carrier.Location!.Country!.ENG,
                    CarrierContract = Item.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == Item.VesselCallDetail.VesselCall.Terminal.Name)!.Contract,
                    CarrierContractDate = Item.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == Item.VesselCallDetail.VesselCall.Terminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy"),

                    Cntr = record.CntrNum,
                    CntrType = record.CntrType!.Normolize!,
                    Seal = record.Seal != string.Empty ? record.Seal : "N/A",
                    CntrTareWt = record.CntrTareWt,
                    CntrTotalWeight = record.CntrTareWt + (double)record.Contents.Sum(c => c.GrossWt)!,

                    PackageQtys = (uint)record.Contents.Sum(c => c.PackageQty)!,
                    PackageNames = string.Join(", ", record.Contents.Select(rc => rc.PackageName is not null ? rc.PackageName.ToUpper() : "").Distinct().Order()),
                    NetWeights = record.Contents.Sum(c => c.NetWt),
                    GrossWeights = record.Contents.Sum(c => c.GrossWt),
                    Volumes = record.Contents.Sum(c => c.Volume),
                    IMO = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.IMO).Distinct()),
                    UNNO = string.Join("; ", record.Contents.Select(rc => rc.DocumentRecord.UNNO).Distinct()),

                    Commodities = string.Join("; ", record.Contents.Select(rc =>
                                                                    (
                                                                      rc.DocumentRecord.CommodityName + " " + (rc.DocumentRecord.IsIMO ? "IMO:" : "") +
                                                                      rc.DocumentRecord.IMO + " " + (rc.DocumentRecord.IsIMO ? "UNNO:" : "") +
                                                                      rc.DocumentRecord.UNNO
                                                                    ).
                                                                    Trim()).Distinct().Order()),

                    CommoditiesEn = string.Join("; ", record.Contents.Select(rc =>
                                                                     (
                                                                        rc.DocumentRecord.CommodityEngName + " " + (rc.DocumentRecord.IsIMO ? "IMO:" : "") +
                                                                        rc.DocumentRecord.IMO + " " + (rc.DocumentRecord.IsIMO ? "UNNO:" : "") +
                                                                        rc.DocumentRecord.UNNO
                                                                      )
                                                                      .Trim()).Distinct().Order()),                    

                    Shippers = "S: " + string.Join(", ", record.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.NameEn).Distinct()),
                    Consignees = "C: " + string.Join(", ", record.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.NameEn).Distinct()),
                    ShippersCountries = string.Join(", ", record.Contents.Select(rc => rc.DocumentRecord.Document.Shipper!.CountryENG).Distinct()),
                    ConsigneesCountries = string.Join(", ", record.Contents.Select(rc => rc.DocumentRecord.Document.Consignee!.CountryENG).Distinct()),

                    CustomsOfficeCode = Item.VesselCallDetail!.VesselCall.Terminal.Customs!.Code,
                    CustomsOfficeName = Item.VesselCallDetail!.VesselCall.Terminal.Customs!.Office,
                    CustomsOfficeShortName = Item.VesselCallDetail!.VesselCall.Terminal.Customs!.OfficeShort,
                    CustomsDapartment = Item.VesselCallDetail!.VesselCall.Terminal.Customs!.Dapartment,

                    PersonFamily = Item.Person!.FamilyName,
                    PersonSurName = Item.Person!.Name! + " " + Item.Person!.SurName,
                    PersonBirthYear = Item.Person!.BirthYear,
                    PersonBirthPlace = Item.Person!.BirthPlace,
                    PersonCompany = Item.Person!.Company,
                    PersonAddress = Item.Person!.Address,
                    PersonPass = Item.Person!.Passport,
                    PersonSign = Item.Person!.Name!.Substring(0, 1) + ". " + Item.Person!.SurName!.Substring(0, 1) + ". " + Item.Person!.FamilyName,

                    DateExplanation = "\"____\" " + Item.VesselCallDetail!.VesselCall!.ETS!.Value.ToString("MMMM yyyy") + "г.",
                };
                
                ItemsDTO.Add(itemDTO);
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
                Dated = record.Dated != null ? record.Dated!.Value.ToShortDateString() : "---",
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
