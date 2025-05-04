namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    Task<AppObjectResponse> GetVesselCallDetailAsync(long vesselCallid);
    Task<IEnumerable<string>> GetVoyages(string? vessel);
    Task<AppObjectResponse> RemoveVesselCallDetailAsync(long id);
}