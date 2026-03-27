namespace IstanbulSenin.BLL.Services.Notifications
{
    /// <summary>
    /// API Response DTO - Mobil app ve 3. party entegrasyonlar için
    /// </summary>
    public class NotificationResponseDto
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Bildirim başlığı
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Bildirim içeriği
        /// </summary>
        public string Body { get; set; } = string.Empty;
        
        /// <summary>
        /// Hedef kitle: "all", "guest", "regular"
        /// </summary>
        public string TargetAudience { get; set; } = string.Empty;
        
        /// <summary>
        /// Gönderim durumu
        /// </summary>
        public bool IsSent { get; set; }
        
        /// <summary>
        /// Gönderim tarihi (UTC)
        /// </summary>
        public DateTime? SentAt { get; set; }
        
        /// <summary>
        /// Oluşturulma tarihi (UTC)
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Test modu mu?
        /// </summary>
        public bool IsTestMode { get; set; }
    }

    /// <summary>
    /// Bildirim gönderme isteği - Mobil app'dan POST isteği
    /// </summary>
    public class SendNotificationRequestDto
    {
        public int NotificationId { get; set; }
    }

    /// <summary>
    /// API Response - Standart format (tüm endpoint'ler)
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T? data, string message = "Başarılı")
            => new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> ErrorResponse(string message)
            => new() { Success = false, Message = message };
    }

    /// <summary>
    /// Hata detayları
    /// </summary>
    public class ErrorResponseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
    }
}
