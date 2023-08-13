
namespace ExportOrderDbContext;

public interface IEntityProvider<T>
{
    Task<AppObjectResponse> GetItemsAsync();
    Task<AppObjectResponse> GetItemsAsync(object parameters);
    Task<AppObjectResponse> GetItemAsync(long id);
    Task<AppObjectResponse> ModifyItemAsync(T item);
    Task<AppObjectResponse> NewItemAsync(T item);
    Task<AppObjectResponse> RemoveItemAsync(T item);

    Task<IEnumerable<string>> GetSearchNames();
    Task<IEnumerable<string>> GetSearchNamesEn();
}
