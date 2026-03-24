using IstanbulSenin.CORE.Entities;

namespace IstanbulSenin.BLL.Services.QRCodes
{
    public interface IQRCodeService
    {
        Task<List<QRCode>> GetAllAsync();
        Task<QRCode?> GetByIdAsync(int id);
        Task<QRCode?> GetByCodeAsync(string code);
        Task<(bool Success, string Error)> CreateAsync(string userId, DateTime expiresAt, string description, string? createdByAdmin = null);
        Task<(bool Success, string Error)> ValidateAndUseAsync(string code, string ipAddress);
        Task<(bool Success, string Error)> CancelAsync(int id);
        Task<List<QRCode>> GetExpiredAsync();
        Task<List<QRCode>> GetUserCodesAsync(string userId);
    }
}
