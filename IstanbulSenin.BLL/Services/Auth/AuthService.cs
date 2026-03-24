using IstanbulSenin.CORE.Entities;
using Microsoft.AspNetCore.Identity;

namespace IstanbulSenin.BLL.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly SignInManager<AppUser> _signInManager;

        public AuthService(SignInManager<AppUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public async Task<bool> LoginAsync(string email, string password, bool rememberMe)
        {
            // E-posta ile giriş: UserName olarak e-posta kullanılıyor.
            var result = await _signInManager.PasswordSignInAsync(
                userName: email,
                password: password,
                isPersistent: rememberMe,
                lockoutOnFailure: true);

            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
