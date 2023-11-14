using Microsoft.EntityFrameworkCore;
using Microsoft.Office.Interop.Excel;
using System.Linq;
using static MudBlazor.CategoryTypes;

namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public class VesselCallProvider : IVesselCallProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public VesselCallProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.VesselCalls
                                                        .Include(vc => vc.Vessel)
                                                        .Include(vc => vc.Terminal)
                                                        .Include(vc => vc.Details).ThenInclude(vcd => vcd.POD)
                                                        .AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            
            if (appObjResponse.Object is null)
                appObjResponse.ErrorAdd($"There is no voyage you choose.");
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemAsync(string voyage)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            try
            {            
                var db = await _db;

                if (voyage.Contains('&'))
                {
                    var vesselName = voyage.Split('&')[0];
                    var voyageNo = voyage.Split('&')[1];

                    appObjResponse.Object = await db.VesselCalls
                                                                .Include(vc => vc.Vessel)
                                                                .Include(vc => vc.Terminal)
                                                                .Include(vc => vc.Details).ThenInclude(d => d.POD)
                                                                .Where(vc => vc.Vessel.Name!.ToUpper() == vesselName.ToUpper() &&
                                                                             vc.VoyageNo.ToUpper() == voyageNo.ToUpper()
                                                                 )
                                                                .FirstOrDefaultAsync();

                    if (appObjResponse.Object is null)
                        appObjResponse.ErrorAdd($"There is no voyage: {vesselName} / {voyageNo}");
                }
                else
                    appObjResponse.ErrorAdd("Wrong request");

                return appObjResponse;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(msg);
                return appObjResponse;
            }
        }
    }

    public async Task<AppObjectResponse> GetVesselCallDetailItemAsync(long vesselCallid)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            try
            {
                var db = await _db;

                appObjResponse.Object = await db.Set<VesselCallDetail>().AsNoTracking()
                                                    .Include(vcd => vcd.POD)
                                                    .Include(vcd => vcd.VesselCall.Terminal)
                                                    .Include(vcd => vcd.VesselCall).ThenInclude(vc => vc.Vessel)
                                                    .FirstOrDefaultAsync(vcd => vcd.Id == vesselCallid);

                if (appObjResponse.Object is null)
                    appObjResponse.ErrorAdd("There is no voyage");

                return appObjResponse;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                appObjResponse.ErrorAdd(msg);
                return appObjResponse;
            }
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.VesselCalls.ToListAsync();
            return appObjResponse;
        }
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

                var vesselCalls = await db.VesselCalls
                                                    .Include(vc => vc.Vessel)
                                                    .Include(vc => vc.Terminal)
                                                    .Include(vc => vc.Details).ThenInclude(d => d.POD)
                                                    //.Include(vc => vc.Details).ThenInclude(vcd => vcd.ExportOrders).ThenInclude(eo => eo.Carrier)
                                                    .Where(vc => filter.ETA.HasValue ? vc.ETA!.Value >= filter.ETA.Value : true)
                                                    .Where(vc => filter.ETS.HasValue ? vc.ETS!.Value >= filter.ETS.Value : true)
                                                    .AsSplitQuery()
                                                    .ToListAsync();
                
                var vesselCallsDTO = vcRecords(vesselCalls);

                if (!string.IsNullOrEmpty(filter.Vessel))
                    vesselCallsDTO = vesselCallsDTO.Where(s => s.VesselName == filter.Vessel).ToList();

                if (!string.IsNullOrEmpty(filter.Voyage))
                    vesselCallsDTO = vesselCallsDTO.Where(s => s.VoyageNo == filter.Voyage).ToList();

                if (!string.IsNullOrEmpty(filter.Terminal))
                    vesselCallsDTO = vesselCallsDTO.Where(s => s.Terminal == filter.Terminal).ToList();

                if (!string.IsNullOrEmpty(filter.POD))
                    vesselCallsDTO = vesselCallsDTO.Where(s => s.POD == filter.POD).ToList();

                //if (!string.IsNullOrEmpty(filter.Carrier))
                //    vesselCallsDTO = vesselCallsDTO.Where(s => s.CarrierName == filter.Carrier).ToList();

                appObjResponse.Object = vesselCallsDTO.ToArray();
            }            

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(VesselCallEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.VesselCalls
                                                    .Include(vc => vc.Vessel)
                                                    .Include(vc => vc.Terminal)
                                                    .Include(vc => vc.Details).ThenInclude(d => d.POD)
                                                    .FirstOrDefaultAsync(s => s.Id == item.Id);

                string modifyVoyage = $"{modifyItem!.Vessel?.Name}&{modifyItem.VoyageNo}";
                string itemVoyage = $"{item!.Vessel?.Name}&{item.VoyageNo}";

                if (modifyVoyage != itemVoyage)
                {
                    var itemExistCheck = await db.VesselCalls.Where(s => s.Vessel!.Name!.ToUpper() == item.Vessel!.Name!.ToUpper())
                                                            .Where(s => s.VoyageNo.ToUpper() == item.VoyageNo.ToUpper())
                                                            .FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($" {itemVoyage} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.VoyageNo = item.VoyageNo;
                modifyItem!.VoyageNoTerminal = item.VoyageNoTerminal;
                modifyItem!.ETA = item.ETA;
                modifyItem!.ETS = item.ETS;

                if (!modifyItem.Vessel!.Id.Equals(item.Vessel!.Id))
                    modifyItem!.Vessel = item.Vessel;
                if (!modifyItem.Terminal!.Id.Equals(item.Terminal!.Id))
                    modifyItem!.Terminal = item.Terminal;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                // compaire new item with existed
                foreach (var modifyDetail in modifyItem.Details!)
                    if (!item.Details!.Any(s => s.Id == modifyDetail.Id))
                        modifyItem.Details!.Remove(modifyDetail);
                    else
                    {
                        modifyDetail.CreateUser = User!;
                        modifyDetail.CreateTime = DateTime.Now;
                        modifyDetail.AgentPOD = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.AgentPOD;

                        if (!modifyDetail.POD!.Id.Equals(item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.POD!.Id))
                            modifyDetail.POD = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.POD;

                        if (!modifyDetail.FinalDestination!.Id.Equals(item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.FinalDestination!.Id))
                            modifyDetail.FinalDestination = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.FinalDestination;
                    }

                // compaire existed item with new
                foreach (var itemDetail in item.Details!)
                    if (!modifyItem.Details.Any(s => s.Id == itemDetail.Id))
                    {
                        itemDetail.CreateUser = User!;
                        itemDetail.CreateTime = DateTime.Now;                        
                        
                        db.Entry(itemDetail.POD!).State = EntityState.Unchanged;
                        db.Entry(itemDetail.FinalDestination!).State = EntityState.Unchanged;
                        db.Entry(itemDetail.CreateUser).State = EntityState.Unchanged;

                        db.Entry(itemDetail).State = EntityState.Added;
                        modifyItem.Details.Add(itemDetail);
                    }

                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                appObjResponse.ErrorAdd(ex.Message);

                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> NewItemAsync(VesselCallEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            try
            {
            
                var db = await _db;
                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                // check an Existing item
                //var itemExistCheck = await db.VesselCalls
                //                                        .Where(s => s.Vessel.Name == item.Vessel.Name)
                //                                        .Where(s => s.VoyageNo == item.VoyageNo).FirstOrDefaultAsync();

                var itemExistCheck = await db.VesselCalls.Where(s => s.Vessel.Name == item.Vessel.Name && s.VoyageNo == item.VoyageNo).FirstOrDefaultAsync();

                if (itemExistCheck != null)
                {
                    appObjResponse.ErrorAdd($"Voyage: {item.VoyageNo} for vessel: {item.Vessel.Name} is exists already.");
                    return appObjResponse;
                }

                item.CreateUser = User!;

                foreach (var detail in item.Details)
                {
                    detail.CreateUser = User!;
                    detail.CreateTime = DateTime.Now;

                    db.Entry(detail.POD!).State = EntityState.Unchanged;
                    db.Entry(detail.FinalDestination!).State = EntityState.Unchanged;

                    db.Entry(detail).State = EntityState.Added;                
                }

                db.Entry(item.Vessel).State = EntityState.Unchanged;
                db.Entry(item.Terminal).State = EntityState.Unchanged;

                db.Entry(item).State = EntityState.Added;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                return appObjResponse;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return appObjResponse;
            }
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

                // check an Existing item
                var existedItem = await db.VesselCalls.Where(s => s.Id == id).FirstOrDefaultAsync();

                if (existedItem is null)
                {
                    appObjResponse.ErrorAdd("Record wasn't deleted");
                    return appObjResponse;
                }

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

    public async Task<AppObjectResponse> RemoveVesselCallDetailDTOAsync(long id)
    {
        appObjResponse = new();

        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                // check an Existing item
                //var existedItem = db.Set<VesselCallDetail>().Where(vcd => vcd.Id == id).FirstOrDefaultAsync();
                var existedItem = db.VesselCalls.Include(vc => vc.Details).Where(vc => vc.Details.Any(vcd => vcd.Id == id)).FirstOrDefaultAsync();

                if (existedItem is null)
                {
                    appObjResponse.ErrorAdd("Record wasn't deleted");
                    return appObjResponse;
                }

                db.Entry(existedItem).State = EntityState.Deleted;
                //db.Entry(db.Set<VesselCallDetail>().Where(vcd => vcd.Id == id)).State = EntityState.Deleted;

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

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Vessels.Select(s => s.Name!).ToListAsync();
        }
    }

    public Task<IEnumerable<string>> GetNamesEn()
    {
        throw new NotImplementedException();
    }



    #region AUXILARY METHODS

    public async Task<IEnumerable<string>> GetPODs()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Locations.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetVoyages()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.VesselCalls.Select(s => s.VoyageNo!).ToListAsync();
        }
    }

    Func<IEnumerable<VesselCallEntity>, IEnumerable<VesselCallDetailDTO>> vcRecords = (Records) =>
    {
        var RecordsDTO = new List<VesselCallDetailDTO>();

        foreach (var record in Records)
        {
            foreach (var detail in record.Details)                
            {
                var recordDTO = new VesselCallDetailDTO()
                {
                    Id = detail.Id,
                    VesselCallId = detail.VesselCall.Id,
                    VesselName = record.Vessel.Name,
                    VoyageNo = record.VoyageNo,
                    VoyageNoTerminal = record.VoyageNoTerminal,
                    Terminal = record.Terminal.Name,
                    ETA = record.ETA,
                    ETS = record.ETS,
                    POD = detail.POD!.NameEn,
                    AgentPOD = detail.AgentPOD,
                    Status = detail.Status,
                };

                RecordsDTO.Add(recordDTO);
            };
        }        
        
        return RecordsDTO;
    };

    #endregion
}
