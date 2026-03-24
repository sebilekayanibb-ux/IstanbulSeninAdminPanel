namespace IstanbulSenin.BLL.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
        Task<DashboardDto> GetDashboardDataByDateRangeAsync(DateTime startDate, DateTime endDate);
    }

    public class DashboardDto
    {
        /// <summary>Toplam bildirim sayısı</summary>
        public int TotalNotifications { get; set; }

        /// <summary>Gönderilmiş bildirim sayısı</summary>
        public int SentNotifications { get; set; }

        /// <summary>Beklemede olan bildirim sayısı</summary>
        public int PendingNotifications { get; set; }

        /// <summary>Başarılı gönderim sayısı</summary>
        public int SuccessfulLogs { get; set; }

        /// <summary>Başarısız gönderim sayısı</summary>
        public int FailedLogs { get; set; }

        /// <summary>Test gönderim sayısı</summary>
        public int TestLogs { get; set; }

        /// <summary>Başarı oranı (%)</summary>
        public decimal SuccessRate { get; set; }

        /// <summary>Son 7 günün günlük gönderim sayıları</summary>
        public List<DailyStatistics> DailyStats { get; set; } = new();

        /// <summary>Son gönderilen bildirimler</summary>
        public List<RecentNotificationDto> RecentNotifications { get; set; } = new();

        /// <summary>Hedef kitle dağılımı</summary>
        public Dictionary<string, int> TargetAudienceDistribution { get; set; } = new();
    }

    public class DailyStatistics
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class RecentNotificationDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TargetAudience { get; set; }
        public bool IsSent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
