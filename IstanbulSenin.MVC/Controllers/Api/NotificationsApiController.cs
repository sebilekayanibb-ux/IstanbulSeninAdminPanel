using IstanbulSenin.BLL.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcDtos = IstanbulSenin.MVC.Dtos;

namespace IstanbulSenin.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsApiController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsApiController> _logger;

        public NotificationsApiController(INotificationService notificationService, ILogger<NotificationsApiController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<MvcDtos.ApiResponse<List<MvcDtos.NotificationResponseDto>>>> GetAll()
        {
            try
            {
                var notifications = await _notificationService.GetAllAsync();
                var dtos = notifications.Select(n => new MvcDtos.NotificationResponseDto
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
                return Ok(MvcDtos.ApiResponse<List<MvcDtos.NotificationResponseDto>>.SuccessResponse(dtos, $"{dtos.Count} bildirim bulundu"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hata: {Message}", ex.Message);
                return StatusCode(500, MvcDtos.ApiResponse<List<MvcDtos.NotificationResponseDto>>.ErrorResponse("Hata oluştu"));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MvcDtos.ApiResponse<MvcDtos.NotificationResponseDto>>> GetById(int id)
        {
            try
            {
                var notifications = await _notificationService.GetAllAsync();
                var notification = notifications.FirstOrDefault(n => n.Id == id);
                if (notification == null)
                    return NotFound(MvcDtos.ApiResponse<MvcDtos.NotificationResponseDto>.ErrorResponse("Bildirim bulunamadı"));
                var dto = new MvcDtos.NotificationResponseDto
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
                return Ok(MvcDtos.ApiResponse<MvcDtos.NotificationResponseDto>.SuccessResponse(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, MvcDtos.ApiResponse<MvcDtos.NotificationResponseDto>.ErrorResponse("Hata oluştu"));
            }
        }
    }
}
