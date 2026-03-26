using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Repositories;
using IstanbulSenin.BLL.Services.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace IstanbulSenin.BLL.Services.Sections
{
    public class SectionService : ISectionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDashboardService _dashboardService;

        public SectionService(IUnitOfWork unitOfWork, IDashboardService dashboardService)
        {
            _unitOfWork = unitOfWork;
            _dashboardService = dashboardService;
        }

        public async Task<List<Section>> GetSectionsWithItemsAsync()
        {
            return await Task.FromResult(
                _unitOfWork.Sections.Query()
                    .Include(x => x.Items)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList()
            );
        }

        public async Task<Section?> GetSectionDetailsAsync(int id)
        {
            return await Task.FromResult(
                _unitOfWork.Sections.Query()
                    .Include(x => x.Items)
                    .FirstOrDefault(x => x.Id == id)
            );
        }
        public async Task CreateSectionAsync(Section section)
        {
            var allSections = _unitOfWork.Sections.Query().ToList();
            var count = allSections.Count;

            if (section.DisplayOrder <= 0 || section.DisplayOrder > count + 1)
                section.DisplayOrder = count + 1;

            var conflict = allSections.Any(x => x.DisplayOrder == section.DisplayOrder);

            if (conflict)
            {
                var toShift = allSections
                    .Where(x => x.DisplayOrder >= section.DisplayOrder)
                    .ToList();
                foreach (var s in toShift) s.DisplayOrder++;
            }

            await _unitOfWork.Sections.AddAsync(section);
            await _unitOfWork.SaveChangesAsync();

            _dashboardService.InvalidateDashboardCache();
            await RenormalizeOrdersAsync();
        }

        public async Task UpdateSectionAsync(Section section)
        {
            var existing = await _unitOfWork.Sections.GetByIdAsync(section.Id);
            if (existing == null) return;

            existing.Title = section.Title;
            existing.Role = section.Role;
            existing.Size = section.Size;

            if (section.DisplayOrder > 0)
            {
                var allSections = _unitOfWork.Sections.Query().ToList();
                var count = allSections.Count;
                existing.DisplayOrder = Math.Min(section.DisplayOrder, count);
            }

            _unitOfWork.Sections.Update(existing);
            await _unitOfWork.SaveChangesAsync();
            _dashboardService.InvalidateDashboardCache();
            await RenormalizeOrdersAsync(existing.Id);
        }

        public async Task RemoveSectionAsync(int id)
        {
            var section = await _unitOfWork.Sections.GetByIdAsync(id);
            if (section == null) return;

            _unitOfWork.Sections.Delete(section);
            await _unitOfWork.SaveChangesAsync();

            _dashboardService.InvalidateDashboardCache();
            await RenormalizeOrdersAsync();
        }

        public async Task ReorderAsync(List<int> orderedIds)
        {
            var sections = _unitOfWork.Sections.Query().ToList();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var sec = sections.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (sec != null) sec.DisplayOrder = i + 1;
            }
            await _unitOfWork.SaveChangesAsync();
            _dashboardService.InvalidateDashboardCache();
        }

        private async Task RenormalizeOrdersAsync(int preferredId = 0)
        {
            var sections = _unitOfWork.Sections.Query()
                .ToList()
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id == preferredId ? 0 : 1)
                .ThenBy(x => x.Id)
                .ToList();

            for (int i = 0; i < sections.Count; i++)
                sections[i].DisplayOrder = i + 1;

            foreach (var section in sections)
                _unitOfWork.Sections.Update(section);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
