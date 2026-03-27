namespace IstanbulSenin.CORE.Entities
{
    /// <summary>
    /// Test bildirimleri almak için kayıtlı cihazlar
    /// </summary>
    public class TestDevice
    {
        public int Id { get; set; }

        /// <summary>
        /// Firebase Device Token veya cihaz ID'si
        /// </summary>
        public string DeviceToken { get; set; } = string.Empty;

        /// <summary>
        /// Cihazın adı (örn: "Seçkin'in iPhone")
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Cihaz türü (iOS, Android, Web)
        /// </summary>
        public string DeviceType { get; set; } = "Android";  // Android, iOS, Web

        /// <summary>
        /// Cihaz aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Son token güncellenme tarihi
        /// </summary>
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Cihazı kaydeden admin
        /// </summary>
        public string? RegisteredBy { get; set; }  // Admin ID

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
