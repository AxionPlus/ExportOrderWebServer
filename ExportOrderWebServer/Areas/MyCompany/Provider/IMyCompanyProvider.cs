using ExportOrderEntites.MyCompany;

namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public interface IMyCompanyProvider
{
    Task<AppObjectResponse> GetItemAsync(uint id);
    Task<bool> IsItemNullAsync();
    Task<AppObjectResponse> GetLastItemAsync();
    Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item);
    Task<AppObjectResponse> ModifyItemAsync(MyCompanyEntity item);
}