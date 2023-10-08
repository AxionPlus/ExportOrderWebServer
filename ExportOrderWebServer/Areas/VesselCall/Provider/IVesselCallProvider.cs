namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    Task<AppObjectResponse> GetItemAsync(string name);
    Task<IEnumerable<string>> GetPODs();
    Task<IEnumerable<string>> GetVoyagesCarrier();
    Task<IEnumerable<VesselCallCarrierDTO>> GetVesselCallCarriersAsync(long vslCallId);
}
