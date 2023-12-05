namespace ExportOrderWebServer.Areas.Carrier.Provider;

public interface ICarrierProvider : IEntityProvider<CarrierCatalog>
{
    Task<AppObjectResponse> RemoveDetailsAsync(CarrierTerminalDetails item);
    Task<IEnumerable<string>> GetTerminalNames();
    Task<AppObjectResponse> GetTerminalNameAsync(long vesselCallid, string name);
}
