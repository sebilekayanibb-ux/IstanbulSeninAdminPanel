using IstanbulSenin.CORE.Entities;

namespace IstanbulSenin.BLL.Services.Sections
{
    public interface ISectionService
    {
        Task<List<Section>> GetSectionsWithItemsAsync();

        Task<Section?> GetSectionDetailsAsync(int id);

        Task CreateSectionAsync(Section section);

        Task UpdateSectionAsync(Section section);

        Task RemoveSectionAsync(int id);

        Task ReorderAsync(List<int> orderedIds);
    }
}