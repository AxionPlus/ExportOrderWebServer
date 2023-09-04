namespace ExportOrderWebServer.Areas.Cntr.Provider;

public interface ICntrTypeProvider
{
    Task<AppObjectResponse> GetItemsAsync();
    Task<AppObjectResponse> NewItemsAsync(List<CntrTpSz> NewItems);
    Task<AppObjectResponse> RemoveItemAsync(CntrTpSz item);
    Task<IEnumerable<CntrTpSz>> GetCntrTypes();


    // NOT USED YET
    Task<CntrTpSz> GetCntrType(string type);
    Task<IEnumerable<string>> GetCntrNums();
    Task<IEnumerable<string>> GetCntrTypeNames();    
}
