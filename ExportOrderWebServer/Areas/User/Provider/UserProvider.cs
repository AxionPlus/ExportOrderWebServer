using Microsoft.AspNetCore.Identity;

namespace ExportOrderWebServer.Areas.User.Provider;

public class UserProvider : IUserProvider
{
    private readonly IDbContextFactory<ApplicationDbContext> _dbContext;    
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly RoleManager<ApplicationRole> RoleManager;

    public UserProvider(IDbContextFactory<ApplicationDbContext> dbContext, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
    {
        _dbContext = dbContext;
        UserManager = userManager;
        RoleManager = roleManager;
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            return await db.Users.ToListAsync();
        }
    }

    public async Task<IEnumerable<ApplicationUserDTO>> GetUsersDTOAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var result = new List<ApplicationUserDTO>();
            var users = await db.Users.AsNoTracking().ToListAsync();
            var roles = await db.Roles.AsNoTracking().ToListAsync();
            var userRoles = await db.UserRoles.AsNoTracking().ToListAsync();

            foreach (var user in users)
            {
                var userRoleList = new List<string>();
                var userRolesId = userRoles.Where(s => s.UserId == user.Id).Select(s => s.RoleId).ToList();

                foreach (var userRoleId in userRolesId)
                {
                    var roleName = roles.FirstOrDefault(s => s.Id == userRoleId)!.Name;
                    userRoleList.Add(roleName!);
                }

                var item = new ApplicationUserDTO()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Roles = userRoleList
                };
            }

            return result;
        }
    }

    public async Task<ApplicationUser> GetUserAsync(string Name)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var users = await db.Users.AsNoTracking().ToListAsync();
            return users.Where(s => s.UserName!.Contains(Name)).FirstOrDefault()!;
        }
    }

    public async Task<IEnumerable<string>> GetRolesAsync()
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            var roles = await db.Roles.AsNoTracking().ToListAsync();
            return roles.Select(s => s.Name).ToList()!;
        }
    }

    public async Task<bool> RolesToUserAsync(ApplicationUser User, IEnumerable<string> Roles)
    {
        using (var _db = _dbContext.CreateDbContextAsync())
        {
            var db = await _db;
            try
            {
                var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == User.Id);
                var roles = await db.Roles.AsNoTracking().ToListAsync();
                if (user is null)
                    return false;
                var userRoles = await UserManager.GetRolesAsync(user);

                foreach (var role in Roles)
                    if (!userRoles.Any(s => s == role))
                    {
                        var userRole = new IdentityUserRole<string>() { UserId = User.Id, RoleId = roles.FirstOrDefault(x => x.Name == role)!.Id };
                        db.Set<IdentityUserRole<string>>().Add(userRole);
                    }

                foreach (var role in userRoles)
                    if (!Roles.Any(s => s == role))
                    {
                        var RoleId = roles.FirstOrDefault(x => x.Name == role)!.Id;
                        var userRole = await db.Set<IdentityUserRole<string>>().FirstOrDefaultAsync(s => s.UserId == User.Id && s.RoleId == RoleId);
                        db.Set<IdentityUserRole<string>>().Remove(userRole!);
                    }

                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public async Task AddRoleAsync(string roleName)
    {
        var roleResult = await RoleManager.CreateAsync(new ApplicationRole() { Name = roleName });
    }

    public async Task RemoveRolesAsync(string roleName)
    {
        var role = await RoleManager.FindByNameAsync(roleName);
        if (role is not null)
            await RoleManager.DeleteAsync(role);
    }    
}
