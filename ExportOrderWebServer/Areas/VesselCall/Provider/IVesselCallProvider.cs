namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    Task<AppObjectResponse> GetItemAsync(string name);
    Task<IEnumerable<string>> GetPODs();
    Task<IEnumerable<string>> GetVoyagesCarrier();
    //Task<IEnumerable<_VesselCallCarrierDTO>> GetVesselCallCarriersAsync(long vslCallId);
    Task<AppObjectResponse> RemoveItemDTOAsync(VesselCallDTO item);

}
