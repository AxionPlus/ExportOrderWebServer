namespace ExportOrderWebServer.Areas.Cntr.Provider;

public interface ICntrProvider : IEntityProvider<CntrEntity>
{
    Task<AppObjectResponse> GetItemAsync(string Num);
    Task<IEnumerable<string>> GetCntrNums();
    Task<IEnumerable<string>> GetCntrTypeNames();
    Task<IEnumerable<CntrTpSz>> GetCntrTypes();
}
