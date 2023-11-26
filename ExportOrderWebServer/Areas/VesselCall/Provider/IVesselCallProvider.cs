namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    Task<AppObjectResponse> GetItemAsync(string voyage);
    Task<AppObjectResponse> GetVesselCallDetailItemAsync(long vesselCallid);
    Task<IEnumerable<string>> GetPODs();
    Task<IEnumerable<string>> GetVoyages(string? vessel);
    Task<AppObjectResponse> RemoveVesselCallDetailDTOAsync(long id);
}
