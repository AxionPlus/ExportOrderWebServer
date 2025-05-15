namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public interface IMyCompanyProvider
{
    Task<AppObjectResponse> GetLastItemAsync();
    Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item);
    Task<AppObjectResponse> ModifyItemAsync(MyCompanyEntity item);
    Task<AppObjectResponse> GetPersonItemAsync(long id);
    Task<AppObjectResponse> ModifyPersonItemAsync(PersonEntity item);
}