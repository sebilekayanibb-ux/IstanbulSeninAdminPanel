using IstanbulSenin.BLL.Services.Sections;
using IstanbulSenin.CORE.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IstanbulSenin.MVC.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class SectionController : Controller
    {
        private readonly ISectionService _sectionService;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        public async Task<IActionResult> Index()
        {
            var sections = await _sectionService.GetSectionsWithItemsAsync();
            return View(sections);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Section section)
        {
            // DisplayOrder boş bırakılırsa service otomatik en sona ekler
            ModelState.Remove(nameof(Section.DisplayOrder));

            if (ModelState.IsValid)
            {
                await _sectionService.CreateSectionAsync(section);
                return RedirectToAction(nameof(Index));
            }
            return View(section);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var section = await _sectionService.GetSectionDetailsAsync(id);
            if (section == null) return NotFound();
            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Section section)
        {
            // DisplayOrder boş bırakılırsa service mevcut sırayı korur (0 ile gelir, service günceller)
            ModelState.Remove(nameof(Section.DisplayOrder));

            if (ModelState.IsValid)
            {
                await _sectionService.UpdateSectionAsync(section);
                return RedirectToAction(nameof(Index));
            }
            return View(section);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _sectionService.RemoveSectionAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds)
        {
            if (orderedIds == null || !orderedIds.Any()) return BadRequest();
            await _sectionService.ReorderAsync(orderedIds);
            return Ok();
        }
    }
}