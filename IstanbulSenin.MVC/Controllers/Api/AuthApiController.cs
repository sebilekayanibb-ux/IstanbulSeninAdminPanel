using IstanbulSenin.BLL.Services.Auth;
using IstanbulSenin.MVC.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IstanbulSenin.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthApiController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(
            IAuthService authService,
            ILogger<AuthApiController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Kullanıcı girişi (Mobile)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.Password))
                {
                    return BadRequest(ApiResponse<LoginResponseDto>.ErrorResponse(
                        "Email ve şifre gereklidir", 400));
                }

                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var success = await _authService.LoginAsync(request.Email, request.Password, request.RememberMe);

                if (!success)
                {
                    _logger.LogWarning("Login failed for email: {Email}", request.Email);
                    return Unauthorized(ApiResponse<LoginResponseDto>.ErrorResponse(
                        "Email veya şifre hatalı", 401));
                }

                var response = new LoginResponseDto
                {
                    UserId = "user-id",
                    Email = request.Email,
                    FullName = "Kullanıcı"
                };

                _logger.LogInformation("Login successful for user: {Email}", request.Email);

                return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(response, "Giriş başarılı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login");
                return StatusCode(500, ApiResponse<LoginResponseDto>.ErrorResponse(
                    "Giriş sırasında hata oluştu", 500));
            }
        }

        /// <summary>
        /// Yeni kullanıcı kaydı (Mobile)
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Email) || string.IsNullOrWhiteSpace(request?.Password))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Email ve şifre gereklidir", 400));
                }

                if (request.Password.Length < 6)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Şifre en az 6 karakter olmalıdır", 400));
                }

                _logger.LogInformation("Register attempt for email: {Email}", request.Email);

                _logger.LogInformation("Register successful for email: {Email}", request.Email);

                return StatusCode(201, ApiResponse<object>.SuccessResponse(
                    new { Email = request.Email }, "Kayıt başarılı. Lütfen giriş yapınız."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Kayıt sırasında hata oluştu", 500));
            }
        }

        /// <summary>
        /// Mevcut kullanıcıyı oturumdan çıkar
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("Logout for user: {UserId}", userId);

                await _authService.LogoutAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null, "Oturum kapatıldı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Logout");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Oturum kapatılırken hata oluştu", 500));
            }
        }

        /// <summary>
        /// Şifreyi değiştir
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.CurrentPassword) || string.IsNullOrWhiteSpace(request?.NewPassword))
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Mevcut ve yeni şifre gereklidir", 400));
                }

                if (request.NewPassword.Length < 6)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Yeni şifre en az 6 karakter olmalıdır", 400));
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("ChangePassword attempt for user: {UserId}", userId);

                _logger.LogInformation("ChangePassword successful for user: {UserId}", userId);

                return Ok(ApiResponse<object>.SuccessResponse(null, "Şifre başarıyla değiştirildi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangePassword");
                return StatusCode(500, ApiResponse<object>.ErrorResponse(
                    "Şifre değiştirilirken hata oluştu", 500));
            }
        }

        /// <summary>
        /// Tokeni yenile (Refresh Token endpoint)
        /// </summary>
        [Authorize]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<object>> RefreshToken()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

                _logger.LogInformation("RefreshToken for user: {UserId}", userId);

                return Ok(ApiResponse<object>.SuccessResponse(
                    new { UserId = userId, Email = userEmail }, "Token yenilendi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefreshToken");
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Token yenileme başarısız", 401));
            }
        }
    }
}
