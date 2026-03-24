using IstanbulSenin.CORE.Entities;
using IstanbulSenin.HELPER.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IstanbulSenin.BLL.Services.Seeding
{
    /// <summary>
    /// Veritabanı başlangıç verilerini (seed) oluşturmak için servis
    /// - Rolleri oluşturur (ilk çalıştırmada)
    /// - Sistemde hiç SuperAdmin yoksa ilk SuperAdmin kullanıcısını oluşturur
    /// </summary>
    public class SeedingService : ISeedingService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SeedingService> _logger;

        public SeedingService(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            ILogger<SeedingService> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// Roller ve varsayılan SuperAdmin kullanıcısını oluşturur (idempotent)
        public async Task InitializeDefaultDataAsync()
        {
            try
            {
                // 1. Rolleri oluştur (yoksa)
                await EnsureRolesExistAsync();

                // 2. Sistemde hiç SuperAdmin yoksa ilk SuperAdmin'i oluştur
                await EnsureFirstSuperAdminExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Seed işlemi sırasında hata oluştu.");
                throw;
            }
        }

        /// Gerekli rolleri oluşturur (yoksa)
        private async Task EnsureRolesExistAsync()
        {
            foreach (var role in AppConstants.DefaultRoles)
            {
                var roleExists = await _roleManager.RoleExistsAsync(role);
                if (!roleExists)
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("✓ '{RoleName}' rolü oluşturuldu.", role);
                    }
                    else
                    {
                        _logger.LogWarning("✗ '{RoleName}' rolü oluşturulamadı.", role);
                    }
                }
            }
        }

        /// Sistemde hiç SuperAdmin yoksa ilk SuperAdmin'i oluşturur. (Sonraki SuperAdmin'ler manuel olarak AdminUserService üzerinden eklenir)
        private async Task EnsureFirstSuperAdminExistsAsync()
        {
            // Sistemde var olan SuperAdmin sayısını kontrol et
            var existingSuperAdmins = await _userManager.GetUsersInRoleAsync(AppConstants.Roles.SuperAdmin);

            // Eğer SuperAdmin varsa işlem yapma (idempotent)
            if (existingSuperAdmins.Count > 0)
            {
                _logger.LogInformation("✓ Sistemde {Count} adet SuperAdmin bulunmaktadır.", existingSuperAdmins.Count);
                return;
            }

            // appsettings.json'dan ilk SuperAdmin bilgilerini oku
            var superAdminConfig = _configuration.GetSection("DefaultSuperAdmin");
            var email = superAdminConfig?["Email"];
            var password = superAdminConfig?["Password"];

            // Konfigürasyon kontrol et
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("✗ appsettings.json'da 'DefaultSuperAdmin' konfigürasyonu eksik veya boş.");
                return;
            }

            // İlk SuperAdmin'i oluştur
            var superAdmin = new AppUser
            {
                FullName = "Super Admin",
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow // ✓ UtcNow kullanılıyor
            };

            var result = await _userManager.CreateAsync(superAdmin, password);
            if (result.Succeeded)
            {
                // SuperAdmin rolünü ata
                await _userManager.AddToRoleAsync(superAdmin, AppConstants.Roles.SuperAdmin);
                _logger.LogInformation("✓ İlk SuperAdmin kullanıcısı oluşturuldu: {Email}", email);
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("✗ SuperAdmin kullanıcı oluştururken hata: {Errors}", errors);
            }
        }
    }
}
