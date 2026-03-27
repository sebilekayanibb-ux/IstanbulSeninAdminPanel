using IstanbulSenin.BLL.Services.Dashboard;
using IstanbulSenin.MVC.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers.Api
{
    /// <summary>
    /// Mobil app ve dashboard için istatistik ve metrikleri sağlayan API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardApiController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardApiController> _logger;

        public DashboardApiController(
            IDashboardService dashboardService,
            ILogger<DashboardApiController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        /// <summary>
        /// Kapsamlı istatistikleri getir (toplam, gönderilen, beklemede, başarı oranı vb.)
        /// </summary>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(ApiResponse<DashboardStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<DashboardStatisticsDto>>> GetStatistics()
        {
            try
            {
                _logger.LogInformation("GetStatistics API - User: {User}", User.Identity?.Name);

                var dashboard = await _dashboardService.GetDashboardDataAsync();

                if (dashboard == null)
                {
                    return Ok(ApiResponse<DashboardStatisticsDto>.SuccessResponse(
                        new DashboardStatisticsDto(),
                        "Veri henüz yoktur"));
                }

                var dto = new DashboardStatisticsDto
                {
                    TotalNotifications = dashboard.TotalNotifications,
                    SentNotifications = dashboard.SentNotifications,
                    PendingNotifications = dashboard.PendingNotifications,
                    SuccessRate = dashboard.SuccessRate,
                    DailyStatistics = dashboard.DailyStats?.Select(d => new DailyStatisticsDto
                    {
                        Date = DateTime.TryParse(d.Date, out var parsedDate) ? parsedDate : DateTime.Today,
                        SentCount = d.Count,
                        CreatedAt = DateTime.UtcNow
                    }).ToList() ?? new(),
                    RecentNotifications = dashboard.RecentNotifications?.Select(n => new NotificationResponseDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Body = "",
                        TargetAudience = n.TargetAudience,
                        IsSent = n.IsSent,
                        SentAt = n.SentAt,
                        CreatedAt = n.CreatedAt,
                        IsTestMode = false
                    }).ToList() ?? new(),
                    AudienceDistribution = dashboard.TargetAudienceDistribution ?? new()
                };

                return Ok(ApiResponse<DashboardStatisticsDto>.SuccessResponse(
                    dto,
                    "İstatistikler başarıyla getirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstatistikleri getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<DashboardStatisticsDto>.ErrorResponse(
                    "İstatistikleri getirirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Bugünün istatistiklerini getir
        /// </summary>
        [HttpGet("today")]
        [ProducesResponseType(typeof(ApiResponse<DailyStatisticsDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<DailyStatisticsDto>>> GetTodayStatistics()
        {
            try
            {
                var dashboard = await _dashboardService.GetDashboardDataAsync();

                var today = dashboard?.DailyStats?.FirstOrDefault(d => 
                    DateTime.TryParse(d.Date, out var parsedDate) && parsedDate.Date == DateTime.Today);

                if (today == null)
                {
                    return Ok(ApiResponse<DailyStatisticsDto>.SuccessResponse(
                        new DailyStatisticsDto { Date = DateTime.Today, SentCount = 0 },
                        "Bugün için veri yoktur"));
                }

                var dto = new DailyStatisticsDto
                {
                    Date = DateTime.Today,
                    SentCount = today.Count,
                    CreatedAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<DailyStatisticsDto>.SuccessResponse(
                    dto,
                    "Bugünün istatistikleri getirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bugünün istatistiklerini getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<DailyStatisticsDto>.ErrorResponse(
                    "Bugünün istatistikleri getirilemedi"));
            }
        }

        /// <summary>
        /// Son 7 günün günlük istatistiklerini getir
        /// </summary>
        [HttpGet("last-7-days")]
        [ProducesResponseType(typeof(ApiResponse<List<DailyStatisticsDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DailyStatisticsDto>>>> GetLast7DaysStatistics()
        {
            try
            {
                var startDate = DateTime.Today.AddDays(-7);
                var endDate = DateTime.Today;

                var dashboard = await _dashboardService.GetDashboardDataByDateRangeAsync(startDate, endDate);

                var last7Days = dashboard?.DailyStats?
                    .OrderByDescending(d => d.Date)
                    .Select(d => new DailyStatisticsDto
                    {
                        Date = DateTime.TryParse(d.Date, out var parsedDate) ? parsedDate : DateTime.Today,
                        SentCount = d.Count,
                        CreatedAt = DateTime.UtcNow
                    }).ToList() ?? new();

                return Ok(ApiResponse<List<DailyStatisticsDto>>.SuccessResponse(
                    last7Days,
                    $"Son 7 günün istatistikleri getirildi ({last7Days.Count} gün)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Son 7 günün istatistiklerini getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<List<DailyStatisticsDto>>.ErrorResponse(
                    "İstatistikler getirilemedi"));
            }
        }

        /// <summary>
        /// Belirli tarih aralığı için istatistikleri getir
        /// </summary>
        [HttpGet("range")]
        [ProducesResponseType(typeof(ApiResponse<List<DailyStatisticsDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<DailyStatisticsDto>>>> GetRangeStatistics(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest(ApiResponse<List<DailyStatisticsDto>>.ErrorResponse(
                        "Başlangıç tarihi, bitiş tarihinden önceki olmalıdır"));
                }

                var dashboard = await _dashboardService.GetDashboardDataByDateRangeAsync(startDate, endDate);

                var rangeStats = dashboard?.DailyStats?
                    .OrderByDescending(d => d.Date)
                    .Select(d => new DailyStatisticsDto
                    {
                        Date = DateTime.TryParse(d.Date, out var parsedDate) ? parsedDate : DateTime.Today,
                        SentCount = d.Count,
                        CreatedAt = DateTime.UtcNow
                    }).ToList() ?? new();

                return Ok(ApiResponse<List<DailyStatisticsDto>>.SuccessResponse(
                    rangeStats,
                    $"Belirtilen tarih aralığı için istatistikler getirildi ({rangeStats.Count} gün)"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tarih aralığı istatistiklerini getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<List<DailyStatisticsDto>>.ErrorResponse(
                    "İstatistikler getirilemedi"));
            }
        }
    }
}
