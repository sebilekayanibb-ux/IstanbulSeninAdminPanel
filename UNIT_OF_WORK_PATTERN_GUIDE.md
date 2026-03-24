/**
 * ================================================
 * UNIT OF WORK + REPOSITORY PATTERN ÖZET
 * ================================================
 * 
 * Katmanlı Mimaride Veri Erişimi Yönetimi
 * .NET 10 | C# | Entity Framework Core 10
 * 
 */

// ============================================
// 1. MIMARI KATMANLAR
// ============================================

/*
┌─────────────────────────────────┐
│   MVC (Presentation Layer)      │ ← Controller, View, Request/Response
├─────────────────────────────────┤
│   BLL (Business Logic Layer)    │ ← Services (NotificationService, SectionService...)
│                                 │   ✓ IUnitOfWork dependency injection
├─────────────────────────────────┤
│   DAL (Data Access Layer)       │ ← Repository, UnitOfWork implementations
│                                 │   ✓ Repository<T> generic class
│                                 │   ✓ UnitOfWork concrete class
├─────────────────────────────────┤
│   CORE (Domain/Entity Layer)    │ ← Entities, Interfaces (IRepository, IUnitOfWork)
└─────────────────────────────────┘

AKIŞ:
Controller → Service (IUnitOfWork inject) → Repository (DbSet wrapper) → DbContext → Database
*/

// ============================================
// 2. UNIT OF WORK PATTERN NEDİR?
// ============================================

/*
AMAÇ: Birden çok repository işlemini tek bir transaction'ın altında yönetmek

ÖNCESİ (Sorunlu):
public class NotificationService
{
    private readonly AppDbContext _context;
    
    public async Task SendAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        notification.IsSent = true;
        await _context.SaveChangesAsync();  // İlk save
        
        var log = new NotificationLog { ... };
        _context.NotificationLogs.Add(log);
        await _context.SaveChangesAsync();  // İkinci save
        
        // ✗ PROBLEM: 2 ayrı transaction - aralarında error olursa inconsistency
    }
}

SONRASI (Unit of Work Pattern):
public class NotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task SendAsync(int id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();  // Transaction başlat
            
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            notification.IsSent = true;
            _unitOfWork.Notifications.Update(notification);
            
            var log = new NotificationLog { ... };
            await _unitOfWork.NotificationLogs.AddAsync(log);
            
            await _unitOfWork.SaveChangesAsync();  // TEK save, atomic operation
            await _unitOfWork.CommitAsync();       // Transaction commit
            
            // ✓ İKİ OPERASYON AYNI TRANSACTION'DA - Atomicity garantili
        }
        catch
        {
            await _unitOfWork.RollbackAsync();  // Her şey geri alınır
            throw;
        }
    }
}
*/

// ============================================
// 3. REPOSITORY PATTERN NEDİR?
// ============================================

/*
AMAÇ: DbSet<T> işlemlerini soyutlamak, testability artırmak

INTERFACE (CORE/Repositories/IRepository.cs):
public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);
    void DeleteRange(IEnumerable<T> entities);
    Task<bool> AnyAsync(Func<T, bool> predicate);
    IQueryable<T> Query();  // Custom LINQ için
}

IMPLEMENTASYON (DAL/Repositories/Repository.cs):
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public virtual async Task<List<T>> GetAllAsync() => await _dbSet.ToListAsync();
    
    public virtual async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
    
    public virtual async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
    
    public virtual void Update(T entity) => _dbSet.Update(entity);
    
    public virtual void Delete(T entity) => _dbSet.Remove(entity);
    
    public virtual IQueryable<T> Query() => _dbSet.AsQueryable();
    
    // ... diğer metodlar
}
*/

// ============================================
// 4. UNIT OF WORK PATTERN ARAYÜZÜ
// ============================================

/*
INTERFACE (CORE/Repositories/IUnitOfWork.cs):
public interface IUnitOfWork : IDisposable
{
    // Repository Aggregation
    IRepository<Notification> Notifications { get; }
    IRepository<NotificationLog> NotificationLogs { get; }
    IRepository<Section> Sections { get; }
    IRepository<MiniAppItem> MiniAppItems { get; }
    
    // Transaction Management
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
    
    // Save Changes
    Task<int> SaveChangesAsync();
}

IMPLEMENTASYON (DAL/Repositories/UnitOfWork.cs):
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    
    // Lazy Loading - İlk kullanımda oluştur
    private IRepository<Notification>? _notifications;
    public IRepository<Notification> Notifications => 
        _notifications ??= new Repository<Notification>(_context);
    
    // Transaction Methods
    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();
    
    public async Task CommitAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null) await _transaction.CommitAsync();
        }
        finally
        {
            if (_transaction != null) await _transaction.DisposeAsync();
        }
    }
    
    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction != null) await _transaction.RollbackAsync();
        }
        finally
        {
            if (_transaction != null) await _transaction.DisposeAsync();
        }
    }
    
    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
*/

// ============================================
// 5. DEPENDENCY INJECTION (Program.cs)
// ============================================

/*
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

✓ Scoped Lifetime: Her HTTP request'te bir UnitOfWork instance'ı
✓ Tüm repositories aynı DbContext'i kullanır
✓ Transaction scope tek bir request içinde
*/

// ============================================
// 6. SERVIS KULLANIMI ÖRNEĞI
// ============================================

/*
public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<List<Notification>> GetAllAsync()
    {
        // Direkt repository erişimi
        var notifications = await _unitOfWork.Notifications.GetAllAsync();
        return notifications.OrderByDescending(n => n.CreatedAt).ToList();
    }
    
    public async Task<(bool, string)> CreateAsync(Notification notification)
    {
        // Validasyon
        if (string.IsNullOrWhiteSpace(notification.Title))
            return (false, "Başlık boş olamaz");
        
        // Repository'ye ekle
        await _unitOfWork.Notifications.AddAsync(notification);
        
        // Tek SaveChanges call
        await _unitOfWork.SaveChangesAsync();
        
        return (true, string.Empty);
    }
    
    public async Task<(bool, string)> SendAsync(int id)
    {
        try
        {
            // Transaction başlat
            await _unitOfWork.BeginTransactionAsync();
            
            // Bildirim getir
            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
            if (notification == null)
            {
                await _unitOfWork.RollbackAsync();
                return (false, "Bildirim bulunamadı");
            }
            
            // Durum güncelle
            notification.IsSent = true;
            notification.SentAt = DateTime.UtcNow;
            _unitOfWork.Notifications.Update(notification);
            
            // Log ekle
            var log = new NotificationLog
            {
                NotificationId = id,
                Status = "Success",
                SentAt = DateTime.UtcNow
            };
            await _unitOfWork.NotificationLogs.AddAsync(log);
            
            // Atomic save
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();  // Tüm değişiklikleri geri al
            return (false, ex.Message);
        }
    }
    
    public async Task DeleteAsync(int id)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(id);
        if (notification != null)
        {
            _unitOfWork.Notifications.Delete(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
*/

// ============================================
// 7. ADVANTAJları
// ============================================

/*
✓ TRANSACTION ATOMICITY
  Birden çok repository işlemi tek transaction'da
  
✓ TESTABILITY
  IUnitOfWork interface'ine mock verebiliriz
  DbContext'e doğrudan bağlı değiliz
  
✓ CODE REUSABILITY
  Generic Repository<T> ile code duplication ortadan kalkar
  
✓ SEPARATION OF CONCERNS
  Servisler sadece business logic'e odaklanır
  Data access logic repository'de
  
✓ CENTRALIZED TRANSACTION MANAGEMENT
  Tüm transaction yönetimi UnitOfWork'de
  
✓ LAZY LOADING
  Repositoriler ihtiyacında oluşturulur (??= operator)
  Performans kazancı
*/

// ============================================
// 8. REFACTORED SERVİSLER
// ============================================

/*
✅ NotificationService
   - AppDbContext → IUnitOfWork
   - Transaction wrapping in SendAsync
   - Atomic Notification + NotificationLog operations

✅ NotificationLogService  
   - AppDbContext → IUnitOfWork
   - Query() ile custom filtering
   - DeleteRange() ile batch deletion

✅ SectionService
   - AppDbContext → IUnitOfWork
   - Include() ile related data loading
   - Display order management

✅ MiniAppItemService
   - AppDbContext → IUnitOfWork
   - Include() ile related data loading
   - Active app filtering

⚠️ AdminUserService (UNCHANGED)
   - UserManager<T> / RoleManager<T> kullanıyor
   - Identity Framework'ün kendi transaction yönetimi var
   - UnitOfWork pattern'ına uymaz
   - Olduğu gibi bırakılması önerilir
*/

// ============================================
// 9. QUERY EXAMPLES
// ============================================

/*
// Include ile related data getirme
var sections = _unitOfWork.Sections.Query()
    .Include(x => x.Items)
    .OrderBy(x => x.DisplayOrder)
    .ToList();

// Where ile filtering
var activeMiniApps = _unitOfWork.MiniAppItems.Query()
    .Include(x => x.Sections)
    .Where(x => !x.IsHide && !x.IsTest)
    .OrderBy(x => x.DisplayOrder)
    .ToList();

// FirstOrDefault ile tek entity
var notification = await _unitOfWork.Notifications.Query()
    .FirstOrDefaultAsync(x => x.Id == id);

// GetAllAsync basit kullanım
var allNotifications = await _unitOfWork.Notifications.GetAllAsync();

// DeleteRange ile batch delete
var oldLogs = _unitOfWork.NotificationLogs.Query()
    .Where(x => x.SentAt < cutoffDate)
    .ToList();
_unitOfWork.NotificationLogs.DeleteRange(oldLogs);
await _unitOfWork.SaveChangesAsync();
*/

// ============================================
// 10. BEST PRACTICES
// ============================================

/*
1. ALWAYS USE ASYNC/AWAIT
   ✓ public async Task<List<T>> GetAllAsync()
   ✗ public List<T> GetAll()

2. TRANSACTION SCOPES
   ✓ await _unitOfWork.BeginTransactionAsync()
   ✗ El ile DbConnection açmak

3. ERROR HANDLING IN TRANSACTIONS
   ✓ try { ... await _unitOfWork.CommitAsync(); }
     catch { await _unitOfWork.RollbackAsync(); }

4. DEPENDENCY INJECTION
   ✓ public MyService(IUnitOfWork uow) { _unitOfWork = uow; }
   ✗ new AppDbContext()

5. QUERY COMPOSITION
   ✓ var query = _unitOfWork.Items.Query();
     if (condition) query = query.Where(...);
     var results = query.ToList();

6. BATCH OPERATIONS
   ✓ _unitOfWork.Repository.DeleteRange(items);
   ✗ foreach (var item in items) _unitOfWork.Repository.Delete(item);

7. SCOPED LIFETIME
   ✓ builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
   ✗ Transient veya Singleton
*/

// ============================================
// 11. DEBUGGING & LOGGING
// ============================================

/*
UnitOfWork'de SaveChangesAsync():
    ↓
AppDbContext'e SaveChangesAsync() call
    ↓
Entity Framework Validators
    ↓
SQL Generated & Executed
    ↓
Changes Tracked
    ↓
Logging (if configured)

Configuration (Program.cs):
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

Loggers in Services:
private readonly ILogger<NotificationService> _logger;

Usage:
_logger.LogInformation("Bildirim gönderildi: {Id}", notificationId);
_logger.LogError(ex, "Hata oluştu");
*/

// ============================================
// SONUÇ
// ============================================

/*
Unit of Work + Repository Pattern Katmanlı Mimaride:

├─ Problem: Doğrudan DbContext kullanımı
│           Multiple SaveChangesAsync() → Transaction issues
│           Test zor
│           Code duplication
│
├─ Çözüm: IRepository<T> arayüzü
│         Generic Repository<T> implementasyonu
│         IUnitOfWork aggregate
│         Merkezi transaction yönetimi
│
└─ Sonuç: Clean Architecture
         Testable Code
         Maintainable Services
         Atomic Transactions
         Reusable Components

✅ BUILD SUCCESSFUL - TÜM SERVİSLER REFACTORED
✅ DEPENDENCY INJECTION CONFIGURED
✅ PATTERN READY FOR PRODUCTION USE
*/
