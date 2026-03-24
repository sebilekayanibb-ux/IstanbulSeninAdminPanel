using IstanbulSenin.BLL.Services.AdminUsers;
using IstanbulSenin.BLL.Services.Auth;
using IstanbulSenin.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IstanbulSenin.MVC.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class AdminUserController : Controller
    {
        private readonly IAdminUserService _adminUserService;
        private readonly IAuthService _authService;

        public AdminUserController(IAdminUserService adminUserService, IAuthService authService)
        {
            _adminUserService = adminUserService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _adminUserService.GetAllUsersAsync();
            var viewModel = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _adminUserService.GetUserRolesAsync(user.Id);
                viewModel.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    Roles = roles,
                    CreatedAt = user.CreatedAt
                });
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create() => View(new CreateUserViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _adminUserService.CreateUserAsync(
                model.FullName, model.Email, model.Password, model.Roles);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["Success"] = $"'{model.FullName}' kullanıcısı başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _adminUserService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _adminUserService.GetUserRolesAsync(id);
            return View(new EditUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Roles = roles,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            ModelState.Remove(nameof(model.NewPassword));
            ModelState.Remove(nameof(model.NewPasswordConfirm));

            if (!ModelState.IsValid) return View(model);

            var (success, error) = await _adminUserService.UpdateUserAsync(
                model.Id, model.FullName, model.Email, model.NewPassword, model.Roles);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }

            TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            // HTTP oturum yönetimi controller'ın kaygısı — kimin sildiğini burada biliyoruz
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isSelf = id == currentUserId;

            var (success, error) = await _adminUserService.DeleteUserAsync(id);

            if (!success)
            {
                TempData["Error"] = error;
                return RedirectToAction(nameof(Index));
            }

            // Kullanıcı kendi hesabını sildiyse oturumu kapat → login'e yönlendir
            if (isSelf)
            {
                await _authService.LogoutAsync();
                return RedirectToAction("Login", "Account");
            }

            TempData["Success"] = "Kullanıcı başarıyla silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
