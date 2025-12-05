using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Dto;
using ExportOrderWebServer.Areas.ImportDocument.Extensions;

namespace ExportOrderWebServer.Areas.ImportDocument.Provider
{
    public interface ICatalogObjectProvider
    {

        Task<IEnumerable<ObjectDto>> GetPort(CancellationToken cancellationToken = default);
        Task<IEnumerable<ObjectDto>> GetTerminal(CancellationToken cancellationToken = default);
        Task<IEnumerable<ObjectDto>> GetVessel(CancellationToken cancellationToken = default);
    }
    public class CatalogObjectProvider : ICatalogObjectProvider
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContext;

        public CatalogObjectProvider(IDbContextFactory<ApplicationDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ObjectDto>> GetPort(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);

            var query = db.Set<PortBaseEntity>()
                .AsQueryable();

            IEnumerable<ObjectDto> items = await query.Select(s => s.ToObjectDto()!).ToArrayAsync(cancellationToken);
            return items;
        }

        public async Task<IEnumerable<ObjectDto>> GetTerminal(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);
            var query = db.Set<TerminalBaseEntity>()
                .AsQueryable();
            IEnumerable<ObjectDto> items = await query.Select(s => s.ToObjectDto()!).ToArrayAsync(cancellationToken);
            return items;
        }

        public async Task<IEnumerable<ObjectDto>> GetVessel(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbContext.CreateDbContextAsync(cancellationToken);
            var query = db.Set<VesselBaseEntity>()
                .AsQueryable();

            IEnumerable<ObjectDto> items = await query.Select(s => s.ToObjectDto()!).ToArrayAsync(cancellationToken);

            return items;
        }
    }


    public enum CatalogObjectDataType
    {
        Name,
        ShortName_Inn,
        SystemName,
        Default
    }
}
