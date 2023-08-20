using ExportOrderEntites.DTO;

namespace ExportOrderWebServer.Areas.User.Provider;

public interface IUserProvider
{
    Task<IEnumerable<ApplicationUser>> GetUsersAsync();
    Task<IEnumerable<ApplicationUserDTO>> GetUsersDTOAsync();
    Task<ApplicationUser> GetUserAsync(string Name);
    Task<IEnumerable<string>> GetRolesAsync();
    Task AddRoleAsync(string roleName);
    Task RemoveRolesAsync(string roleName);
    Task<bool> RolesToUserAsync(ApplicationUser User, IEnumerable<string> Roles);
}
