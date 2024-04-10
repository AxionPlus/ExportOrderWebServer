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
            try
            {
                var db = await _db;

                var voyage = await db.VesselCalls.AsNoTracking().AsSplitQuery()
                                                 .Include(vc => vc.Vessel)
                                                 .Include(vc => vc.Terminal)
                                                 .Include(vc => vc.Details).ThenInclude(vcd => vcd.POD)
                                                 .Include(vc => vc.Details).ThenInclude(vcd => vcd.FinalDestination)
                                                 .Include(vc => vc.Details).ThenInclude(vcd => vcd.ExportOrders).ThenInclude(eo => eo.Records)
                                                 .FirstOrDefaultAsync(s => s.Id == id);

                if (voyage is null)
                    appObjResponse.ErrorAdd($"There is no voyage you choose.");

                appObjResponse.Object = voyage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return appObjResponse;
            }
        }

        return appObjResponse;
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
                                                    .Where(vc => filter.ETA.HasValue ? vc.ETA!.Value >= filter.ETA.Value : true)
                                                    .Where(vc => filter.ETS.HasValue ? vc.ETS!.Value >= filter.ETS.Value : true)
                                                    .AsSplitQuery()
                                                    .ToListAsync();
                
                var vesselCallDetailsDTO = vcRecords(vesselCalls);

                if (filter.Status >= 0)
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.Status == filter.Status).ToList();
                else
                    if (string.IsNullOrEmpty(filter.Vessel) && string.IsNullOrEmpty(filter.Voyage) && string.IsNullOrEmpty(filter.Terminal)
                            && string.IsNullOrEmpty(filter.POD))
                        vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.Status == EntityStatus.New).ToList();
                
                if (!string.IsNullOrEmpty(filter.Vessel))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.VesselName == filter.Vessel).ToList();

                if (!string.IsNullOrEmpty(filter.Voyage))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.VoyageNo == filter.Voyage).ToList();

                if (!string.IsNullOrEmpty(filter.Terminal))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.Terminal == filter.Terminal).ToList();

                if (!string.IsNullOrEmpty(filter.POD))
                    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.POD == filter.POD).ToList();

                //if (!string.IsNullOrEmpty(filter.Carrier))
                //    vesselCallDetailsDTO = vesselCallDetailsDTO.Where(s => s.CarrierName == filter.Carrier).ToList();

                vesselCallDetailsDTO = vesselCallDetailsDTO.OrderByDescending(s => s.CreateTime);

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
                var modifyItem = await db.VesselCalls.AsNoTracking()    // if AsTracking() - check all of modifyItem's elements for changing
                                                    .Include(vc => vc.Vessel)
                                                    .Include(vc => vc.Terminal)
                                                    .Include(vc => vc.Details)
                                                    .FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem is null)
                {
                    appObjResponse.ErrorAdd($"{item.Vessel} / {item.VoyageNo} not found.");
                    return appObjResponse;
                }

                string modifyVoyage = $"{modifyItem.Vessel?.Name}&{modifyItem.VoyageNo}";
                string itemVoyage = $"{item.Vessel?.Name}&{item.VoyageNo}";

                if (modifyVoyage != itemVoyage)
                {
                    var itemExistCheck = await db.VesselCalls.Where(s => s.Vessel!.Name!.ToUpper() == item.Vessel!.Name!.ToUpper())
                                                            .Where(s => s.VoyageNo.ToUpper() == item.VoyageNo.ToUpper())
                                                            .FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($"{itemVoyage} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                // RE-WRITE WITH A NEW ITEM

                modifyItem.CreateUser = User!;
                modifyItem.CreateTime = DateTime.Now;
                modifyItem.Status = item.Status;
                modifyItem.VoyageNo = item.VoyageNo;
                modifyItem.VoyageNoTerminal = item.VoyageNoTerminal;
                modifyItem.ETA = item.ETA;
                modifyItem.ETS = item.ETS;

                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                modifyItem.Vessel = item.Vessel!;
                db.Entry(modifyItem.Vessel!).State = EntityState.Unchanged;

                modifyItem.Terminal = item.Terminal;
                db.Entry(modifyItem.Terminal!).State = EntityState.Unchanged;

                // -------------------------------------------------------------------
                // compaire a new itemDetails with an existed
                foreach (var modifyDetail in modifyItem.Details!)
                    if (!item.Details!.Any(s => s.Id == modifyDetail.Id))
                    {
                        // check for existing ExportOrders within VesselCallDetail
                        if (!db.ExportOrders.Any(eo => eo.VesselCallDetail!.Id == modifyDetail.Id))
                        {
                            db.Entry(modifyDetail).State = EntityState.Deleted;
                            modifyItem.Details.Remove(modifyDetail);
                        }                            
                        else
                        {
                            appObjResponse.ErrorAdd($"Удалить рейс нельзя. В этом рейсе есть поручения.");
                            return appObjResponse;
                        }
                    }
                    else
                    {
                        modifyDetail.CreateUser = User!;
                        modifyDetail.CreateTime = DateTime.Now;
                        modifyDetail.AgentPOD = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.AgentPOD;

                        modifyDetail.POD = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.POD;                        
                        db.Entry(modifyDetail.POD!).State = EntityState.Unchanged;

                        //modifyDetail.FinalDestination = item.Details!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.FinalDestination;

                        //if (modifyDetail.FinalDestination != null)
                        //    db.Entry(modifyDetail.FinalDestination!).State = EntityState.Unchanged;
                        //else
                        //    db.Entry(modifyDetail).Reference("FinalDestination").IsModified = true;

                        db.Entry(modifyDetail).State = EntityState.Modified;
                    }

                // compaire an existed item with a new
                foreach (var itemDetail in item.Details!)
                    if (!modifyItem.Details.Any(s => s.Id == itemDetail.Id))
                    {
                        itemDetail.CreateUser = User!;                        
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

                // Change Status for an EXPORT ORDERS related
                db.ChangeTracker.Clear();

                var eoList = new List<ExportOrderEntity>();
                
                foreach (var detail in item.Details)
                {
                    var expOrders = await db.ExportOrders
                                              .Include(eo => eo.VesselCallDetail)
                                              .Where(eo => eo.VesselCallDetail!.Id == detail.Id)
                                              .AsNoTracking().ToListAsync();

                    if  (expOrders.Count() > 0)
                        eoList.AddRange(expOrders);
                }

                if (eoList.Count() > 0)
                {
                    eoList.ForEach(e => { e.Status = item.Status; });

                    eoList.ForEach(e => { db.Entry(e).State = EntityState.Modified; });

                    await db.SaveChangesAsync();
                }

                db.ChangeTracker.Clear();

                #region HISTORY
                //db.ChangeTracker.Clear();

                //var historyItemDetails = new List<VesselCallDetailsHistory>();

                //foreach (var detail in item.Details)
                //{
                //    historyItemDetails.Add(new VesselCallDetailsHistory()
                //    {
                //        POD = detail.POD is null ? null : detail.POD.NameEn,
                //        FinalDestination = detail.FinalDestination is null ? null : detail.FinalDestination.NameEn,
                //        AgentPOD = detail.AgentPOD
                //    });
                //}

                //var historyItem = new VesselCallHistory()
                //{
                //    Status = item.Status,
                //    CreateUser = User,
                //    CreateTime = item.CreateTime,
                //    Mode = HistoryEventMode.Modify,

                //    VesselName = item.Vessel!.Name,
                //    VoyageNo = item.VoyageNo,
                //    VoyageNoTerminal = item.VoyageNoTerminal,
                //    TerminalName = item.Terminal!.Name,
                //    ETA = item.ETA,
                //    ETS = item.ETS,
                //    Details = historyItemDetails,
                //};

                //db.Entry(historyItem).State = EntityState.Added;

                //await db.SaveChangesAsync();
                #endregion
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

                #region HISTORY
                //db.ChangeTracker.Clear();

                //bool IsItemExists = await db.VesselCalls.AnyAsync(vc => vc.Id == item.Id);
                //if (IsItemExists) { appObjResponse.ErrorAdd($"History not saved"); }
                //else
                //{
                //    //db.ChangeTracker.Clear();
                //    var historyItemDetails = new List<VesselCallDetailsHistory>();

                //    foreach (var detail in item.Details)
                //    {
                //        historyItemDetails.Add(new VesselCallDetailsHistory()
                //        {
                //            POD = detail.POD is null ? null : detail.POD.NameEn,
                //            FinalDestination = detail.FinalDestination is null ? null : detail.FinalDestination.NameEn,
                //            AgentPOD = detail.AgentPOD
                //        });
                //    }                    

                //    var historyItem = new VesselCallHistory()
                //    {
                //        Status = item.Status,
                //        CreateUser = User,
                //        CreateTime = DateTime.Now,
                //        Mode = HistoryEventMode.New,

                //        VesselName = item.Vessel.Name,
                //        VoyageNo = item.VoyageNo,
                //        VoyageNoTerminal = item.VoyageNoTerminal,
                //        TerminalName = item.Terminal.Name,
                //        ETA = item.ETA,
                //        ETS = item.ETS,
                //        Details = historyItemDetails,
                //    };

                //    db.Entry(historyItem).State = EntityState.Added;

                //    await db.SaveChangesAsync();
                //}
                #endregion
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                return appObjResponse;
            }

            return appObjResponse;
        }
    }

    public Task<AppObjectResponse> RemoveItemAsync(long id)
    {
        throw new NotImplementedException();
        //appObjResponse = new();

        //try
        //{
        //    using (var _db = _dbContext.CreateDbContextAsync())
        //    {
        //        var db = await _db;

        //        // check an Existing item
        //        var existedItem = await db.VesselCalls.Where(s => s.Id == id).FirstOrDefaultAsync();

        //        if (existedItem is null)
        //        {
        //            appObjResponse.ErrorAdd("Record wasn't deleted");
        //            return appObjResponse;
        //        }

        //        db.Entry(existedItem).State = EntityState.Deleted;

        //        var bug = db.ChangeTracker.DebugView.LongView;
        //        await db.SaveChangesAsync();

        //        return appObjResponse;
        //    }
        //}
        //catch (Exception ex)
        //{
        //    string msg = ex.Message;
        //    appObjResponse.ErrorAdd(msg);
        //    return appObjResponse;
        //}
    }

    #region AUXILARY METHODS
    public async Task<AppObjectResponse> GetVesselCallDetailAsync(long vcdId)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            try
            {
                var db = await _db;

                appObjResponse.Object = await db.Set<VesselCallDetail>().AsNoTracking()                                                    
                                                    .Include(vcd => vcd.VesselCall).ThenInclude(vc => vc.Terminal)
                                                    .Include(vcd => vcd.VesselCall).ThenInclude(vc => vc.Vessel)
                                                    .Include(vcd => vcd.POD)
                                                    //.Include(vcd => vcd.FinalDestination)
                                                    //.Include(vcd => vcd.ExportOrders)
                                                    .FirstOrDefaultAsync(vcd => vcd.Id == vcdId);
                //var bug = db.ChangeTracker.DebugView?.LongView;
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
    public async Task<AppObjectResponse> RemoveVesselCallDetailAsync(long id)
    {
        appObjResponse = new();

        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                // check an Existing item - Vessel Call Detail
                var existedItem = db.Set<VesselCallDetail>().Where(vcd => vcd.Id == id).Include(vcd => vcd.ExportOrders).FirstOrDefault();

                if (existedItem is null)
                    appObjResponse.ErrorAdd("There is no Record to delete.");
                else
                {
                    // Find an Export Orders in Existing Item
                    if (db.ExportOrders.Any(eo => eo.VesselCallDetail!.Id == id))
                        appObjResponse.ErrorAdd($"Vessel Call has an Export Orders issued.<br/>Delete all of it's Export Orders first.");
                    else
                    {
                        db.Entry(existedItem).State = EntityState.Deleted;

                        var bug = db.ChangeTracker.DebugView.LongView;
                        await db.SaveChangesAsync();
                    }
                }

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
                result = await db.VesselCalls.Where(s => s.Vessel.Name == vessel).OrderByDescending(x => x.CreateTime).Select(s => s.VoyageNo!).ToListAsync();
            else
                result = await db.VesselCalls.OrderByDescending(x => x.CreateTime).Select(s => s.VoyageNo!).ToListAsync();

            return result;
        }
    }

    public async Task<IEnumerable<string>> GetExportOrderNumsAsync(long id)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var vesselCallDetail = await db.VesselCalls
                                               .Include(vc => vc.Vessel)
                                               .Include(vc => vc.Details).ThenInclude(vcd => vcd.ExportOrders)                                               
                                               .AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

            if (vesselCallDetail != null)
                return vesselCallDetail.Details.SelectMany(vcd => vcd.ExportOrders).Select(eo => eo.Num).ToList();
            else
                return Enumerable.Empty<string>();
        }
    }
    public async Task<AppObjectResponse> GetItemToCheckAsync(long id)
    {
        appObjResponse = new();
        try
        {
            using (var _db = _dbContext.CreateDbContextAsync())
            {
                var db = await _db;

                var voyage = await db.VesselCalls.AsNoTracking().AsSplitQuery()
                                                .Include(vc => vc.Details).ThenInclude(vcd => vcd.ExportOrders).ThenInclude(eo => eo.Records)
                                                .FirstOrDefaultAsync(s => s.Id == id);

                if (voyage is null)
                    appObjResponse.ErrorAdd($"There is no voyage you choose.");

                appObjResponse.Object = voyage;                
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return appObjResponse;
        }

        return appObjResponse;
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
                    Status = record.Status,
                    CreateTime = detail.CreateTime,
                };

                RecordsDTO.Add(recordDTO);
            };
        }        
        
        return RecordsDTO;
    };

    #endregion
}
