using IstanbulSenin.CORE.Repositories;
using IstanbulSenin.HELPER;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IstanbulSenin.BLL.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            var today = DateTimeHelper.GetTurkeyNow().Date;
            var sevenDaysAgo = today.AddDays(-6);
            
            return await GetDashboardDataByDateRangeAsync(sevenDaysAgo, today.AddDays(1));
        }

        public async Task<DashboardDto> GetDashboardDataByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var dto = new DashboardDto();

                // Tüm bildirimler
                var allNotifications = await _unitOfWork.Notifications.GetAllAsync();

                dto.TotalNotifications = allNotifications.Count;
                dto.SentNotifications = allNotifications.Count(n => n.IsSent);
                dto.PendingNotifications = allNotifications.Count(n => !n.IsSent);

                // Tüm loglar
                var allLogs = await _unitOfWork.NotificationLogs.Query()
                    .Include(l => l.Notification)
                    .ToListAsync();

                dto.SuccessfulLogs = allLogs.Count(l => l.Status == "Success");
                dto.FailedLogs = allLogs.Count(l => l.Status == "Failed");
                dto.TestLogs = allLogs.Count(l => l.Status == "Test");

                // Başarı oranı
                var totalLogs = dto.SuccessfulLogs + dto.FailedLogs;
                dto.SuccessRate = totalLogs > 0 
                    ? Math.Round((decimal)dto.SuccessfulLogs / totalLogs * 100, 2) 
                    : 0;

                // Günlük istatistikler (son 7 gün)
                dto.DailyStats = GetDailyStatistics(allLogs, startDate, endDate);

                // Son gönderilen bildirimler (10 adet)
                dto.RecentNotifications = allNotifications
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(10)
                    .Select(n => new RecentNotificationDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        TargetAudience = n.TargetAudience,
                        IsSent = n.IsSent,
                        CreatedAt = n.CreatedAt,
                        SentAt = n.SentAt
                    })
                    .ToList();

                // Hedef kitle dağılımı
                dto.TargetAudienceDistribution = allNotifications
                    .GroupBy(n => n.TargetAudience)
                    .ToDictionary(
                        g => GetAudienceLabel(g.Key),
                        g => g.Count()
                    );

                _logger.LogInformation("Dashboard verileri başarıyla hesaplandı");
                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard verileri hesaplanırken hata oluştu");
                throw;
            }
        }

        private List<DailyStatistics> GetDailyStatistics(
            List<CORE.Entities.NotificationLog> logs,
            DateTime startDate,
            DateTime endDate)
        {
            var stats = new List<DailyStatistics>();

            for (var date = startDate.Date; date < endDate.Date; date = date.AddDays(1))
            {
                var count = logs.Count(l => 
                    l.SentAt.Date == date && l.Status == "Success");
                
                stats.Add(new DailyStatistics
                {
                    Date = date.ToString("dd.MM"),
                    Count = count
                });
            }

            return stats;
        }

        private string GetAudienceLabel(string audience)
        {
            return audience switch
            {
                "guest" => "Misafirler",
                "regular" => "Kayıtlı Kullanıcılar",
                "all" => "Herkes",
                _ => audience
            };
        }
    }
}
