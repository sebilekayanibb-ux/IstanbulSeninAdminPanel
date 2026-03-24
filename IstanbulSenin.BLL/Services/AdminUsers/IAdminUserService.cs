using IstanbulSenin.CORE.Entities;

namespace IstanbulSenin.BLL.Services.AdminUsers
{
    public interface IAdminUserService
    {
        Task<List<AppUser>> GetAllUsersAsync();
        Task<AppUser?> GetUserByIdAsync(string id);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<(bool Success, string Error)> CreateUserAsync(string fullName, string email, string password, List<string> roles);
        Task<(bool Success, string Error)> UpdateUserAsync(string id, string fullName, string email, string? newPassword, List<string> roles);
        Task<(bool Success, string Error)> DeleteUserAsync(string id);
    }
}
