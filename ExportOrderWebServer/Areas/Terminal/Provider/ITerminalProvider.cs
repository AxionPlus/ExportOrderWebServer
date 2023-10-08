namespace ExportOrderWebServer.Areas.Terminal.Provider;

public interface ITerminalProvider : IEntityProvider<TerminalCatalog>
{
    Task<IEnumerable<CustomsCatalog>> GetCustomsOfficesAsync();
}
