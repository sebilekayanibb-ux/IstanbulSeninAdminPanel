using IstanbulSenin.CORE.Repositories;
using IstanbulSenin.HELPER;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace IstanbulSenin.BLL.Services.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;
        private readonly IMemoryCache _cache;

        // Cache key'leri
        private const string DashboardCacheKeyWeekly = "dashboard_stats_weekly";
        private const string DashboardCacheKeyPrefix = "dashboard_stats_";

        public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cache = cache;
        }

        // ✅ Cache invalidation - Yeni bildirim eklenince çağrılacak
        public void InvalidateDashboardCache()
        {
            _cache.Remove(DashboardCacheKeyWeekly);
            _logger.LogInformation("✓ Dashboard cache temizlendi - Yeni veriler hesaplanacak");
        }

        public async Task<DashboardDto> GetDashboardDataAsync()
        {
            const string cacheKey = DashboardCacheKeyWeekly;

            if (_cache.TryGetValue(cacheKey, out DashboardDto? cachedData))
            {
                _logger.LogInformation("✓ Dashboard verileri bellekten alındı (Cache)");
                return cachedData!;
            }

            var today = DateTimeHelper.GetTurkeyNow().Date;
            var sevenDaysAgo = today.AddDays(-6);

            var result = await GetDashboardDataByDateRangeAsync(sevenDaysAgo, today.AddDays(1));

            // Cache'e kaydet (5 dakika)
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, result, cacheOptions);
            _logger.LogInformation("✓ Dashboard verileri cache'e kaydedildi (5 dakika)");

            return result;
        }

        public async Task<DashboardDto> GetDashboardDataByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                _logger.LogInformation("→ Dashboard verileri hesaplanıyor... (Tarih: {StartDate} - {EndDate})", startDate, endDate);

                var dto = new DashboardDto();

                // ✅ 1. TOPLAM BİLDİRİM (SQL'de hesapla - bellek kullanma)
                // VERİLER SQL'DE HESAPLAMA VE FİLTRELEME YAPILDI BÖYLECE BELLEK DAHA AZ KULLANILDI.
                try
                {
                    var totalCount = await _unitOfWork.Notifications.Query()
                        .CountAsync();
                    dto.TotalNotifications = totalCount;

                    var sentCount = await _unitOfWork.Notifications.Query()
                        .CountAsync(n => n.IsSent);
                    dto.SentNotifications = sentCount;

                    var pendingCount = await _unitOfWork.Notifications.Query()
                        .CountAsync(n => !n.IsSent);
                    dto.PendingNotifications = pendingCount;

                    _logger.LogInformation("✓ Bildirim istatistikleri: Toplam={Total}, Gönderildi={Sent}, Beklemede={Pending}",
                        dto.TotalNotifications, dto.SentNotifications, dto.PendingNotifications);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Bildirim sayıları hesaplarken DB hatası");
                    throw new InvalidOperationException("Bildirim verileri erişilemedi", dbEx);
                }

                // ✅ 2. LOGLAR (Tarih filtresiyle - SQL'de)
                List<CORE.Entities.NotificationLog> filteredLogs = new();
                try
                {
                    filteredLogs = await _unitOfWork.NotificationLogs.Query()
                        .Where(l => l.SentAt >= startDate && l.SentAt < endDate)
                        .Include(l => l.Notification)
                        .ToListAsync();

                    dto.SuccessfulLogs = filteredLogs.Count(l => l.Status == "Success");
                    dto.FailedLogs = filteredLogs.Count(l => l.Status == "Failed");
                    dto.TestLogs = filteredLogs.Count(l => l.Status == "Test");

                    _logger.LogInformation("✓ Log istatistikleri: Başarılı={Success}, Başarısız={Failed}, Test={Test}",
                        dto.SuccessfulLogs, dto.FailedLogs, dto.TestLogs);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Loglar çekilirken DB hatası");
                    throw new InvalidOperationException("Log verileri erişilemedi", dbEx);
                }

                // ✅ 3. BAŞARI ORANI
                try
                {
                    var totalLogs = dto.SuccessfulLogs + dto.FailedLogs;
                    dto.SuccessRate = totalLogs > 0
                        ? Math.Round((decimal)dto.SuccessfulLogs / totalLogs * 100, 2)
                        : 0;

                    _logger.LogInformation("✓ Başarı oranı: {SuccessRate}%", dto.SuccessRate);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Başarı oranı hesaplarken hata");
                    dto.SuccessRate = 0;
                }

                // ✅ 4. GÜNLÜK İSTATİSTİKLER (Belleği kullanan loglardan hesapla)
                try
                {
                    dto.DailyStats = GetDailyStatistics(filteredLogs, startDate, endDate);
                    _logger.LogInformation("✓ Günlük istatistikler: {Count} gün", dto.DailyStats.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Günlük istatistikler hesaplarken hata");
                    dto.DailyStats = new();
                }

                // ✅ 5. SON GÖNDERILEN BİLDİRİMLER (Pagination: Max 10 kayıt)
                try
                {
                    dto.RecentNotifications = await _unitOfWork.Notifications.Query()
                        .Where(n => n.CreatedAt >= startDate && n.CreatedAt < endDate)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(10)  // ← Sadece 10 kayıt (Pagination)
                        .Select(n => new RecentNotificationDto
                        {
                            Id = n.Id,
                            Title = n.Title,
                            TargetAudience = n.TargetAudience,
                            IsSent = n.IsSent,
                            CreatedAt = n.CreatedAt,
                            SentAt = n.SentAt
                        })
                        .ToListAsync();

                    _logger.LogInformation("✓ Son bildirimler: {Count} kayıt (Pagination: Max 10)", dto.RecentNotifications.Count);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Son bildirimler çekilirken DB hatası");
                    dto.RecentNotifications = new();
                }

                // ✅ 6. HEDEF KİTLE DAĞILIMI (SQL'de Group By)
                try
                {
                    dto.TargetAudienceDistribution = await _unitOfWork.Notifications.Query()
                        .GroupBy(n => n.TargetAudience)
                        .Select(g => new
                        {
                            Audience = g.Key,
                            Count = g.Count()
                        })
                        .ToDictionaryAsync(
                            x => GetAudienceLabel(x.Audience),
                            x => x.Count
                        );

                    _logger.LogInformation("✓ Hedef kitle dağılımı: {Count} kategori", dto.TargetAudienceDistribution.Count);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Hedef kitle dağılımı hesaplarken DB hatası");
                    dto.TargetAudienceDistribution = new();
                }

                // ========== PANEL GENEL İSTATİSTİKLERİ ==========

                // ✅ 7. TOPLAM ADMIN KULLANICILAR
                try
                {
                    dto.TotalAdminUsers = await _unitOfWork.Query<CORE.Entities.AppUser>()
                        .CountAsync();

                    _logger.LogInformation("✓ Toplam admin kullanıcı: {Count}", dto.TotalAdminUsers);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Admin kullanıcı sayısı hesaplarken hata");
                    dto.TotalAdminUsers = 0;
                }

                // ✅ 8. TOPLAM BÖLÜMLER (SECTIONS)
                try
                {
                    dto.TotalSections = await _unitOfWork.Sections.Query()
                        .CountAsync();

                    _logger.LogInformation("✓ Toplam bölüm: {Count}", dto.TotalSections);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Bölüm sayısı hesaplarken hata");
                    dto.TotalSections = 0;
                }

                // ✅ 9. TOPLAM MİNİ UYGULAMALAR
                try
                {
                    dto.TotalMiniApps = await _unitOfWork.MiniAppItems.Query()
                        .CountAsync();

                    _logger.LogInformation("✓ Toplam mini uygulama: {Count}", dto.TotalMiniApps);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Mini uygulama sayısı hesaplarken hata");
                    dto.TotalMiniApps = 0;
                }

                _logger.LogInformation("✓✓✓ Dashboard verileri başarıyla hesaplandı!");
                return dto;
            }
            catch (InvalidOperationException ioEx)
            {
                _logger.LogError(ioEx, "Dashboard işlem hatası: {Message}", ioEx.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dashboard verileri hesaplanırken beklenmeyen hata: {Message}", ex.Message);
                throw new InvalidOperationException("Dashboard verileri hesaplanırken bir hata oluştu", ex);
            }
        }

        private List<DailyStatistics> GetDailyStatistics(
            List<CORE.Entities.NotificationLog> logs,
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                var stats = new List<DailyStatistics>();

                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    // Tüm status'ları say: Created + Test + Success
                    var count = logs.Count(l =>
                        l.SentAt.Date == date && 
                        (l.Status == "Created" || l.Status == "Test" || l.Status == "Success"));

                    stats.Add(new DailyStatistics
                    {
                        Date = date.ToString("dd.MM"),
                        Count = count
                    });
                }

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Günlük istatistikler hesaplarken hata");
                return new();
            }
        }

        private string GetAudienceLabel(string audience)
        {
            try
            {
                return audience switch
                {
                    "guest" => "Misafirler",
                    "regular" => "Kayıtlı Kullanıcılar",
                    "all" => "Herkes",
                    _ => audience ?? "Bilinmeyen Kitle"
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Hedef kitle etiketi dönüştürülürken hata: {Audience}", audience);
                return "Tanımlanmamış";
            }
        }
    }
}
