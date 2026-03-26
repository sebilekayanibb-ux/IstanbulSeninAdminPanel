using IstanbulSenin.CORE.Entities;
using IstanbulSenin.HELPER.Constants;
using IstanbulSenin.BLL.Services.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace IstanbulSenin.BLL.Services.AdminUsers
{
    public class AdminUserService : IAdminUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IDashboardService dashboardService,
            ILogger<AdminUserService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public Task<List<AppUser>> GetAllUsersAsync()
        {
            return Task.FromResult(_userManager.Users.OrderBy(u => u.FullName).ToList());
        }

        public async Task<AppUser?> GetUserByIdAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new();
            return (await _userManager.GetRolesAsync(user)).ToList();
        }

        public async Task<(bool Success, string Error)> CreateUserAsync(
            string fullName, string email, string password, List<string> roles)
        {
            if (!email.EndsWith(AppConstants.EmailDomain, StringComparison.OrdinalIgnoreCase))
                return (false, $"E-posta adresi {AppConstants.EmailDomain} uzantılı olmalıdır.");

            if (roles == null || roles.Count == 0)
                return (false, "En az bir rol seçilmelidir.");

            var user = new AppUser
            {
                FullName = fullName,
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

            foreach (var role in roles)
                await _userManager.AddToRoleAsync(user, role);

            _dashboardService.InvalidateDashboardCache();
            _logger.LogInformation("✓ Kullanıcı oluşturuldu: {Email} - Roller: {Roles}", email, string.Join(", ", roles));
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> UpdateUserAsync(
            string id, string fullName, string email, string? newPassword, List<string> roles)
        {
            if (!email.EndsWith(AppConstants.EmailDomain, StringComparison.OrdinalIgnoreCase))
                return (false, $"E-posta adresi {AppConstants.EmailDomain} uzantılı olmalıdır.");

            if (roles == null || roles.Count == 0)
                return (false, "En az bir rol seçilmelidir.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return (false, "Kullanıcı bulunamadı.");

            // SuperAdmin rolü kaldırılıyorsa son SuperAdmin kontrolü yap
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Contains(AppConstants.Roles.SuperAdmin) && !roles.Contains(AppConstants.Roles.SuperAdmin))
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync(AppConstants.Roles.SuperAdmin);
                if (superAdmins.Count <= 1)
                    return (false, "Sistemde yalnızca bir SuperAdmin var. Rolü kaldırmadan önce başka bir SuperAdmin ekleyin.");
            }

            user.FullName = fullName;
            user.Email = email;
            user.UserName = email;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));

            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
                if (!pwResult.Succeeded)
                    return (false, string.Join(" ", pwResult.Errors.Select(e => e.Description)));
            }

            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            foreach (var role in roles)
                await _userManager.AddToRoleAsync(user, role);

            _dashboardService.InvalidateDashboardCache();
            return (true, string.Empty);
        }

        public async Task<(bool Success, string Error)> DeleteUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return (false, "Kullanıcı bulunamadı.");

            // Son SuperAdmin silinemez
            var isSuperAdmin = await _userManager.IsInRoleAsync(user, AppConstants.Roles.SuperAdmin);
            if (isSuperAdmin)
            {
                var superAdmins = await _userManager.GetUsersInRoleAsync(AppConstants.Roles.SuperAdmin);
                if (superAdmins.Count <= 1)
                    return (false, "Sistemde yalnızca bir SuperAdmin var. Silmeden önce başka bir SuperAdmin ekleyin.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _dashboardService.InvalidateDashboardCache();
                _logger.LogInformation("✓ Kullanıcı silindi: {Email}", user.Email);
                return (true, string.Empty);
            }
            return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
