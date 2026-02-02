namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface _IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    Task<AppObjectResponse> GetVesselCallDetailAsync(long vesselCallid);
    Task<IEnumerable<string>> GetVoyages(string? vessel);
}