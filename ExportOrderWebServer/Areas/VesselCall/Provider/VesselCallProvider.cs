using Microsoft.EntityFrameworkCore;

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

            appObjResponse.Object = await db.VesselCalls.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
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
                                    .Where(s => filter.ETA.HasValue ? s.ETA!.Value >= filter.ETA.Value : true)
                                    .Where(s => filter.ETS.HasValue ? s.ETS!.Value >= filter.ETS.Value : true)
                                    .ToListAsync();


                if (!string.IsNullOrEmpty(filter.VesselName))
                    vesselCalls = vesselCalls.Where(s => s.Vessel.Name == filter.VesselName).ToList();

                if (!string.IsNullOrEmpty(filter.TerminalName))
                    vesselCalls = vesselCalls.Where(s => s.LoadingTerminal.Name == filter.TerminalName).ToList();

                if (!string.IsNullOrEmpty(filter.POD))
                    vesselCalls = vesselCalls.Where(s => s.POD.Name == filter.POD).ToList();

                appObjResponse.Object = vesselCalls.ToArray();
            }            

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(VesselCallEntity item)
    {
        throw new NotImplementedException();
    }

    public async Task<AppObjectResponse> NewItemAsync(VesselCallEntity item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.VesselCalls.Where(s => s.Vessel.Name == item.Vessel.Name)
                                                    .Where(s => s.ETA!.Value == item.ETA!.Value).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Vessel {item.Vessel.Name} with ETA {item.ETA!.Value.ToString("d")} is exists already.");
                return appObjResponse;
            }

            item.CreateUser = User!;

            db.Entry(item.Vessel).State = EntityState.Unchanged;
            db.Entry(item.LoadingTerminal).State = EntityState.Unchanged;
            db.Entry(item.POD).State = EntityState.Unchanged;
            db.Entry(item).State = EntityState.Added;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(VesselCallEntity item)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Vessels.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
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

    public async Task<IEnumerable<string>> GetVoyagesCarrier()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.VesselCalls.Select(s => s.VoyageCarrier!).ToListAsync();
        }
    }

    public async Task<IEnumerable<VesselCallCarrierDTO>> GetVesselCallCarriersAsync(long vslCallId)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var eoItems = await db.ExportOrders.AsNoTracking().Include(eo => eo.Carrier)
                            .Where(eo => eo.VesselCall!.Id == vslCallId)
                            .Select(eo => new 
                            {
                                VesselCallId = eo.VesselCall!.Id,
                                CarrierId = eo.Carrier!.Id,
                                CarrierNameEn = eo.Carrier!.NameEn,
                                ExportOrderId = eo.Id,
                                ExportOrderNum = eo.Num,
                                ExportOrderDate = eo.Dated,
                            })
                            .ToListAsync();            

            var CarriersGroup = eoItems.GroupBy(e => e.CarrierNameEn)
                                  .Select(g => new VesselCallCarrierDTO()
                                  {
                                      VesselCallId = g.FirstOrDefault()!.VesselCallId,
                                      CarrierId = g.FirstOrDefault()!.CarrierId,
                                      CarrierNameEn = g.Key,
                                      RecordsDTO = g.Select(r => new VesselCallRecordDTO()
                                      {
                                        ExportOrderId = r.ExportOrderId,
                                        ExportOrderNum = r.ExportOrderNum,
                                        ExportOrderDate = r.ExportOrderDate,
                                        //vesselCallCarrierDTO = g,
                                      }).ToList()
                                  }).ToList();

            return CarriersGroup;
        }
    }

    #endregion
}
