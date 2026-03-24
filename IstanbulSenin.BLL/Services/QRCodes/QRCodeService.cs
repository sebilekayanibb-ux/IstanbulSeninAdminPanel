using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Repositories;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace IstanbulSenin.BLL.Services.QRCodes
{
    public class QRCodeService : IQRCodeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<QRCodeService> _logger;

        public QRCodeService(IUnitOfWork unitOfWork, ILogger<QRCodeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<QRCode>> GetAllAsync()
        {
            return await _unitOfWork.QRCodes.GetAllAsync();
        }

        public async Task<QRCode?> GetByIdAsync(int id)
        {
            return await _unitOfWork.QRCodes.GetByIdAsync(id);
        }

        public async Task<QRCode?> GetByCodeAsync(string code)
        {
            return await _unitOfWork.QRCodes.Query()
                .FirstOrDefaultAsync(q => q.Code == code);
        }

        public async Task<(bool Success, string Error)> CreateAsync(string userId, DateTime expiresAt, string description, string? createdByAdmin = null)
        {
            try
            {
                // Validasyon
                if (expiresAt <= DateTime.UtcNow)
                    return (false, "Bitiş tarihi gelecek bir zaman olmalıdır.");

                if (string.IsNullOrWhiteSpace(description))
                    return (false, "Açıklama boş olamaz.");

                // Benzersiz kod oluştur
                string code = GenerateUniqueCode();

                // Kullanıcı kontrol et
                var user = await _unitOfWork.Query<AppUser>()
                    .Where(u => u.Id == userId)
                    .FirstOrDefaultAsync();

                if (user == null)
                    return (false, "Kullanıcı bulunamadı.");

                var qrCode = new QRCode
                {
                    Code = code,
                    UserId = userId,
                    Description = description,
                    Status = QRCodeStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsUsed = false,
                    CreatedByAdmin = createdByAdmin
                };

                await _unitOfWork.QRCodes.AddAsync(qrCode);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"QR kod oluşturuldu. Code: {code}, UserId: {userId}, CreatedBy: {createdByAdmin}");
                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError($"QR kod oluşturma hatası: {ex.Message}");
                return (false, "QR kod oluşturulamadı.");
            }
        }

        public async Task<(bool Success, string Error)> ValidateAndUseAsync(string code, string ipAddress)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code))
                    return (false, "QR kod boş olamaz.");

                var qrCode = await GetByCodeAsync(code);

                if (qrCode == null)
                    return (false, "QR kod bulunamadı.");

                // Durum kontrolleri
                if (qrCode.Status == QRCodeStatus.Used)
                    return (false, "Bu QR kod zaten kullanılmıştır.");

                if (qrCode.Status == QRCodeStatus.Expired)
                    return (false, "Bu QR kodun süresi dolmuştur.");

                if (qrCode.Status == QRCodeStatus.Cancelled)
                    return (false, "Bu QR kod iptal edilmiştir.");

                // Zaman kontrolü
                if (DateTime.UtcNow > qrCode.ExpiresAt)
                {
                    qrCode.Status = QRCodeStatus.Expired;
                    await _unitOfWork.SaveChangesAsync();
                    return (false, "QR kodun süresi dolmuştur.");
                }

                // Kullanılan olarak işaretle
                qrCode.IsUsed = true;
                qrCode.UsedAt = DateTime.UtcNow;
                qrCode.UsedByIp = ipAddress;
                qrCode.Status = QRCodeStatus.Used;

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"QR kod kullanıldı. Code: {code}, UserId: {qrCode.UserId}, IP: {ipAddress}");
                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError($"QR kod doğrulama hatası: {ex.Message}");
                return (false, "QR kod doğrulanırken hata oluştu.");
            }
        }

        public async Task<(bool Success, string Error)> CancelAsync(int id)
        {
            try
            {
                var qrCode = await GetByIdAsync(id);

                if (qrCode == null)
                    return (false, "QR kod bulunamadı.");

                if (qrCode.Status == QRCodeStatus.Used)
                    return (false, "Kullanılmış QR kod iptal edilemez.");

                qrCode.Status = QRCodeStatus.Cancelled;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"QR kod iptal edildi. Id: {id}, Code: {qrCode.Code}");
                return (true, "");
            }
            catch (Exception ex)
            {
                _logger.LogError($"QR kod iptal hatası: {ex.Message}");
                return (false, "QR kod iptal edilemedi.");
            }
        }

        public async Task<List<QRCode>> GetExpiredAsync()
        {
            var now = DateTime.UtcNow;
            return await _unitOfWork.QRCodes.Query()
                .Where(q => q.ExpiresAt < now && q.Status != QRCodeStatus.Expired && q.Status != QRCodeStatus.Cancelled)
                .ToListAsync();
        }

        public async Task<List<QRCode>> GetUserCodesAsync(string userId)
        {
            return await _unitOfWork.QRCodes.Query()
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }

        private string GenerateUniqueCode()
        {
            // 12 karakterli benzersiz kod oluştur (örnek: QR-ABC123XYZ789)
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[9];
                rng.GetBytes(data);

                StringBuilder builder = new StringBuilder("QR-");
                foreach (byte b in data)
                {
                    builder.Append(chars[b % chars.Length]);
                }

                return builder.ToString();
            }
        }
    }
}
