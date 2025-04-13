namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    Task<AppObjectResponse> GetVesselCallDetailAsync(long vesselCallid);
    Task<IEnumerable<string>> GetPODs();
    Task<IEnumerable<string>> GetVoyages(string? vessel);
    Task<AppObjectResponse> GetItemToCheckAsync(long id);
    Task<AppObjectResponse> RemoveVesselCallDetailAsync(long id);
}