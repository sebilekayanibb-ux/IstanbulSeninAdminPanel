using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Repositories;
using IstanbulSenin.BLL.Services.Dashboard;
using Microsoft.EntityFrameworkCore;

namespace IstanbulSenin.BLL.Services.MiniApps
{
    public class MiniAppItemService : IMiniAppItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDashboardService _dashboardService;

        public MiniAppItemService(IUnitOfWork unitOfWork, IDashboardService dashboardService)
        {
            _unitOfWork = unitOfWork;
            _dashboardService = dashboardService;
        }

        public async Task<List<MiniAppItem>> GetMiniAppsWithSectionsAsync() =>
            await Task.FromResult(
                _unitOfWork.MiniAppItems.Query()
                    .Include(x => x.Sections)
                    .OrderBy(x => x.DisplayOrder)
                    .ToList()
            );

        public async Task<MiniAppItem?> GetMiniAppDetailsAsync(int id) =>
            await Task.FromResult(
                _unitOfWork.MiniAppItems.Query()
                    .Include(x => x.Sections)
                    .FirstOrDefault(x => x.Id == id)
            );

        public async Task<List<MiniAppItem>> GetActiveMiniAppsForUserAsync(bool isTestUser)
        {
            var query = _unitOfWork.MiniAppItems.Query()
                .Include(x => x.Sections)
                .Where(x => !x.IsHide);

            if (!isTestUser) 
                query = query.Where(x => !x.IsTest);

            return await Task.FromResult(
                query.OrderBy(x => x.DisplayOrder).ToList()
            );
        }

        public async Task CreateMiniAppAsync(MiniAppItem item)
        {
            var allItems = _unitOfWork.MiniAppItems.Query().ToList();
            var count = allItems.Count;

            if (item.DisplayOrder <= 0 || item.DisplayOrder > count + 1)
                item.DisplayOrder = count + 1;

            var conflict = allItems.Any(x => x.DisplayOrder == item.DisplayOrder);

            if (conflict)
            {
                var toShift = allItems
                    .Where(x => x.DisplayOrder >= item.DisplayOrder)
                    .ToList();
                foreach (var a in toShift) a.DisplayOrder++;
            }

            await _unitOfWork.MiniAppItems.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();

            _dashboardService.InvalidateDashboardCache();
            await RenormalizeOrdersAsync();
        }

        public async Task UpdateMiniAppAsync(MiniAppItem trackedItem)
        {
            if (trackedItem.DisplayOrder > 0)
            {
                var allItems = _unitOfWork.MiniAppItems.Query().ToList();
                var count = allItems.Count;
                trackedItem.DisplayOrder = Math.Min(trackedItem.DisplayOrder, count);
            }

            _unitOfWork.MiniAppItems.Update(trackedItem);
            await _unitOfWork.SaveChangesAsync();
            _dashboardService.InvalidateDashboardCache();
            await RenormalizeOrdersAsync(trackedItem.Id);
        }

        public async Task RemoveMiniAppAsync(int id)
        {
            var item = await _unitOfWork.MiniAppItems.GetByIdAsync(id);
            if (item == null) return;

            _unitOfWork.MiniAppItems.Delete(item);
            await _unitOfWork.SaveChangesAsync();

            _dashboardService.InvalidateDashboardCache();
            await RenormalizeOrdersAsync();
        }

        public async Task ReorderAsync(List<int> orderedIds)
        {
            var items = _unitOfWork.MiniAppItems.Query().ToList();
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var item = items.FirstOrDefault(x => x.Id == orderedIds[i]);
                if (item != null) item.DisplayOrder = i + 1;
            }
            await _unitOfWork.SaveChangesAsync();
            _dashboardService.InvalidateDashboardCache();
        }

        private async Task RenormalizeOrdersAsync(int preferredId = 0)
        {
            var items = _unitOfWork.MiniAppItems.Query()
                .ToList()
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Id == preferredId ? 0 : 1)
                .ThenBy(x => x.Id)
                .ToList();

            for (int i = 0; i < items.Count; i++)
                items[i].DisplayOrder = i + 1;

            foreach (var item in items)
                _unitOfWork.MiniAppItems.Update(item);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
