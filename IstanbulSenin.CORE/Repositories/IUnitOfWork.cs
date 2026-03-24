using IstanbulSenin.CORE.Entities;

namespace IstanbulSenin.CORE.Repositories
{
    /// <summary>
    /// Unit of Work Pattern - Tüm repository'leri ve transaction yönetimini merkezi yönetir
    /// Bir işlemde birden çok entity değişikliği yapılıyorsa, hepsi aynı transaction'da kaydedilir
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Notification> Notifications { get; }
        IRepository<NotificationLog> NotificationLogs { get; }
        IRepository<Section> Sections { get; }
        IRepository<MiniAppItem> MiniAppItems { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        IQueryable<T> Query<T>() where T : class;
    }
}
