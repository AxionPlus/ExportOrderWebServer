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

            appObjResponse.Object = await db.VesselCalls.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemAsync(string name)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            if (name.Contains('&'))
            {
                var vesselName = name.Split('&')[0];
                var voyageNo = name.Split('&')[1];

                var vesselCalls = await db.VesselCalls
                                                    .Include(vc => vc.Vessel)
                                                    .Include(vc => vc.Terminal)
                                                    .Include(vc => vc.Details).ThenInclude(d => d.POD)
                                                    .AsSplitQuery()
                                                    .ToListAsync();
                if (appObjResponse.Object is null)
                    appObjResponse.ErrorAdd($"There is no voayeg {vesselName} {voyageNo}");
            }
            else
                appObjResponse.ErrorAdd("Wrong request");

            return appObjResponse;
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
                    //vesselCallsDTO = vesselCallsDTO.Where(s => s.Details.FirstOrDefault(d => d.VesselCall!.Id == s.Id)!.POD!.Name == filter.POD).ToList();

                appObjResponse.Object = vesselCallsDTO.ToArray();
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
                appObjResponse.ErrorAdd($"Vessel {item.Vessel.Name} with ETA {item.ETA!.Value.ToString("dd.MM.yy")} is exists already.");
                return appObjResponse;
            }

            item.CreateUser = User!;

            foreach (var detail in item.Details)
            {
                db.Entry(detail).State = EntityState.Added;
                db.Entry(detail.POD!).State = EntityState.Unchanged;
            }

            db.Entry(item.Vessel).State = EntityState.Unchanged;
            db.Entry(item.Terminal).State = EntityState.Unchanged;
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
            return await db.VesselCalls.Select(s => s.VoyageNo!).ToListAsync();
        }
    }

    Func<IEnumerable<VesselCallEntity>, IEnumerable<VesselCallDTO>> vcRecords = (Records) =>
    {
        var RecordsDTO = new List<VesselCallDTO>();

        foreach (var record in Records)
        {
            foreach (var detail in record.Details)                
            {
                var recordDTO = new VesselCallDTO()
                {
                    Id = detail.Id,
                    VesselName = record.Vessel.Name,
                    VoyageNo = record.VoyageNo,
                    VoyageNoTerminal = record.VoyageNoTerminal,
                    ETA = record.ETA,
                    ETS = record.ETS,
                    POD = detail.POD!.NameEn,
                    AgentPOD = detail.AgentPOD,
                };

                RecordsDTO.Add(recordDTO);
            };
        }        
        
        return RecordsDTO;
    };

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
