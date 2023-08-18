namespace ExportOrderWebServer.Areas.VesselCall.Provider;

public interface IVesselCallProvider : IEntityProvider<VesselCallEntity>
{
    //Task<IEnumerable<string>> GetTerminalNames();
    Task<IEnumerable<string>> GetPODs();
}
