namespace ExportOrderWebServer.Areas.Carrier.Provider;

public interface ICarrierProvider : IEntityProvider<CarrierCatalog>
{
    Task<AppObjectResponse> RemoveDetailsAsync(CarrierTerminalDetails item);
    Task<AppObjectResponse> GetTerminalNameAsync(long vesselCallid, string name);
}
