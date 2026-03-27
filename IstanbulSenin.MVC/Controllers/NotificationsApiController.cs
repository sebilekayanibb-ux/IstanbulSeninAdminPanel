using IstanbulSenin.BLL.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers
{
    /// <summary>
    /// Mobil app ve 3. party entegrasyonları için Notification API
    /// 
    /// Kullanım:
    /// - Bildirim göndermek: POST /api/notifications/send
    /// - Bildirimleri listelemek: GET /api/notifications
    /// - Bildirim almış olarak işaretlemek: PUT /api/notifications/{id}/mark-as-read
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsApiController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsApiController> _logger;

        public NotificationsApiController(
            INotificationService notificationService,
            ILogger<NotificationsApiController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm bildirimleri listele (mobil app için)
        /// </summary>
        /// <returns>Bildirim listesi</returns>
        /// <response code="200">Başarılı</response>
        /// <response code="401">Yetkisiz erişim</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetAll()
        {
            try
            {
                _logger.LogInformation("GetAll API - User: {User}", User.Identity?.Name);

                var notifications = await _notificationService.GetAllAsync();
                
                var dtos = notifications.Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    TargetAudience = n.TargetAudience,
                    IsSent = n.IsSent,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt,
                    IsTestMode = n.IsTestMode
                }).ToList();

                return Ok(ApiResponse<List<NotificationResponseDto>>.SuccessResponse(
                    dtos,
                    $"{dtos.Count} bildirim bulundu"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirimleri getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<List<NotificationResponseDto>>.ErrorResponse(
                    "Bildirimleri getirirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Belirli bir bildirimi getir
        /// </summary>
        /// <param name="id">Bildirim ID</param>
        /// <returns>Bildirim detayları</returns>
        /// <response code="200">Başarılı</response>
        /// <response code="404">Bulunamadı</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<NotificationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<NotificationResponseDto>>> GetById(int id)
        {
            try
            {
                var notifications = await _notificationService.GetAllAsync();
                var notification = notifications.FirstOrDefault(n => n.Id == id);

                if (notification == null)
                    return NotFound(ApiResponse<NotificationResponseDto>.ErrorResponse(
                        "Bildirim bulunamadı"));

                var dto = new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Body = notification.Body,
                    TargetAudience = notification.TargetAudience,
                    IsSent = notification.IsSent,
                    SentAt = notification.SentAt,
                    CreatedAt = notification.CreatedAt,
                    IsTestMode = notification.IsTestMode
                };

                return Ok(ApiResponse<NotificationResponseDto>.SuccessResponse(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<NotificationResponseDto>.ErrorResponse(
                    "Bildirim getirirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Gönderilmiş bildirimleri getir
        /// </summary>
        /// <returns>Gönderilmiş bildirimler</returns>
        [HttpGet("sent")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetSent()
        {
            try
            {
                var notifications = await _notificationService.GetAllAsync();
                var sent = notifications.Where(n => n.IsSent).ToList();

                var dtos = sent.Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    TargetAudience = n.TargetAudience,
                    IsSent = n.IsSent,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt,
                    IsTestMode = n.IsTestMode
                }).ToList();

                return Ok(ApiResponse<List<NotificationResponseDto>>.SuccessResponse(
                    dtos,
                    $"{dtos.Count} gönderilmiş bildirim"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gönderilmiş bildirimleri getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<List<NotificationResponseDto>>.ErrorResponse(
                    "Gönderilmiş bildirimleri getirirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Beklemede olan bildirimleri getir
        /// </summary>
        /// <returns>Beklemede bildirimleri</returns>
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<List<NotificationResponseDto>>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<List<NotificationResponseDto>>>> GetPending()
        {
            try
            {
                var notifications = await _notificationService.GetAllAsync();
                var pending = notifications.Where(n => !n.IsSent).ToList();

                var dtos = pending.Select(n => new NotificationResponseDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    TargetAudience = n.TargetAudience,
                    IsSent = n.IsSent,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt,
                    IsTestMode = n.IsTestMode
                }).ToList();

                return Ok(ApiResponse<List<NotificationResponseDto>>.SuccessResponse(
                    dtos,
                    $"{dtos.Count} beklemede bildirim"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Beklemede bildirimleri getirirken hata: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<List<NotificationResponseDto>>.ErrorResponse(
                    "Beklemede bildirimleri getirirken bir hata oluştu"));
            }
        }

        /// <summary>
        /// Bildirimi gönder
        /// </summary>
        /// <param name="id">Bildirim ID</param>
        /// <returns>Gönderim sonucu</returns>
        /// <response code="200">Başarıyla gönderildi</response>
        /// <response code="400">Hatalı istek</response>
        /// <response code="404">Bulunamadı</response>
        [HttpPost("{id}/send")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<NotificationResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<NotificationResponseDto>>> Send(int id)
        {
            try
            {
                _logger.LogInformation("📱 API: Bildirim gönder - ID: {Id}", id);

                var (success, error) = await _notificationService.SendAsync(id);

                if (!success)
                    return BadRequest(ApiResponse<NotificationResponseDto>.ErrorResponse(error));

                var notifications = await _notificationService.GetAllAsync();
                var notification = notifications.FirstOrDefault(n => n.Id == id);

                if (notification == null)
                    return NotFound(ApiResponse<NotificationResponseDto>.ErrorResponse(
                        "Bildirim bulunamadı"));

                var dto = new NotificationResponseDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Body = notification.Body,
                    TargetAudience = notification.TargetAudience,
                    IsSent = notification.IsSent,
                    SentAt = notification.SentAt,
                    CreatedAt = notification.CreatedAt,
                    IsTestMode = notification.IsTestMode
                };

                return Ok(ApiResponse<NotificationResponseDto>.SuccessResponse(
                    dto,
                    "Bildirim başarıyla gönderildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bildirim gönderilemedi: {Message}", ex.Message);
                return StatusCode(500, ApiResponse<NotificationResponseDto>.ErrorResponse(
                    "Bildirim gönderilemedi"));
            }
        }
    }
}
