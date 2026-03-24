namespace IstanbulSenin.BLL.Services.Seeding
{
    /// Veritabanı başlangıç verilerini (seed) oluşturmak için arayüz
    public interface ISeedingService
    {
        /// Roller ve varsayılan SuperAdmin kullanıcısını oluşturur (idempotent)
        Task InitializeDefaultDataAsync();
    }
}
