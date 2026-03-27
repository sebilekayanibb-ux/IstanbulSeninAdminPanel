using IstanbulSenin.BLL.Services.Notifications;
using IstanbulSenin.CORE.Entities;
using IstanbulSenin.HELPER.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers
{
    [Authorize(Roles = AppConstants.Roles.SuperAdmin)]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationLogService _logService;

        public NotificationController(
            INotificationService notificationService,
            INotificationLogService logService)
        {
            _notificationService = notificationService;
            _logService = logService;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _notificationService.GetAllAsync();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new Notification());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (!ModelState.IsValid)
                return View(notification);

            var (success, error) = await _notificationService.CreateAsync(notification);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(notification);
            }

            TempData["Success"] = "Bildirim kaydedildi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var notifications = await _notificationService.GetAllAsync();
            var notification = notifications.FirstOrDefault(x => x.Id == id);

            if (notification == null)
                return NotFound();

            if (notification.IsSent)
            {
                TempData["Error"] = "Gönderilmiş bildirimi düzenlenemez.";
                return RedirectToAction(nameof(Index));
            }

            return View(notification);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Notification notification)
        {
            if (!ModelState.IsValid)
                return View(notification);

            var (success, error) = await _notificationService.UpdateAsync(id, notification);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, error);
                return View(notification);
            }

            TempData["Success"] = "Bildirim güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Send(int id)
        {
            var (success, error) = await _notificationService.SendAsync(id);

            if (!success)
                TempData["Error"] = error;
            else
                TempData["Success"] = "Bildirim gönderildi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _notificationService.DeleteAsync(id);
            if (!success)
            {
                TempData["Error"] = error;
            }
            else
            {
                TempData["Success"] = "Bildirim silindi.";
            }

            return RedirectToAction(nameof(Index));
        }

        /// Tüm bildirim gönderme geçmişini göster
        public async Task<IActionResult> Logs()
        {
            var logs = await _logService.GetAllAsync();
            return View(logs);
        }

        /// Belirli bildirimin gönderme geçmişini göster
        public async Task<IActionResult> NotificationLogs(int notificationId)
        {
            var logs = await _logService.GetByNotificationIdAsync(notificationId);
            ViewBag.NotificationId = notificationId;
            return View(logs);
        }
    }
}

