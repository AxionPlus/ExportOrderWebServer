namespace ExportOrderWebServer.Areas.MyCompany.Provider;

public interface IMyCompanyProvider
{
    Task<AppObjectResponse> GetLastItemAsync();
    Task<AppObjectResponse> NewItemAsync(MyCompanyEntity item, string? UserName = "");
    Task<AppObjectResponse> ModifyItemAsync(MyCompanyEntity item, string? UserName = "");
    Task<AppObjectResponse> GetPersonItemAsync(long id);
    Task<AppObjectResponse> ModifyPersonItemAsync(PersonEntity item);
}