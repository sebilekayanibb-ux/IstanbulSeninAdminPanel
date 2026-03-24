using IstanbulSenin.CORE.Entities;

namespace IstanbulSenin.BLL.Services.MiniApps
{
    public interface IMiniAppItemService
    {
        Task<List<MiniAppItem>> GetMiniAppsWithSectionsAsync();

        Task<MiniAppItem?> GetMiniAppDetailsAsync(int id);
        Task CreateMiniAppAsync(MiniAppItem item);
        Task UpdateMiniAppAsync(MiniAppItem item);
        Task RemoveMiniAppAsync(int id);

        Task ReorderAsync(List<int> orderedIds);
    }
}