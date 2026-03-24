namespace IstanbulSenin.CORE.Repositories
{
    /// Generic Repository pattern - Tüm entity işlemleri için
    public interface IRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<bool> AnyAsync(Func<T, bool> predicate);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        IQueryable<T> Query();
    }
}
