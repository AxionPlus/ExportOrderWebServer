namespace ExportOrderWebServer.Areas.Vessel.Provider;

public interface IVesselProvider : IEntityProvider<VesselEntity>
{
    Task<IEnumerable<string>> GetIMOnos();
}
