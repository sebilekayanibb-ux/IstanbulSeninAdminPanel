using System.ComponentModel.DataAnnotations;

namespace IstanbulSenin.CORE.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Bildirim başlığı zorunludur.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bildirim içeriği zorunludur.")]
        public string Body { get; set; } = string.Empty;

        // "all" = herkes, "guest" = misafir, "regular" = kayıtlı kullanıcı
        public string TargetAudience { get; set; } = "all";

        /// Test modunda gönderilir mi? (true = Test, false = Gerçek Gönderim)
        public bool IsTestMode { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }

        /// Bu bildirimin gönderme geçmişi (log kayıtları)
        public List<NotificationLog> Logs { get; set; } = new();
    }
}
