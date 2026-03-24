using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IstanbulSenin.CORE.Entities
{
    /// <summary>
    /// Bildirim gönderme geçmişini tutmak için log tablosu
    /// </summary>
    public class NotificationLog
    {
        public int Id { get; set; }

        /// <summary>
        /// Hangi bildirimlere ait olduğu
        /// </summary>
        [ForeignKey("Notification")]
        public int NotificationId { get; set; }
        public Notification? Notification { get; set; }

        /// <summary>
        /// Gönderme başlangıç zamanı
        /// </summary>
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gönderme durumu (Başarılı, Başarısız, Test vb.)
        /// </summary>
        [Required]
        public string Status { get; set; } = "Test";  // "Success", "Failed", "Test"

        /// <summary>
        /// Hata mesajı (başarısız olursa)
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Hangi hedef kitleye gönderildi
        /// </summary>
        [Required]
        public string TargetAudience { get; set; } = string.Empty;  // "all", "guest", "regular"

        /// <summary>
        /// Kaç kişiye gönderildi (Gelecek: Firebase entegre olunca)
        /// </summary>
        public int? RecipientCount { get; set; }

        /// <summary>
        /// Ek bilgiler (JSON formatında)
        /// </summary>
        public string? Metadata { get; set; }
    }
}
