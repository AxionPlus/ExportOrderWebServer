namespace ExportOrderWebServer.Areas.Carrier.Provider;

public class CarrierProvider : ICarrierProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public CarrierProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task<AppObjectResponse> GetItemAsync(long id)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Carriers.AsNoTracking().Include(s => s.CarrierDetails)
                                                                    .Include(s => s.Location)
                                                                    .FirstOrDefaultAsync(s => s.Id == id);
        }

        return appObjResponse;
    }

    public async Task<AppObjectResponse> GetItemsAsync()
    {
        appObjResponse = new();
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            appObjResponse.Object = await db.Carriers.Include(c => c.CarrierDetails).ToListAsync();
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Carriers = await db.Carriers.ToListAsync();

            if (parameters.GetType() == typeof(FilterParameters))
            {
                var filter = (FilterParameters)parameters;

                if (!string.IsNullOrEmpty(filter.Name))
                    Carriers = Carriers.Where(s => s.Name == filter.Name).ToList();

                if (!string.IsNullOrEmpty(filter.NameEn))
                    Carriers = Carriers.Where(s => s.NameEn == filter.NameEn).ToList();
            }

            appObjResponse.Object = Carriers.ToArray();

            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> ModifyItemAsync(CarrierCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            try
            {
                var modifyItem = await db.Carriers.Include(car => car.CarrierDetails).AsNoTracking().FirstOrDefaultAsync(s => s.Id == item.Id);

                if (modifyItem!.Name != item.Name)
                {
                    var itemExistCheck = await db.Carriers.Where(s => s.Name!.ToUpper() == item.Name!.ToUpper()).FirstOrDefaultAsync();

                    if (itemExistCheck is not null)
                    {
                        appObjResponse.ErrorAdd($" {item.Name} exists already");
                        return appObjResponse;
                    }
                }

                var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

                modifyItem.CreateUser = User!;
                modifyItem.CreateTime = DateTime.Now;
                modifyItem.Name = item.Name;
                modifyItem.NameEn = item.NameEn;                
                modifyItem.BlTemplate = item.BlTemplate;
                modifyItem.Location = item.Location!;

                db.Entry(modifyItem.Location).State = EntityState.Unchanged;
                db.Entry(modifyItem.CreateUser).State = EntityState.Unchanged;

                // compaire new item with existed
                foreach (var modifyDetail in modifyItem.CarrierDetails!)
                    if (!item.CarrierDetails!.Any(s => s.Id == modifyDetail.Id))
                        db.Entry(modifyDetail).State = EntityState.Deleted;
                    else
                    {
                        modifyDetail.Id = item.CarrierDetails!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.Id;
                        modifyDetail.TerminalName = item.CarrierDetails!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.TerminalName;
                        modifyDetail.Contract = item.CarrierDetails!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.Contract;
                        modifyDetail.DateContract = item.CarrierDetails!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.DateContract;
                        modifyDetail.AgentPOL = item.CarrierDetails!.FirstOrDefault(s => s.Id == modifyDetail.Id)!.AgentPOL;
                    }

                // compaire existed item with new
                foreach (var itemDetail in item.CarrierDetails!)
                    if (!modifyItem.CarrierDetails.Any(s => s.Id == itemDetail.Id))
                    {   
                        db.Entry(itemDetail).State = EntityState.Added;
                        modifyItem.CarrierDetails.Add(itemDetail);
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

    public async Task<AppObjectResponse> NewItemAsync(CarrierCatalog item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var User = await db.Set<ApplicationUser>().AsNoTracking().FirstOrDefaultAsync(s => s.UserName == ApplicationParameter.ApplicationUser);

            // check an Existing item
            var itemExistCheck = await db.Carriers.Where(s => s.Name == item!.Name).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Carrier exists already: {item!.Name}");
                return appObjResponse;
            }

            item.CreateUser = User!;         

            foreach (var record in item.CarrierDetails)
                db.Entry(record).State = EntityState.Added;
            
            db.Entry(item).State = EntityState.Added;
            
            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();
          
            return appObjResponse;
        }
    }

    public async Task<AppObjectResponse> RemoveItemAsync(long id)
    {
        appObjResponse = new();
        return appObjResponse;
    }

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Carriers.OrderBy(s => s.Name).Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Carriers.OrderBy(S => S.NameEn).Select(s => s.NameEn!).ToListAsync();
        }
    }

    #region AUXILARY

    public async Task<AppObjectResponse> RemoveDetailsAsync(CarrierTerminalDetails item)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            // check an Existing item
            var itemExistCheck = await db.CarrierDetails.Where(s => s.Id == item!.Id).FirstOrDefaultAsync();
            if (itemExistCheck != null)
            {
                appObjResponse.ErrorAdd($"Carrier details dosn't exists for: {item!.TerminalName}");
                return appObjResponse;
            }

            db.Entry(item).State = EntityState.Deleted;

            var bug = db.ChangeTracker.DebugView.LongView;

            await db.SaveChangesAsync();

            return appObjResponse;
        }
    }

    public async Task<IEnumerable<string>> GetTerminalNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Terminals.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<AppObjectResponse> GetTerminalNameAsync(long vesselCallid, string name)
    {
        appObjResponse = new();

        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;

            var Carrier = await db.ExportOrders.Include(eo => eo.Carrier).ThenInclude(c => c!.CarrierDetails)
                                               .Include(eo => eo.VesselCallDetail!.VesselCall)
                                               .Where(eo => eo.VesselCallDetail!.VesselCall.Id == vesselCallid)
                                               .Select(eo => eo.Carrier).Where(c => c!.CarrierDetails.Any(c => c.TerminalName != name) == true)
                                               .ToListAsync();

            bool IsExistedTerminal = await db.ExportOrders.Include(eo => eo.Carrier).ThenInclude(c => c!.CarrierDetails)
                                                .Include(eo => eo.VesselCallDetail!.VesselCall)
                                                .Where(eo => eo.VesselCallDetail!.VesselCall.Id == vesselCallid)
                                                .Select(eo => eo.Carrier).Select(c => c!.CarrierDetails.Any(cd => cd.TerminalName == name))
                                                .FirstOrDefaultAsync();

            if (!IsExistedTerminal)
            {
                string carriers = string.Empty;

                if (Carrier.Count() > 0)
                    carriers = string.Join("; ", Carrier.Select(c => c!.NameEn));

                appObjResponse.ErrorAdd($"At list one of the Carrier has no Agreement with a Terminal nominated for present Voyage<br>Add Terminal to the following Carriers:<br>{carriers}");
            }
        }

        return appObjResponse;
    }
    
    #endregion

}
