using Microsoft.EntityFrameworkCore;

namespace ExportOrderWebServer.Areas.Vessel.Provider;

public class VesselProvider : IVesselProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

    private AppObjectResponse appObjResponse;

    public VesselProvider(IDbContextFactory<ApplicationDbContext> dbContext)
    {
        _dbContext = dbContext;
    }



    public Task<AppObjectResponse> GetItemAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<AppObjectResponse> GetItemsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<AppObjectResponse> GetItemsAsync(object parameters)
    {
        throw new NotImplementedException();
    }

    public Task<AppObjectResponse> ModifyItemAsync(VesselEntity item)
    {
        throw new NotImplementedException();
    }

    public Task<AppObjectResponse> NewItemAsync(VesselEntity item)
    {
        throw new NotImplementedException();
    }

    public Task<AppObjectResponse> RemoveItemAsync(VesselEntity item)
    {
        throw new NotImplementedException();
    }

    #region SEARCH METHODS

    public async Task<IEnumerable<string>> GetNames()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.Select(s => s.Name!).ToListAsync();
        }
    }

    public async Task<IEnumerable<string>> GetNamesEn()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.Select(s => s.EngName!).ToListAsync();
        }
    }
        public async Task<IEnumerable<string>> GetIMOnos()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Commodities.Select(s => s.HSCode).ToListAsync();
        }
    }

    #endregion
}
