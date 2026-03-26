namespace IstanbulSenin.BLL.Services.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardDataAsync();
        Task<DashboardDto> GetDashboardDataByDateRangeAsync(DateTime startDate, DateTime endDate);
        void InvalidateDashboardCache();
    }

    public class DashboardDto
    {
        public int TotalNotifications { get; set; }

        public int SentNotifications { get; set; }

        public int PendingNotifications { get; set; }

        public int SuccessfulLogs { get; set; }

        public int FailedLogs { get; set; }

        public int TestLogs { get; set; }

        public decimal SuccessRate { get; set; }

        public List<DailyStatistics> DailyStats { get; set; } = new();

        public List<RecentNotificationDto> RecentNotifications { get; set; } = new();

        public Dictionary<string, int> TargetAudienceDistribution { get; set; } = new();

        // ========== PANEL GENEL İSTATİSTİKLERİ ==========
        public int TotalAdminUsers { get; set; }

        public int TotalSections { get; set; }

        public int TotalMiniApps { get; set; }
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
