namespace IstanbulSenin.BLL.Services.Auth
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password, bool rememberMe);
        Task LogoutAsync();
    }
}
