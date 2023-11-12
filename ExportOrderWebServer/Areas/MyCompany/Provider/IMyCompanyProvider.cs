using ExportOrderEntites.MyCompany;

namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public interface IMyCompanyProvider
{
    Task<AppObjectResponse> GetItemAsync(uint id);
    Task<bool> IsItemNullAsync();
    Task<AppObjectResponse> GetLastItemAsync();
    Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item);
    Task<AppObjectResponse> ModifyItemAsync(MyCompanyEntity item);
    Task<AppObjectResponse> GetPersonItemAsync(long id);
    Task<AppObjectResponse> NewPersonItemAsync(PersonEntity item);
    Task<AppObjectResponse> ModifyPersonItemAsync(PersonEntity item);
}