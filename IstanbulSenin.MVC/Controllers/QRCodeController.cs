using IstanbulSenin.BLL.Services.QRCodes;
using IstanbulSenin.CORE.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QRCodeController : Controller
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly UserManager<AppUser> _userManager;

        public QRCodeController(IQRCodeService qrCodeService, UserManager<AppUser> userManager)
        {
            _qrCodeService = qrCodeService;
            _userManager = userManager;
        }

        // GET: QRCode/Index - Tüm QR kodlarını listele
        public async Task<IActionResult> Index()
        {
            var qrCodes = await _qrCodeService.GetAllAsync();
            return View(qrCodes);
        }

        // GET: QRCode/Create - QR kod oluştur formu
        public async Task<IActionResult> Create()
        {
            var users = await _userManager.GetUsersInRoleAsync("User");
            ViewBag.Users = users;
            return View();
        }

        // POST: QRCode/Create - QR kod oluştur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string userId, int expirationDays, string description)
        {
            try
            {
                // Validasyon
                if (string.IsNullOrWhiteSpace(userId))
                {
                    TempData["Error"] = "Kullanıcı seçilmelidir.";
                    return RedirectToAction(nameof(Create));
                }

                if (expirationDays <= 0 || expirationDays > 365)
                {
                    TempData["Error"] = "Geçerlilik süresi 1-365 gün arasında olmalıdır.";
                    return RedirectToAction(nameof(Create));
                }

                if (string.IsNullOrWhiteSpace(description))
                {
                    TempData["Error"] = "Açıklama boş olamaz.";
                    return RedirectToAction(nameof(Create));
                }

                var expiresAt = DateTime.UtcNow.AddDays(expirationDays);
                var currentAdmin = await _userManager.GetUserAsync(User);
                var createdByAdmin = currentAdmin?.UserName ?? "System";

                var (success, error) = await _qrCodeService.CreateAsync(userId, expiresAt, description, createdByAdmin);

                if (success)
                {
                    TempData["Success"] = "QR kod başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = error;
                    return RedirectToAction(nameof(Create));
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "QR kod oluşturulurken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: QRCode/Details/5 - QR kod detaylarını göster
        public async Task<IActionResult> Details(int id)
        {
            var qrCode = await _qrCodeService.GetByIdAsync(id);

            if (qrCode == null)
            {
                TempData["Error"] = "QR kod bulunamadı.";
                return RedirectToAction(nameof(Index));
            }

            return View(qrCode);
        }

        // POST: QRCode/Cancel/5 - QR kod iptal et
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var (success, error) = await _qrCodeService.CancelAsync(id);

            if (success)
            {
                TempData["Success"] = "QR kod başarıyla iptal edildi.";
            }
            else
            {
                TempData["Error"] = error;
            }

            return RedirectToAction(nameof(Index));
        }

        // API: POST /api/qrcode/validate - Mobil uygulaması için
        [HttpPost]
        [Route("api/qrcode/validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCode([FromBody] ValidateQRCodeRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Code))
                {
                    return BadRequest(new { success = false, message = "QR kod boş olamaz." });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var (success, error) = await _qrCodeService.ValidateAndUseAsync(request.Code, ipAddress);

                if (success)
                {
                    var qrCode = await _qrCodeService.GetByCodeAsync(request.Code);
                    return Ok(new { success = true, userId = qrCode?.UserId, message = "QR kod doğrulandı." });
                }
                else
                {
                    return Ok(new { success = false, message = error });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Sunucu hatası: " + ex.Message });
            }
        }
    }

    // Mobil uygulamadan gelen request modeli
    public class ValidateQRCodeRequest
    {
        public string Code { get; set; }
        public string? DeviceId { get; set; }
    }
}
