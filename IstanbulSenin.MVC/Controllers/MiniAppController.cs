using IstanbulSenin.BLL.Services.MiniApps;
using IstanbulSenin.BLL.Services.Sections;
using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class MiniAppController : Controller
    {
        private readonly IMiniAppItemService _miniAppService;
        private readonly ISectionService _sectionService;
        private readonly IWebHostEnvironment _env;

        public MiniAppController(IMiniAppItemService miniAppService, ISectionService sectionService, IWebHostEnvironment env)
        {
            _miniAppService = miniAppService;
            _sectionService = sectionService;
            _env = env;
        }

        public async Task<IActionResult> Index() => View(await _miniAppService.GetMiniAppsWithSectionsAsync());

        public async Task<IActionResult> Create()
        {
            ViewBag.Sections = await _sectionService.GetSectionsWithItemsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MiniAppItem item, IFormFile? iconFile, List<int> selectedSectionIds, List<PermissionType> selectedPermissions, List<PluginType> selectedPlugins)
        {
            // DisplayOrder boş bırakılırsa service otomatik en sona ekler
            ModelState.Remove(nameof(MiniAppItem.DisplayOrder));

            // 1. Resim yükle ve Image ModelState hatasını temizle
            if (iconFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads/icons");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + Path.GetExtension(iconFile.FileName);
                using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create)) { await iconFile.CopyToAsync(stream); }
                item.Image = "/uploads/icons/" + fileName;
                ModelState.Remove(nameof(MiniAppItem.Image));
            }

            // 2. Bölüm zorunluluk kontrolü
            if (selectedSectionIds == null || !selectedSectionIds.Any())
            {
                ModelState.AddModelError("SectionError", "Lütfen en az bir bölüm seçiniz.");
            }
            else
            {
                foreach (var sId in selectedSectionIds)
                {
                    var section = await _sectionService.GetSectionDetailsAsync(sId);
                    if (section != null) item.Sections.Add(section);
                }
            }

            // 3. Validation kontrolü
            if (!ModelState.IsValid)
            {
                ViewBag.Sections = await _sectionService.GetSectionsWithItemsAsync();
                ViewBag.SelectedSectionIds = selectedSectionIds;
                return View(item);
            }

            // 4. Kaydet
            item.Permissions = selectedPermissions ?? new();
            item.Plugins = selectedPlugins ?? new();
            await _miniAppService.CreateMiniAppAsync(item);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var app = await _miniAppService.GetMiniAppDetailsAsync(id);
            if (app == null) return NotFound();
            ViewBag.Sections = await _sectionService.GetSectionsWithItemsAsync();
            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MiniAppItem item, List<int> selectedSectionIds, IFormFile? iconFile, List<PermissionType> selectedPermissions, List<PluginType> selectedPlugins)
        {
            // DisplayOrder boş bırakılırsa mevcut sıra korunur
            ModelState.Remove(nameof(MiniAppItem.DisplayOrder));

            var existingApp = await _miniAppService.GetMiniAppDetailsAsync(item.Id);
            if (existingApp == null) return NotFound();

            // 1. RESİM GÜNCELLEME
            if (iconFile != null)
            {
                string folder = Path.Combine(_env.WebRootPath, "uploads/icons");

                // Eski resim varsa sil
                if (!string.IsNullOrEmpty(existingApp.Image))
                {
                    string oldFileName = existingApp.Image.Replace("/uploads/icons/", "");
                    string oldFilePath = Path.Combine(folder, oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        try { System.IO.File.Delete(oldFilePath); }
                        catch { /* Silme hatasında sessiz geç */ }
                    }
                }

                // Yeni resim yükle
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid() + Path.GetExtension(iconFile.FileName);
                using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create)) { await iconFile.CopyToAsync(stream); }
                existingApp.Image = "/uploads/icons/" + fileName;
            }

            // 2. TEMEL VERİLERİ AKTAR
            existingApp.Title = item.Title;
            existingApp.Url = item.Url;
            existingApp.LogoUrl = item.LogoUrl;
            existingApp.Description = item.Description;
            // DisplayOrder yalnızca form'da değer girilmişse güncellenir; girilmemişse mevcut sıra korunur
            if (item.DisplayOrder > 0) existingApp.DisplayOrder = item.DisplayOrder;
            existingApp.IsTest = item.IsTest;
            existingApp.IsHide = item.IsHide;
            existingApp.IsNative = item.IsNative;
            existingApp.Permissions = selectedPermissions ?? new();
            existingApp.Plugins = selectedPlugins ?? new();

            // 3. BÖLÜMLERİ GÜNCELLE
            if (selectedSectionIds == null || !selectedSectionIds.Any())
            {
                ModelState.AddModelError("SectionError", "Lütfen en az bir bölüm seçiniz.");
                ViewBag.Sections = await _sectionService.GetSectionsWithItemsAsync();
                return View(item);
            }

            existingApp.Sections.Clear();
            foreach (var sId in selectedSectionIds)
            {
                var sec = await _sectionService.GetSectionDetailsAsync(sId);
                if (sec != null) existingApp.Sections.Add(sec);
            }

            // 4. KAYDET
            await _miniAppService.UpdateMiniAppAsync(existingApp);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var app = await _miniAppService.GetMiniAppDetailsAsync(id);
            if (app != null && !string.IsNullOrEmpty(app.Image))
            {
                // Resim dosyasını sil
                string folder = Path.Combine(_env.WebRootPath, "uploads/icons");
                string fileName = app.Image.Replace("/uploads/icons/", "");
                string filePath = Path.Combine(folder, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    try { System.IO.File.Delete(filePath); }
                    catch { /* Silme hatasında sessiz geç */ }
                }
            }

            await _miniAppService.RemoveMiniAppAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds)
        {
            if (orderedIds == null || !orderedIds.Any()) return BadRequest();
            await _miniAppService.ReorderAsync(orderedIds);
            return Ok();
        }
    }
}