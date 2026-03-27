using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IstanbulSenin.MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecurityLogController : ControllerBase
    {
        private readonly ILogger<SecurityLogController> _logger;

        public SecurityLogController(ILogger<SecurityLogController> logger)
        {
            _logger = logger;
        }

        [HttpPost("log")]
        public IActionResult LogSecurityEvent([FromBody] SecurityEvent securityEvent)
        {
            if (securityEvent == null)
                return BadRequest();

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var userId = User.FindFirst("UserId")?.Value ?? "Anonymous";

            _logger.LogWarning(
                "SECURITY EVENT: {EventType} by {UserId} from {ClientIp} | Details: {Details}",
                securityEvent.Type,
                userId,
                clientIp,
                System.Text.Json.JsonSerializer.Serialize(securityEvent.Details));

            return Ok();
        }
    }

    public class SecurityEvent
    {
        public string Type { get; set; }
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public string UserAgent { get; set; }
        public Dictionary<string, object> Details { get; set; }
    }
}
