namespace ExportOrderWebServer.Areas.Location.Provider;

public interface ILocationProvider : IEntityProvider<LocationCatalog>
{
    Task<IEnumerable<string>> GetUNLocodes();
}
