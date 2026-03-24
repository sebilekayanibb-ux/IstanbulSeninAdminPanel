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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsSent { get; set; }
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Bu bildirimin gönderme geçmişi (log kayıtları)
        /// </summary>
        public List<NotificationLog> Logs { get; set; } = new();
    }
}
