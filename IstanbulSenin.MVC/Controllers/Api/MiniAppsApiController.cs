using IstanbulSenin.BLL.Services.MiniApps;
using IstanbulSenin.MVC.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MiniAppsApiController : ControllerBase
    {
        private readonly IMiniAppItemService _miniAppService;
        private readonly ILogger<MiniAppsApiController> _logger;

        public MiniAppsApiController(IMiniAppItemService miniAppService, ILogger<MiniAppsApiController> logger)
        {
            _miniAppService = miniAppService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm mini uygulamaları getir
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MiniAppResponseDto>>>> GetAll()
        {
            try
            {
                var miniApps = await _miniAppService.GetMiniAppsWithSectionsAsync();
                var dtos = miniApps.Select(m => new MiniAppResponseDto
                {
                    Id = m.Id,
                    Name = m.Title ?? "Untitled",
                    Description = m.Description,
                    IconUrl = m.Image,
                    AppUrl = m.Url,
                    IsActive = !m.IsHide,
                    DisplayOrder = m.DisplayOrder,
                    SectionId = m.Sections?.FirstOrDefault()?.Id
                }).ToList();

                return Ok(ApiResponse<List<MiniAppResponseDto>>.SuccessResponse(dtos, "Mini uygulamalar alındı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll");
                return StatusCode(500, ApiResponse<List<MiniAppResponseDto>>.ErrorResponse("Hata oluştu", 500));
            }
        }

        /// <summary>
        /// Belirli bir mini uygulamayı getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MiniAppResponseDto>>> GetById(int id)
        {
            try
            {
                var miniApp = await _miniAppService.GetMiniAppDetailsAsync(id);
                if (miniApp == null)
                    return NotFound(ApiResponse<MiniAppResponseDto>.ErrorResponse("Mini uygulama bulunamadı", 404));

                var dto = new MiniAppResponseDto
                {
                    Id = miniApp.Id,
                    Name = miniApp.Title,
                    Description = miniApp.Description,
                    IconUrl = miniApp.Image,
                    AppUrl = miniApp.Url,
                    IsActive = !miniApp.IsHide,
                    DisplayOrder = miniApp.DisplayOrder,
                    SectionId = miniApp.Sections?.FirstOrDefault()?.Id
                };

                return Ok(ApiResponse<MiniAppResponseDto>.SuccessResponse(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return StatusCode(500, ApiResponse<MiniAppResponseDto>.ErrorResponse("Hata oluştu", 500));
            }
        }

        /// <summary>
        /// Aktif mini uygulamaları getir
        /// </summary>
        [HttpGet("active/list")]
        public async Task<ActionResult<ApiResponse<List<MiniAppResponseDto>>>> GetActive()
        {
            try
            {
                var miniApps = await _miniAppService.GetMiniAppsWithSectionsAsync();
                var activeDtos = miniApps
                    .Where(m => !m.IsHide)
                    .Select(m => new MiniAppResponseDto
                    {
                        Id = m.Id,
                        Name = m.Title,
                        Description = m.Description,
                        IconUrl = m.Image,
                        AppUrl = m.Url,
                        IsActive = !m.IsHide,
                        DisplayOrder = m.DisplayOrder,
                        SectionId = m.Sections?.FirstOrDefault()?.Id
                    })
                    .OrderBy(m => m.DisplayOrder)
                    .ToList();

                return Ok(ApiResponse<List<MiniAppResponseDto>>.SuccessResponse(activeDtos, "Aktif uygulamalar alındı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActive");
                return StatusCode(500, ApiResponse<List<MiniAppResponseDto>>.ErrorResponse("Hata oluştu", 500));
            }
        }
    }
}
