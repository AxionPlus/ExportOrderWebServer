using ExportOrderEntites.ImportVesselCall;

namespace ExportOrderWebServer.Areas.Import.VesselCall.Provider;
public interface IImportVesselCallProvider : IEntityProvider<ImportVesselCallEntity>
{
    Task<AppObjectResponse> GetVesselCallDetailAsync(long vesselCallid);
    Task<IEnumerable<string>> GetVoyages(string? vessel);
    Task<AppObjectResponse> RemoveVesselCallDetailAsync(long id);
}
public class ImportVesselCallProvider : IImportVesselCallProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;
    private AppObjectResponse appObjResponse = new();

    public ImportVesselCallProvider(IDbContextFactory<ApplicationDbContext> dbContext)
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

                var voyage = await db.ImportVesselCalls.AsNoTracking().AsSplitQuery()
                                                 .Include(vc => vc.Vessel)
                                                 .Include(vc => vc.Terminal)
                                                 .Include(vc => vc.Details).ThenInclude(vcd => vcd.POD)
                                                 .Include(vc => vc.Details).ThenInclude(vcd => vcd.FinalDestination)
                                                 .Include(vc => vc.Details).ThenInclude(vcd => vcd.BillofLadings).ThenInclude(eo => eo.ContainerRecords)
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

    public async Task<AppObjectResponse> GetItemsAsync(FilterParameters filter)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync()) 
        {
            var db = await _db;
            
            var vesselCalls = await db.ImportVesselCalls.AsNoTracking().AsSplitQuery()
                                                  .Include(vc => vc.Vessel)
                                                  .Include(vc => vc.Terminal)
                                                  .Include(vc => vc.Details).ThenInclude(d => d.POD)
                                                  .Where(s => filter.DateFrom.HasValue ? s.ETA!.Value >= filter.DateFrom : true)
                                                  .Where(s => filter.DateTo.HasValue ? s.ETS!.Value <= filter.DateTo.Value : true)
                                                  .Where(s => filter.Status != null ? s.Status == filter.Status : s.Status == EntityStatus.New)
                                                  .Where(s => string.IsNullOrEmpty(filter.Vessel) ? true : s.Vessel.Name == filter.Vessel)
                                                  .Where(s => string.IsNullOrEmpty(filter.Voyage) ? true : s.VoyageNo == filter.Voyage)
                                                  .Where(s => string.IsNullOrEmpty(filter.Terminal) ? true : s.Terminal.Name == filter.Terminal)
                                                  .Where(s => string.IsNullOrEmpty(filter.POD) ? true : s.Details.Any(vcd => vcd.POD == null ? true : vcd.POD.NameEn == filter.POD))
                                                  .ToListAsync();

            var vesselCallDetailsDTO = vcRecords(vesselCalls);

            vesselCallDetailsDTO = vesselCallDetailsDTO.OrderByDescending(s => s.CreateTime);                

            appObjResponse.Object = vesselCallDetailsDTO;            

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(ImportVesselCallEntity item, string? UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);
                if (User is null)
                {
                    appObjResponse.ErrorAdd($"User NOT found.");
                    return appObjResponse;
                }

                var modifyItem = await db.ImportVesselCalls.AsNoTracking().AsSplitQuery()    // if AsTracking() - check all of modifyItem's elements for changing
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
                    bool isItemExist = db.ImportVesselCalls.Where(s => item.Vessel != null &&
                                                                !string.IsNullOrWhiteSpace(s.Vessel.Name) && !string.IsNullOrWhiteSpace(item.Vessel.Name) &&
                                                                s.Vessel.Name.ToUpper() == item.Vessel.Name.ToUpper())
                                                     .Where(s => s.VoyageNo.ToUpper() == item.VoyageNo.ToUpper())
                                                     .Any();

                    if (isItemExist)
                    {
                        appObjResponse.ErrorAdd($"{itemVoyage} exists already");
                        return appObjResponse;
                    }
                }

                /// RE-WRITE WITH A NEW ITEM

                modifyItem.CreateUser = User;
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

                /// compaire a new itemDetails with an existed
                foreach (var modifyDetail in modifyItem.Details.ToArray())
                    if (!item.Details.Any(s => s.Id == modifyDetail.Id))
                    {
                        /// check for existing ExportOrders within VesselCallDetail
                        if (!db.ExportOrders.Any(eo => eo.VesselCallDetail != null && eo.VesselCallDetail.Id == modifyDetail.Id))
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
                        modifyDetail.CreateUser = User;
                        modifyDetail.AgentPOD = item.Details.FirstOrDefault(s => s.Id == modifyDetail.Id)!.AgentPOD;

                        modifyDetail.POD = item.Details.FirstOrDefault(s => s.Id == modifyDetail.Id)!.POD;
                        if (modifyDetail.POD is not null)
                            db.Entry(modifyDetail.POD).State = EntityState.Unchanged;

                        db.Entry(modifyDetail).State = EntityState.Modified;
                    }

                /// compaire an existed item with a new
                foreach (var itemDetail in item.Details!)
                    if (!modifyItem.Details.Any(s => s.Id == itemDetail.Id))
                    {
                        itemDetail.CreateUser = User;                        
                        db.Entry(itemDetail.CreateUser).State = EntityState.Unchanged;

                        if (itemDetail.POD is not null)
                            db.Entry(itemDetail.POD).State = EntityState.Unchanged;
                        if (itemDetail.FinalDestination is not null)
                            db.Entry(itemDetail.FinalDestination).State = EntityState.Unchanged;

                        db.Entry(itemDetail).State = EntityState.Added;
                        modifyItem.Details.Add(itemDetail);
                    }

                db.Entry(modifyItem).State = EntityState.Modified;

                //var bug = db.ChangeTracker.DebugView.LongView;

                await db.SaveChangesAsync();

                /// Change Status for an EXPORT ORDERS related
                db.ChangeTracker.Clear();

                var eoList = new List<ExportOrderEntity>();
                
                foreach (var detail in item.Details)
                {
                    var expOrders = await db.ExportOrders.Include(eo => eo.VesselCallDetail)
                                                         .Where(eo => eo.VesselCallDetail!.Id == detail.Id)
                                                         .AsNoTracking().ToListAsync();

                    if  (expOrders.Count > 0)
                        eoList.AddRange(expOrders);
                }

                if (eoList.Count > 0)
                {
                    eoList.ForEach(e => { e.Status = item.Status; db.Entry(e).State = EntityState.Modified; });
                    //eoList.ForEach(e => { db.Entry(e).State = EntityState.Modified; });

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
                Console.WriteLine(ex.Message);
                appObjResponse.ErrorAdd(ex.Message);
                return appObjResponse;
            }

            return appObjResponse;
        }        
    }

    public async Task<AppObjectResponse> NewItemAsync(ImportVesselCallEntity item, string? UserName = "")
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            try
            {            
                var db = await _db;

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == UserName);
                if (User is null)
                {
                    appObjResponse.ErrorAdd($"User NOT found.");
                    return appObjResponse;
                }

                bool isItemExist = db.ImportVesselCalls.Any(s => s.Vessel.Name == item.Vessel.Name && s.VoyageNo == item.VoyageNo);

                if (isItemExist)
                {
                    appObjResponse.ErrorAdd($"Voyage: {item.VoyageNo} for vessel: {item.Vessel.Name} is exists already.");
                    return appObjResponse;
                }

                item.CreateUser = User;

                foreach (var detail in item.Details)
                {
                    detail.CreateUser = User;
                    db.Entry(detail.CreateUser).State = EntityState.Unchanged;

                    if (detail.POD is not null)
                        db.Entry(detail.POD).State = EntityState.Unchanged;

                    if (detail.FinalDestination is not null)
                        db.Entry(detail.FinalDestination).State = EntityState.Unchanged;

                    db.Entry(detail).State = EntityState.Added;                     
                }

                db.Entry(item.CreateUser).State = EntityState.Unchanged;
                db.Entry(item.Vessel).State = EntityState.Unchanged;
                db.Entry(item.Terminal).State = EntityState.Unchanged;

                db.Entry(item).State = EntityState.Added;

                //var bug = db.ChangeTracker.DebugView.LongView;

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
                appObjResponse.ErrorAdd(ex.Message);
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

    public async Task<AppObjectResponse> GetVesselCallDetailAsync(long vcdId)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            try
            {
                var db = await _db;

                appObjResponse.Object = await db.Set<ImportVesselCallDetail>().AsNoTracking().AsSplitQuery()
                                                    .Include(vcd => vcd.VesselCall).ThenInclude(vc => vc.Terminal)
                                                    .Include(vcd => vcd.VesselCall).ThenInclude(vc => vc.Vessel)
                                                    .Include(vcd => vcd.POD)
                                                    //.Include(vcd => vcd.FinalDestination)
                                                    .FirstOrDefaultAsync(vcd => vcd.Id == vcdId);
                
                if (appObjResponse.Object is null)
                    appObjResponse.ErrorAdd("There is no voyage");

                return appObjResponse;
            }
            catch (Exception ex)
            {
                appObjResponse.ErrorAdd(ex.Message);
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

                /// check an Existing - Vessel Call Detail
                var dbVesselCallDetail = db.Set<ImportVesselCallDetail>().Where(vcd => vcd.Id == id).Include(vcd => vcd.BillofLadings).FirstOrDefault();

                if (dbVesselCallDetail is null)
                    appObjResponse.ErrorAdd("There is no Record to delete.");
                else
                {
                    /// Find an Export Orders in Existing Item                    
                    if (dbVesselCallDetail.BillofLadings.Any()) //db.ExportOrders.Any(eo => eo.VesselCallDetail != null && eo.VesselCallDetail.Id == id)
                        appObjResponse.ErrorAdd($"Vessel Call has an Export Orders issued.<br/>Delete all of it's Export Orders first.");
                    else
                    {
                        db.Entry(dbVesselCallDetail).State = EntityState.Deleted;

                        /// Check an Empty Voyage - w/o existing Vessel Call Details
                        bool isOneDetailInVoyage = db.Set<ImportVesselCallDetail>().Count(vcd => vcd.VesselCall.Id == dbVesselCallDetail.VesselCall.Id) == 1;
                        if (isOneDetailInVoyage)
                        {
                            var dbVoyage = db.ImportVesselCalls.FirstOrDefault(s => s.Id == dbVesselCallDetail.VesselCall.Id);
                            if (dbVoyage != null)
                                db.Entry(dbVoyage).State = EntityState.Deleted;
                        }

                        var bug = db.ChangeTracker.DebugView.LongView;
                        await db.SaveChangesAsync();
                    }
                }

                return appObjResponse;
            }
        }
        catch (Exception ex)
        {
            appObjResponse.ErrorAdd(ex.Message);
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
    
    public async Task<IEnumerable<string>> GetVoyages(string? vessel)
    {
        using var _db = _dbContext.CreateDbContextAsync();

        var db = await _db;

        var result = Enumerable.Empty<string>();

        if (!string.IsNullOrEmpty(vessel))
            result = await db.ImportVesselCalls.Where(s => s.Vessel.Name == vessel).OrderByDescending(s => s.CreateTime).Select(s => s.VoyageNo).ToArrayAsync();
        else
            result = await db.ImportVesselCalls.OrderByDescending(s => s.CreateTime).Select(s => s.VoyageNo!).ToArrayAsync();

        return result;
    }

    private readonly Func<IEnumerable<ImportVesselCallEntity>, IEnumerable<VesselCallDetailDTO>> vcRecords = (Records) =>
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
                    POD = detail.POD?.NameEn,
                    AgentPOD = detail.AgentPOD,
                    Status = record.Status,
                    CreateTime = detail.CreateTime,
                };

                RecordsDTO.Add(recordDTO);
            };
        }        
        
        return RecordsDTO;
    };
}
