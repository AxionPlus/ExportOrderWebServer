namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    Task<AppObjectResponse> GetItemAsync(string voyage);
    Task<AppObjectResponse> GetVesselCallDetailItemAsync(string voyagePOD);
    
    Task<IEnumerable<string>> GetPODs();
    Task<IEnumerable<string>> GetVoyagesCarrier();
    //Task<IEnumerable<_VesselCallCarrierDTO>> GetVesselCallCarriersAsync(long vslCallId);
    Task<AppObjectResponse> RemoveItemDetailsAsync(long id);

}
