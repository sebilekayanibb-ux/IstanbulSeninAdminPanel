using Microsoft.Extensions.Logging;

namespace IstanbulSenin.BLL.Services.Notifications
{
    /// <summary>
    /// Mock Push Notification Sender
    /// 
    /// Şu anda kullanılıyor - Firebase yerine mock gönderim
    /// Gerçek bir bildirim göndermiş gibi davranır ama Firebase'e bağlanmaz
    /// 
    /// İlerde FirebaseNotificationSender ile değiştirilecek
    /// </summary>
    public class MockNotificationSender : INotificationSendingService
    {
        private readonly ILogger<MockNotificationSender> _logger;

        public MockNotificationSender(ILogger<MockNotificationSender> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Mock: Bildirimi göndermiş gibi davran
        /// </summary>
        public async Task<(bool Success, string Error)> SendAsync(int notificationId, string targetTopic)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Mock sender: ID={notificationId}, Topic={targetTopic}");

                return await Task.FromResult((true, string.Empty));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mock notification gönderilemedi: {NotificationId}", notificationId);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// Test gönderimi - Development ortamı için
        /// </summary>
        public async Task<(bool Success, string Error)> SendTestAsync(int notificationId, string targetTopic)
        {
            _logger.LogInformation(
                "🧪 TEST MOCK NOTIFICATION: ID={NotificationId}, Topic={Topic}",
                notificationId,
                targetTopic);

            return await Task.FromResult((true, "Test bildirim mock'ta başarıyla gönderildi"));
        }
    }
}
