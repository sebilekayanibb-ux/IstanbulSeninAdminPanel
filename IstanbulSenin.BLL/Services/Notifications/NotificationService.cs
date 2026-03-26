using IstanbulSenin.BLL.Services.Dashboard;
using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Repositories;
using IstanbulSenin.HELPER;
using Microsoft.Extensions.Logging;

namespace IstanbulSenin.BLL.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationLogService _logService;
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            INotificationLogService logService,
            IDashboardService dashboardService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _logService = logService;
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public async Task<List<Notification>> GetAllAsync()
        {
            try
            {
                var notifications = await _unitOfWork.Notifications.GetAllAsync();
                return notifications
                    .OrderByDescending(n => n.CreatedAt)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm bildirimleri getirirken hata oluştu");
                throw;
            }
        }

        public async Task<(bool Success, string Error)> CreateAsync(Notification notification)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(notification.Title))
                    return (false, "Başlık boş olamaz");

                if (string.IsNullOrWhiteSpace(notification.Body))
                    return (false, "İçerik boş olamaz");

                if (string.IsNullOrWhiteSpace(notification.TargetAudience))
                    return (false, "Hedef kitle boş olamaz");

                notification.CreatedAt = DateTimeHelper.GetTurkeyNow();
                notification.IsSent = false;

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Dashboard cache'i temizle - Yeni veriler hesaplanacak!
                _dashboardService.InvalidateDashboardCache();

                _logger.LogInformation("Bildirim oluşturuldu: {NotificationTitle}", notification.Title);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim oluştururken hata oluştu");
                return (false, "Bildirim oluştururken bir hata oluştu: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Error)> UpdateAsync(int id, Notification notification)
        {
            try
            {
                var existing = await _unitOfWork.Notifications.GetByIdAsync(id);
                if (existing == null)
                    return (false, "Bildirim bulunamadı");

                if (existing.IsSent)
                    return (false, "Gönderilmiş bildirimi düzenlenemez");

                if (string.IsNullOrWhiteSpace(notification.Title))
                    return (false, "Başlık boş olamaz");

                if (string.IsNullOrWhiteSpace(notification.Body))
                    return (false, "İçerik boş olamaz");

                if (string.IsNullOrWhiteSpace(notification.TargetAudience))
                    return (false, "Hedef kitle boş olamaz");

                existing.Title = notification.Title;
                existing.Body = notification.Body;
                existing.TargetAudience = notification.TargetAudience;

                _unitOfWork.Notifications.Update(existing);
                await _unitOfWork.SaveChangesAsync();

                // ✅ Dashboard cache'i temizle - Güncellenmiş veriler hesaplanacak!
                _dashboardService.InvalidateDashboardCache();

                _logger.LogInformation("Bildirim güncellendi: {NotificationId} - {NotificationTitle}", id, existing.Title);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim güncellenirken hata oluştu");
                return (false, "Bildirim güncellenirken bir hata oluştu: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Error)> SendAsync(int id)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
                if (notification == null)
                    return (false, "Bildirim bulunamadı");

                if (notification.IsSent)
                    return (false, "Bu bildirim zaten gönderilmiş");

                try
                {
                    // ✅ Başarılı gönderim log kaydı ("Success" status)
                    var successLog = new NotificationLog
                    {
                        NotificationId = notification.Id,
                        Status = "Success",  // ← "Test" yerine "Success"!
                        TargetAudience = notification.TargetAudience,
                        RecipientCount = 0,
                        ErrorMessage = null,
                        SentAt = DateTimeHelper.GetTurkeyNow()
                    };

                    await _unitOfWork.NotificationLogs.AddAsync(successLog);

                    notification.IsSent = true;
                    notification.SentAt = DateTimeHelper.GetTurkeyNow();
                    _unitOfWork.Notifications.Update(notification);

                    await _unitOfWork.SaveChangesAsync();

                    // ✅ Dashboard cache'i temizle - Gönderim durumu değişti!
                    _dashboardService.InvalidateDashboardCache();

                    _logger.LogInformation(
                        "Bildirim gönderildi (Success): {NotificationId} - {Title}",
                        notification.Id,
                        notification.Title);

                    return (true, string.Empty);
                }
                catch (Exception sendEx)
                {
                    _logger.LogError(sendEx, "Bildirim gönderilemedi: {NotificationId}", id);

                    try
                    {
                        var failureLog = new NotificationLog
                        {
                            NotificationId = notification.Id,
                            Status = "Failure",
                            TargetAudience = notification.TargetAudience,
                            RecipientCount = 0,
                            ErrorMessage = sendEx.Message,
                            SentAt = DateTime.UtcNow
                        };

                        await _unitOfWork.NotificationLogs.AddAsync(failureLog);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    catch (Exception logEx)
                    {
                        _logger.LogError(logEx, "Başarısızlık log'u kaydedilirken hata oluştu");
                    }

                    return (false, "Bildirim gönderilemedi: " + sendEx.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderme işleminde beklenmeyen hata: {NotificationId}", id);
                return (false, "Bildirim gönderme işleminde hata oluştu: " + ex.Message);
            }
        }

        public async Task<(bool Success, string Error)> DeleteAsync(int id)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
                if (notification == null)
                    return (false, "Bildirim bulunamadı");

                if (notification.IsSent)
                    return (false, "Gönderilmiş bildirimi silemezsiniz");

                _unitOfWork.Notifications.Delete(notification);
                await _unitOfWork.SaveChangesAsync();

                _dashboardService.InvalidateDashboardCache();

                _logger.LogInformation("Bildirim silindi: {NotificationId}", id);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim silinirken hata oluştu: {NotificationId}", id);
                return (false, "Bildirim silinirken hata oluştu: " + ex.Message);
            }
        }
    }
}
