using ExportOrderEntites.MyCompany;
using Microsoft.EntityFrameworkCore;
using System;


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

            try { 
            appObjResponse.Object = await db.ExportOrders.Include(eo => eo.Person)
                                                         .Include(eo => eo.Carrier)!.ThenInclude(c => c.CarrierDetails)
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
            //try
            //{
            var db = await _db;

            var myCompany = await db.MyCompany.AsNoTracking().FirstOrDefaultAsync();

            var Item = await db.ExportOrders.Include(x => x.VesselCall).ThenInclude(vc => vc!.LoadingTerminal)
                                            .Include(x => x.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCall).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod.Country)
                                            .Include(x => x.Carrier)!.ThenInclude(c => c.CarrierDetails)
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
                                                VesselName = source.VesselCall!.Vessel.Name + " (" + source.VesselCall.Vessel.Flag.RUS + ")",
                                                Voyage = source.VesselCall.VoyageCarrier,
                                                DateOfLoading = source.VesselCall.ETA!.Value.ToString("dd.MM.yyyy"),
                                                POD = source.VesselCall.POD.Name + ", " + source.VesselCall.POD.Country.RUS,
                                                Contract = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name) != null ? source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name).Contract : null,
                                                ContractDate = source.Carrier.CarrierDetails.FirstOrDefault(s => s.TerminalName == source.VesselCall.LoadingTerminal.Name)!.DateContract!.Value.ToString("dd.MM.yyyy"),
                                                MyCompanyName = myCompany!.Name!,
                                                Person = source.Person!.Name + " т. " + source.Person.Phone,
                                                exportOrderRecordsDTO = eoRecords(source.Records!)
                                            })
                                            .FirstOrDefaultAsync(x => x.Id == eoId);

            return Item!;
            //}
            //catch(Exception ex)
            //{
            //    string msg = ex.Message;
            //    return new ExportOrderDTO();
            //}
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

            var items = await db.ExportOrders.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.ExportOrderNum))
                    items = items.Where(s => s.Num == filter.ExportOrderNum).ToList();

                if (!string.IsNullOrEmpty(filter.CntrNum))
                    items = items.Where(s => s.Records!.FirstOrDefault()!.CntrNum == filter.CntrNum).ToList();

                if (!string.IsNullOrEmpty(filter.Voyage))
                    items = items.Where(s => s.VesselCall!.VoyageCarrier == filter.Voyage).ToList();

                if (!string.IsNullOrEmpty(filter.VesselName))
                    items = items.Where(s => s.VesselCall!.Vessel.Name == filter.VesselName).ToList();

                if (!string.IsNullOrEmpty(filter.Carrier))
                    items = items.Where(s => s.Carrier.Name == filter.Carrier).ToList();

                appObjResponse.Object = items.ToArray();
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

                db.Entry(item).State = EntityState.Added;
                db.Entry(item.Carrier).State = EntityState.Unchanged;
                db.Entry(item.VesselCall!).State = EntityState.Unchanged;

                // Documents
                foreach (var document in item.Documents!)
                    db.Entry(document).State = EntityState.Unchanged;

                // Records
                foreach (var record in item.Records!)
                {
                    db.Entry(record).State = EntityState.Added;
                    db.Entry(record.CntrType).State = EntityState.Detached;

                    foreach (var recordContent in record.Contents)
                        db.Entry(recordContent).State = EntityState.Added;
                }

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return appObjResponse;
            }
        }

        return appObjResponse;
    }

    public Task<AppObjectResponse> RemoveItemAsync(ExportOrderEntity item)
    {
        throw new NotImplementedException();
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
            //eoRecordsDTO.Add(new ExportOrderRecordDTO({}));
            ++indexRec;
            eoRecordDTO.Seq = indexRec.ToString();

            foreach (var content in record.Contents)    {
                //eoRecordDTO.Seq = "";
                eoRecordDTO.Cntr = record.CntrNum;
                eoRecordDTO.CntrType = record.CntrType.Normolize!;
                eoRecordDTO.CntrTareWt = record.CntrTareWt;
                eoRecordDTO.Seal = record.Seal;


                eoRecordDTO.Quantity = content.Quantity;
                eoRecordDTO.NetWt = content.NetWt;
                eoRecordDTO.GrossWt = content.GrossWt;

                eoRecordDTO.DocumentName = content.DocumentRecord.Document.Name!;
                eoRecordDTO.Shipper = content.DocumentRecord.Document.Shipper!.Name!;
                eoRecordDTO.Consignee = content.DocumentRecord.Document.Consignee!.Name!;

                eoRecordDTO.CommodityName = content.DocumentRecord.CommodityName;
                eoRecordDTO.HSCode = content.DocumentRecord.CommodityHSCode;
                eoRecordDTO.IMO = content.DocumentRecord.IMO!;
                eoRecordDTO.UNNO = content.DocumentRecord.UNNO!;
                eoRecordDTO.IsIMO = content.DocumentRecord.IsIMO;

                eoRecordsDTO.Add(eoRecordDTO);
                eoRecordDTO = new ExportOrderRecordDTO();
            }
        }

        return eoRecordsDTO.ToArray();
    };


    //public async Task<MyCompanyEntity> GetMyCompanyAsync()
    //{
    //    using (var _db = _dbContext.CreateDbContextAsync())
    //    {
    //        var db = await _db;

    //        var Item = db.MyCompany.Include(x => x.Persons).AsNoTracking().FirstOrDefault();

    //        return Item!;
    //    }
    //}

    #endregion

}
