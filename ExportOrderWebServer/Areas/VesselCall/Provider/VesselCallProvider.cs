
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
                                                        .Include(vc => vc.Details).ThenInclude(vcd => vcd.FinalDestination)
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
                
                var vesselCallDetailsDTO = vcRecords(vesselCalls);

                if (!string.IsNullOrEmpty(filter.Vessel))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.VesselName == filter.Vessel).ToList();

                if (!string.IsNullOrEmpty(filter.Voyage))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.VoyageNo == filter.Voyage).ToList();

                if (!string.IsNullOrEmpty(filter.Terminal))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.Terminal == filter.Terminal).ToList();

                if (!string.IsNullOrEmpty(filter.POD))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.POD == filter.POD).ToList();

                if (filter.Status >= 0)
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.Status == filter.Status).ToList();
                else
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.Status == EntityStatus.New || s.Status == EntityStatus.Pending).ToList();

                //if (!string.IsNullOrEmpty(filter.Carrier))
                //    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.CarrierName == filter.Carrier).ToList();

                vesselCallDetailsDTO = vesselCallDetailsDTO.OrderByDescending(s => s.CreateTime);

                if (!string.IsNullOrEmpty(filter.Voyage))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.OrderBy(s => s.CreateTime);

                appObjResponse.Object = vesselCallDetailsDTO.ToArray();
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
                                                    .Include(vc => vc.Details).ThenInclude(d => d.FinalDestination)
                                                    .AsNoTracking()
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

                // DELETE AN EXISTED modifyItemRecords
                //foreach (var modifyRecord in modifyItem.Details)
                //{
                //    //db.Entry(modifyRecord.ExportOrders).State = EntityState.Unchanged;
                //    db.Entry(modifyRecord).State = EntityState.Deleted;
                //}                    

                //var delBug = db.ChangeTracker.DebugView.LongView;
                //await db.SaveChangesAsync();

                //db.ChangeTracker.Clear();



                // RE-WRITE WITH A NEW ITEM

                modifyItem!.CreateUser = User!;
                modifyItem!.CreateTime = DateTime.Now;
                modifyItem!.VoyageNo = item.VoyageNo;
                modifyItem!.VoyageNoTerminal = item.VoyageNoTerminal;
                modifyItem!.ETA = item.ETA;
                modifyItem!.ETS = item.ETS;

                if (item.Vessel is not null)
                {
                    modifyItem!.Vessel = item.Vessel;
                    db.Entry(modifyItem.Vessel!).State = EntityState.Unchanged;
                }
                    
                if (item.Terminal is not null)
                {
                    modifyItem!.Terminal = item.Terminal;
                    db.Entry(modifyItem.Terminal!).State = EntityState.Unchanged;
                }                    

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                //foreach (var detail in item.Details)
                //{
                //    detail.CreateUser = User!;
                //    detail.CreateTime = DateTime.Now;

                //    db.Entry(detail.POD!).State = EntityState.Unchanged;
                //    if (detail.FinalDestination is not null)
                //        db.Entry(detail.FinalDestination!).State = EntityState.Unchanged;

                //    db.Entry(detail).State = EntityState.Added;
                //    modifyItem.Details.Add(detail);
                //}


                // compaire a new item with an existed
                foreach (var modifyDetail in modifyItem.Details!)
                    if (!item.Details!.Any(s => s.Id == modifyDetail.Id))
                    {
                        //db.Entry(modifyDetail).State = EntityState.Deleted;
                        ////modifyItem.Details!.Remove(modifyDetail);
                    }
                    else
                    {
                        modifyDetail.CreateUser = User!;
                        modifyDetail.CreateTime = DateTime.Now;
                        modifyDetail.AgentPOD = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.AgentPOD;

                        modifyDetail.POD = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.POD;
                        db.Entry(modifyDetail.POD!).State = EntityState.Unchanged;

                        modifyDetail.FinalDestination = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.FinalDestination;

                        if (modifyDetail.FinalDestination != null)
                            db.Entry(modifyDetail.FinalDestination!).State = EntityState.Unchanged;
                        else
                            db.Entry(modifyDetail).Reference("FinalDestination").IsModified = true;


                        //db.Entry(modifyDetail).Property("FinalDestination").IsModified = true;
                        //db.Entry(modifyDetail).Property(s => s.FinalDestination).IsModified = true;

                        db.Entry(modifyDetail).State = EntityState.Modified;
                    }

                // compaire an existed item with a new
                foreach (var itemDetail in item.Details!)
                    if (!modifyItem.Details.Any(s => s.Id == itemDetail.Id))
                    {
                        itemDetail.CreateUser = User!;
                        itemDetail.CreateTime = DateTime.Now;

                        db.Entry(itemDetail.CreateUser).State = EntityState.Unchanged;
                        db.Entry(itemDetail.POD!).State = EntityState.Unchanged;
                        if (itemDetail.FinalDestination is not null)
                            db.Entry(itemDetail.FinalDestination!).State = EntityState.Unchanged;                        

                        db.Entry(itemDetail).State = EntityState.Added;
                        modifyItem.Details.Add(itemDetail);
                    }

                db.Entry(modifyItem).State = EntityState.Modified;

                var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                Console.WriteLine(msg);
                appObjResponse.ErrorAdd(msg);
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
                    if (detail.FinalDestination is not null)
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
                var existedItem = db.Set<VesselCallDetail>().Where(vcd => vcd.Id == id).Include(vcd => vcd.ExportOrders).FirstOrDefault();                

                if (existedItem is null)
                {
                    appObjResponse.ErrorAdd("There is no Record to deleted.");
                    return appObjResponse;
                }

                var existedExportOrders = db.ExportOrders.Where(eo => eo.VesselCallDetail!.Id == id);

                if (existedExportOrders.Count() > 0)
                    foreach (var exportOrder in existedItem.ExportOrders)
                        db.Entry(exportOrder).State = EntityState.Deleted;

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

    public async Task<IEnumerable<string>> GetVoyages(string? vessel)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var result = new List<string>();

            if (!string.IsNullOrEmpty(vessel))
                result = await db.VesselCalls.Where(s => s.Vessel.Name == vessel).Select(s => s.VoyageNo!).ToListAsync();
            else
                result = await db.VesselCalls.Select(s => s.VoyageNo!).ToListAsync();

            return result;
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
                    CreateTime = detail.CreateTime,
                };

                RecordsDTO.Add(recordDTO);
            };
        }        
        
        return RecordsDTO;
    };

    #endregion
}
