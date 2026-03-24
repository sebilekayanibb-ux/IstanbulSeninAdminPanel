using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace IstanbulSenin.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        private IRepository<Notification>? _notificationRepository;
        private IRepository<NotificationLog>? _notificationLogRepository;
        private IRepository<Section>? _sectionRepository;
        private IRepository<MiniAppItem>? _miniAppItemRepository;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public IRepository<Notification> Notifications
        {
            get { return _notificationRepository ??= new Repository<Notification>(_context); }
        }

        public IRepository<NotificationLog> NotificationLogs
        {
            get { return _notificationLogRepository ??= new Repository<NotificationLog>(_context); }
        }

        public IRepository<Section> Sections
        {
            get { return _sectionRepository ??= new Repository<Section>(_context); }
        }

        public IRepository<MiniAppItem> MiniAppItems
        {
            get { return _miniAppItemRepository ??= new Repository<MiniAppItem>(_context); }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch
            {
                if (_transaction != null)
                    await RollbackAsync();
                throw;
            }
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            catch
            {
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}
