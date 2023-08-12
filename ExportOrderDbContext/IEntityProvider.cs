
namespace ExportOrderDbContext;

public interface IEntityProvider<T>
{
    Task<AppObjectResponse> GetItemsAsync();
    Task<AppObjectResponse> GetItemsAsync(object parameters);
    Task<AppObjectResponse> GetItemAsync(Guid id);
    Task<AppObjectResponse> ModifyItemAsync(T item);
    Task<AppObjectResponse> NewItemAsync(T item);
    Task<AppObjectResponse> RemoveItemAsync(T item);
}
