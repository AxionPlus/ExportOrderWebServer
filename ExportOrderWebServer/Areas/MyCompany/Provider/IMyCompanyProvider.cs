using ExportOrderEntites.MyCompany;

namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public interface IMyCompanyProvider
{
    Task<AppObjectResponse> GetItemAsync(uint id);
    Task<bool> IsMyCompanyItemNullAsync();
    Task<uint> LastVersionMyCompanyAsync();
    Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item);
}