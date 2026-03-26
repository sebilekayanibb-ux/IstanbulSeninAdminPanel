using IstanbulSenin.BLL.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // GET: Dashboard/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await _dashboardService.GetDashboardDataAsync();
                return View(data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Dashboard verileri yüklenirken hata oluştu: " + ex.Message;
                return View(new DashboardDto());
            }
        }

        // GET: Dashboard/DateRange
        [HttpGet]
        public async Task<IActionResult> DateRange(DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var start = startDate?.Date ?? DateTime.Now.AddDays(-30).Date;
                var end = endDate?.Date.AddDays(1) ?? DateTime.Now.Date.AddDays(1);

                var data = await _dashboardService.GetDashboardDataByDateRangeAsync(start, end);
                return View("Index", data);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Tarih aralığı filtrelemesi başarısız: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
