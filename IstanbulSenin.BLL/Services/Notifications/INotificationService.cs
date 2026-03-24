using IstanbulSenin.CORE.Entities;

namespace IstanbulSenin.BLL.Services.Notifications
{
    public interface INotificationService
    {
        Task<List<Notification>> GetAllAsync();
        Task<(bool Success, string Error)> CreateAsync(Notification notification);
        Task<(bool Success, string Error)> UpdateAsync(int id, Notification notification);
        Task<(bool Success, string Error)> SendAsync(int id);
        Task<(bool Success, string Error)> DeleteAsync(int id);
    }
}
