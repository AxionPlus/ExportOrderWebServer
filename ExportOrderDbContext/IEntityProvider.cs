namespace ExportOrderDbContext;

public interface IEntityProvider<T>
{
    Task<AppObjectResponse> GetItemsAsync();
    Task<AppObjectResponse> GetItemsAsync(FilterParameters filter);
    Task<AppObjectResponse> GetItemAsync(long id);
    Task<AppObjectResponse> ModifyItemAsync(T item, string? UserName = "");
    Task<AppObjectResponse> NewItemAsync(T item, string? UserName = "");
    Task<AppObjectResponse> RemoveItemAsync(long id, string? UserName = "");

    Task<IEnumerable<string>> GetNames();
    Task<IEnumerable<string>> GetNamesEn();
}
