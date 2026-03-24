using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace IstanbulSenin.BLL.Services.Notifications
{
    public class NotificationLogService : INotificationLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationLogService> _logger;

        public NotificationLogService(IUnitOfWork unitOfWork, ILogger<NotificationLogService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<NotificationLog>> GetAllAsync()
        {
            var logs = await _unitOfWork.NotificationLogs.Query()
                .Include(x => x.Notification)
                .OrderByDescending(x => x.SentAt)
                .ToListAsync();
            return logs;
        }

        public async Task<List<NotificationLog>> GetByNotificationIdAsync(int notificationId)
        {
            return await _unitOfWork.NotificationLogs.Query()
                .Include(x => x.Notification)
                .Where(x => x.NotificationId == notificationId)
                .OrderByDescending(x => x.SentAt)
                .ToListAsync();
        }

        public async Task LogSuccessAsync(int notificationId, string targetAudience, int? recipientCount = null)
        {
            var log = new NotificationLog
            {
                NotificationId = notificationId,
                Status = "Success",
                TargetAudience = targetAudience,
                RecipientCount = recipientCount,
                SentAt = DateTime.UtcNow,
                Metadata = $"{{\"method\": \"firebase\", \"timestamp\": \"{DateTime.UtcNow:O}\"}}"
            };

            await _unitOfWork.NotificationLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("✓ Bildirim log kaydı oluşturuldu: ID={NotificationId}, Status=Success", notificationId);
        }

        public async Task LogFailureAsync(int notificationId, string targetAudience, string errorMessage)
        {
            var log = new NotificationLog
            {
                NotificationId = notificationId,
                Status = "Failed",
                TargetAudience = targetAudience,
                ErrorMessage = errorMessage,
                SentAt = DateTime.UtcNow,
                Metadata = $"{{\"error\": \"{errorMessage}\", \"timestamp\": \"{DateTime.UtcNow:O}\"}}"
            };

            await _unitOfWork.NotificationLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogWarning("✗ Bildirim log kaydı oluşturuldu: ID={NotificationId}, Status=Failed, Error={Error}",
                notificationId, errorMessage);
        }

        public async Task LogTestAsync(int notificationId, string targetAudience)
        {
            var log = new NotificationLog
            {
                NotificationId = notificationId,
                Status = "Test",
                TargetAudience = targetAudience,
                SentAt = DateTime.UtcNow,
                Metadata = $"{{\"mode\": \"test\", \"note\": \"Firebase entegrasyonu bekleniyor\", \"timestamp\": \"{DateTime.UtcNow:O}\"}}"
            };

            await _unitOfWork.NotificationLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("ℹ Bildirim test log kaydı oluşturuldu: ID={NotificationId}, Status=Test", notificationId);
        }

        public async Task DeleteOlderLogsAsync(int daysOld)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var logsToDelete = _unitOfWork.NotificationLogs.Query()
                .Where(x => x.SentAt < cutoffDate)
                .ToList();

            if (logsToDelete.Any())
            {
                _unitOfWork.NotificationLogs.DeleteRange(logsToDelete);
                await _unitOfWork.SaveChangesAsync();
            }

            _logger.LogInformation("✓ {Count} eski bildirim log kaydı silindi", logsToDelete.Count);
        }
    }
}
