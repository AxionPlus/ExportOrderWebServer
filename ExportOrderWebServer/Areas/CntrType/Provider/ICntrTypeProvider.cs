namespace ExportOrderWebServer.Areas.Cntr.Provider;

public interface ICntrTypeProvider
{
    Task<AppObjectResponse> GetItemsAsync();
    Task<AppObjectResponse> NewItemsAsync(List<CntrTpSz> NewItems, string? UserName = "");
    Task<AppObjectResponse> RemoveItemAsync(CntrTpSz item);
    Task<IEnumerable<CntrTpSz>> GetCntrTypes();
}
