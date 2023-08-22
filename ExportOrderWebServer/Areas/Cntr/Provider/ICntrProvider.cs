namespace ExportOrderWebServer.Areas.Cntr.Provider;

public interface ICntrProvider : IEntityProvider<CntrEntity>
{
    Task<IEnumerable<string>> GetCntrNums();
    Task<IEnumerable<string?>> GetCntrTypes();
}
