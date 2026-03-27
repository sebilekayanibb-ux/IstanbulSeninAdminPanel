using IstanbulSenin.BLL.Services.Sections;
using IstanbulSenin.MVC.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SectionsApiController : ControllerBase
    {
        private readonly ISectionService _sectionService;
        private readonly ILogger<SectionsApiController> _logger;

        public SectionsApiController(ISectionService sectionService, ILogger<SectionsApiController> logger)
        {
            _sectionService = sectionService;
            _logger = logger;
        }

        /// <summary>
        /// Tüm bölümleri getir (mini uygulamalar ile birlikte)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<SectionResponseDto>>>> GetAll()
        {
            try
            {
                var sections = await _sectionService.GetSectionsWithItemsAsync();
                var dtos = sections.Select(s => new SectionResponseDto
                {
                    Id = s.Id,
                    Name = s.Title ?? "Untitled",
                    Description = s.Role,
                    IsActive = !s.Items.Any(i => i.IsHide),
                    DisplayOrder = s.DisplayOrder,
                    CreatedAt = DateTime.Now,
                    MiniApps = s.Items.Select(m => new MiniAppResponseDto
                    {
                        Id = m.Id,
                        Name = m.Title ?? "Untitled",
                        Description = m.Description,
                        IconUrl = m.Image,
                        AppUrl = m.Url,
                        IsActive = !m.IsHide,
                        DisplayOrder = m.DisplayOrder
                    }).ToList()
                }).ToList();

                return Ok(ApiResponse<List<SectionResponseDto>>.SuccessResponse(dtos, "Bölümler başarıyla alındı"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll Sections");
                return StatusCode(500, ApiResponse<List<SectionResponseDto>>.ErrorResponse("Hata oluştu", 500));
            }
        }

        /// <summary>
        /// Belirli bir bölümü getir
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<SectionResponseDto>>> GetById(int id)
        {
            try
            {
                var section = await _sectionService.GetSectionDetailsAsync(id);
                if (section == null)
                    return NotFound(ApiResponse<SectionResponseDto>.ErrorResponse("Bölüm bulunamadı", 404));

                var dto = new SectionResponseDto
                {
                    Id = section.Id,
                    Name = section.Title,
                    Description = section.Role,
                    IsActive = !section.Items.Any(i => i.IsHide),
                    DisplayOrder = section.DisplayOrder,
                    CreatedAt = DateTime.Now,
                    MiniApps = section.Items.Select(m => new MiniAppResponseDto
                    {
                        Id = m.Id,
                        Name = m.Title,
                        Description = m.Description,
                        IconUrl = m.Image,
                        AppUrl = m.Url,
                        IsActive = !m.IsHide,
                        DisplayOrder = m.DisplayOrder
                    }).ToList()
                };

                return Ok(ApiResponse<SectionResponseDto>.SuccessResponse(dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return StatusCode(500, ApiResponse<SectionResponseDto>.ErrorResponse("Hata oluştu", 500));
            }
        }
    }
}
