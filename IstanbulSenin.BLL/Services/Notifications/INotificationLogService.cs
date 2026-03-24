using IstanbulSenin.CORE.Entities;

namespace IstanbulSenin.BLL.Services.Notifications
{
    /// <summary>
    /// Bildirim gönderme geçmişini log tutmak için servis
    /// </summary>
    public interface INotificationLogService
    {
        /// <summary>
        /// Tüm log kayıtlarını getir
        /// </summary>
        Task<List<NotificationLog>> GetAllAsync();

        /// <summary>
        /// Belirli bildirimin log kayıtlarını getir
        /// </summary>
        Task<List<NotificationLog>> GetByNotificationIdAsync(int notificationId);

        /// <summary>
        /// Başarılı gönderilen log kaydı oluştur
        /// </summary>
        Task LogSuccessAsync(int notificationId, string targetAudience, int? recipientCount = null);

        /// <summary>
        /// Başarısız gönderilen log kaydı oluştur
        /// </summary>
        Task LogFailureAsync(int notificationId, string targetAudience, string errorMessage);

        /// <summary>
        /// Test (simüle edilmiş) gönderilen log kaydı oluştur
        /// </summary>
        Task LogTestAsync(int notificationId, string targetAudience);

        /// <summary>
        /// Eski log kayıtlarını temizle (isteğe bağlı)
        /// </summary>
        Task DeleteOlderLogsAsync(int daysOld);
    }
}
