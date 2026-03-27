namespace IstanbulSenin.BLL.Services.Notifications
{
    /// <summary>
    /// Push notification gönderme servisinin ara yüzü
    /// 
    /// Şu an Mock implementation kullanılıyor
    /// İlerde Firebase entegrasyonunda gerçek implementation yazılacak
    /// Interface aynı kalacak, sadece implementation değişecek
    /// </summary>
    public interface INotificationSendingService
    {
        /// <summary>
        /// Bildirimi belirtilen topic'e gönder
        /// </summary>
        /// <param name="notificationId">Bildirim ID</param>
        /// <param name="targetTopic">Target topic: "all-users", "test-only", "guests", "regular-users"</param>
        /// <returns>(Success, ErrorMessage)</returns>
        Task<(bool Success, string Error)> SendAsync(int notificationId, string targetTopic);

        /// <summary>
        /// Test gönderimi (Firebase'e gerçek göndermiyor, sadece log tutuyor)
        /// </summary>
        Task<(bool Success, string Error)> SendTestAsync(int notificationId, string targetTopic);
    }
}
