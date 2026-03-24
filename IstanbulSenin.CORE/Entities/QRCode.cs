namespace IstanbulSenin.CORE.Entities
{
    public class QRCode
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string UserId { get; set; }
        public AppUser? User { get; set; }

        public string Description { get; set; }

        public QRCodeStatus Status { get; set; } = QRCodeStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        public string? UsedByIp { get; set; }

        public string? CreatedByAdmin { get; set; }
    }

    public enum QRCodeStatus
    {
        Active = 1,
        Used = 2,
        Expired = 3,
        Cancelled = 4
    }
}
