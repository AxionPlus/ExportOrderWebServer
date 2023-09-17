using ExportOrderEntites.DTO;
using ExportOrderEntites.ExportOrder;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static MudBlazor.Colors;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            appObjResponse.Object = await db.ExportOrders.AsNoTracking().Include(s => s.Records)!.ThenInclude(c => c.Contents).ThenInclude(dr => dr.DocumentRecord).ThenInclude(d => d.Document)
                                                                        .Include(x => x.Documents)!.ThenInclude(dr => dr.Records)
                                                                        .FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<ExportOrderDTO> GetItemDTOAsync(long id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            //try
            //{
            var db = await _db;

            var Item = await db.ExportOrders.Include(x => x.VesselCall).ThenInclude(vc => vc!.Vessel).ThenInclude(vsl => vsl.Flag)
                                            .Include(x => x.VesselCall).ThenInclude(vc => vc!.POD).ThenInclude(pod => pod.Country)
                                            //.Include(x => x.Documents)!.ThenInclude(x => x.Shipper as DocumentCustomer)
                                            //.Include(x => x.Documents)!.ThenInclude(d => d.Consignee)
                                            .Include(x => x.Documents)!.ThenInclude(d => d.Records) //.ThenInclude(dr => dr.Document)
                                            .Include(x => x.Records)!.ThenInclude(r => r.CntrType)
                                            //.Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document).ThenInclude(d => d.Shipper)
                                            //.Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document).ThenInclude(d => d.Consignee)
                                            .Include(x => x.Records)!.ThenInclude(r => r.Contents).ThenInclude(co => co.DocumentRecord).ThenInclude(dr => dr.Document)
                                            .AsNoTracking()
                                            .Select(source => new ExportOrderDTO()
                                            {
                                                Id = source.Id,
                                                Num = source.Num,
                                                Dated = source.Dated,
                                                VesselName = source.VesselCall!.Vessel.Name + " (" + source.VesselCall.Vessel.Flag.RUS + ")",
                                                Voyage = source.VesselCall.VoyageCarrier,
                                                DateOfLoading = source.VesselCall.ETA,
                                                POD = source.VesselCall.POD.Name + ", " + source.VesselCall.POD.Name,
                                                //_Documents = eoDocuments(source.Documents!),
                                                exportOrderRecordsDTO = eoRecords(source.Records!)
                                            })
                                            .FirstOrDefaultAsync(x => x.Id == id);
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
                    items = items.Where(s => s.Carrier.FullName == filter.Carrier).ToList();

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

#region AUXIALARY

    Func<IEnumerable<ExportOrderRecord>, IEnumerable<ExportOrderRecordDTO>> eoRecords = (_eoRecords) =>
    {
        var eoRecordsDTO = new List<ExportOrderRecordDTO>();

        var eoDocumentsDTO = new List<DocumentDTO>();

        uint indexRec = 0;
        //uint indexDoc = 0;

        foreach (var record in _eoRecords)
        {
            foreach (var content in record.Contents)
            {
                eoRecordsDTO.Add(new ExportOrderRecordDTO()
                {
                    Seq = ++indexRec,
                    Cntr = record.CntrNum,
                    CntrType = record.CntrType.Normolize!,
                    CntrTareWt = record.CntrTareWt,
                    Seal = record.Seal,

                    Quantity = content.Quantity,
                    NetWt = content.NetWt,
                    GrossWt = content.GrossWt,

                    DocumentName = content.DocumentRecord.Document.Name!,
                    Shipper = content.DocumentRecord.Document.Shipper!.Name!,
                    Consignee = content.DocumentRecord.Document.Consignee!.Name!,

                    CommodityName = content.DocumentRecord.CommodityName!,
                    HSCode = content.DocumentRecord.CommodityHSCode!,
                    IMO = content.DocumentRecord.IMO,
                    UNNO = content.DocumentRecord.UNNO,
                    IsIMO = content.DocumentRecord.IsIMO,
                });
            }
        }

        return eoRecordsDTO.ToArray();
    };

    Func<IEnumerable<DocumentEntity>, IEnumerable<DocumentDTO>> eoDocuments = (_eoDocuments) =>
    {
        var eoDocumentsDTO = new List<DocumentDTO>();

        uint index = 0;

        foreach (var document in _eoDocuments)
        {
            foreach (var record in document.Records)
            {
                eoDocumentsDTO.Add(new DocumentDTO()
                {
                    IndexDocument = index++,
                    DocumentName = document.Name,
                    Shipper = document.Shipper!.Name,
                    Consignee = document.Consignee!.Name,

                    CommodityName = record.CommodityName,
                    CommodityHSCode = record.CommodityHSCode,
                    //IMO = record.IMO,
                    //UNNO = record.UNNO,
                    //IsIMO = record.IsIMO,
                    Quantity = 1,
                });
            }
        }

        return eoDocumentsDTO.ToArray();
    };


    #region MULTI-LEVEL RECORDS

    //Func<IEnumerable<ExportOrderRecord>, IEnumerable<ExportOrderRecordDTO>> expOrderRecords = (_eoRecords) =>
    //{
    //    var eoRecordsDTO = new List<ExportOrderRecordDTO>();

    //    var containerContentsDTO = new List<ContainerContentDTO>();

    //    uint index = 0;

    //    foreach (var record in _eoRecords)
    //    {
    //        foreach (var content in record.Contents)
    //        {
    //            containerContentsDTO.Add(new ContainerContentDTO()
    //            {
    //                Quantity = content.Quantity,
    //                NetWt = content.NetWt,
    //                GrossWt = content.GrossWt,
    //                CommodityName = content.DocumentRecord.CommodityName,
    //                HSCode = content.DocumentRecord.CommodityHSCode,
    //                //IMO = content.DocumentRecord.IMO,
    //                //UNNO = content.DocumentRecord.UNNO,
    //                //IsIMO = content.DocumentRecord.IsIMO,
    //            });
    //        }

    //        eoRecordsDTO.Add(new ExportOrderRecordDTO()
    //        {
    //            IndexExpRecord = index++,
    //            Cntr = record.CntrNum,
    //            CntrType = record.CntrType.Normolize!,
    //            CntrTareWt = record.CntrTareWt,
    //            Seal = record.Seal,
    //            //ContentsDTO = containerContentsDTO
    //        });            
    //    }

    //    return eoRecordsDTO.ToArray();
    //};

    #endregion

    #endregion

}
