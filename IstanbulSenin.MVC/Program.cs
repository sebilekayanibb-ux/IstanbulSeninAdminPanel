using IstanbulSenin.BLL.Services.AdminUsers;
using IstanbulSenin.BLL.Services.Auth;
using IstanbulSenin.BLL.Services.MiniApps;
using IstanbulSenin.BLL.Services.Notifications;
using IstanbulSenin.BLL.Services.Sections;
using IstanbulSenin.BLL.Services.Seeding;
using IstanbulSenin.CORE.Entities;
using IstanbulSenin.CORE.Repositories;
using IstanbulSenin.DAL;
using IstanbulSenin.DAL.Repositories;
using IstanbulSenin.HELPER.Constants;
using IstanbulSenin.MVC;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı bağlantısı (EnableRetryOnFailure eklendi)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql => sql.EnableRetryOnFailure()));

// 2. ASP.NET Core Identity — Güçlendirilmiş Güvenlik
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    // Şifre politikası — GÜÇLÜ
    options.Password.RequireDigit = true;                    // ✓ Rakam zorunlu
    options.Password.RequireUppercase = true;               // ✓ Büyük harf zorunlu
    options.Password.RequireNonAlphanumeric = false;        // ✓ Özel karakter istenmez
    options.Password.RequiredLength = 6;                   // ✓ En az 6 karakter

    // Hesap kilitleme — BRUTE-FORCE KORUMA
    options.Lockout.AllowedForNewUsers = true;              // ✓ Kilitleme aktif
    options.Lockout.MaxFailedAccessAttempts = 5;           // ✓ 5 başarısız deneme
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // ✓ 15 dakika kilitle

    // E-posta benzersiz olmalı
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<TurkishIdentityErrorDescriber>()
.AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

// 3. Cookie ayarları — Güvenli (HttpOnly, Secure, SameSite)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = AppConstants.Paths.LoginPath;
    options.AccessDeniedPath = AppConstants.Paths.AccessDeniedPath;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;

    // ✓ XSS saldırılarından koruma
    options.Cookie.HttpOnly = true;

    // ✓ HTTPS-only (üretim ortamı)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // ✓ CSRF saldırılarından koruma
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// SecurityStamp: kullanıcı silindiğinde veya rolü değiştiğinde cookie geçersiz sayılır
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.AddControllersWithViews();

// 4. Repository & Unit of Work (Dependency Injection)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 5. Servis kayıtları (Dependency Injection)
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IMiniAppItemService, MiniAppItemService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationLogService, NotificationLogService>();
builder.Services.AddScoped<ISeedingService, SeedingService>();

var app = builder.Build();

// Veritabanı migrationları ve başlangıç verilerini uygula
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Migrationları uygula (veritabanı yoksa oluştur)
        logger.LogInformation("→ Migrationlar uygulanıyor...");
        db.Database.Migrate();
        logger.LogInformation("✓ Migrationlar tamamlandı.");

        // 2. Varsayılan verileri (seed) oluştur
        logger.LogInformation("→ Varsayılan veriler oluşturuluyor...");
        var seedingService = scope.ServiceProvider.GetRequiredService<ISeedingService>();
        await seedingService.InitializeDefaultDataAsync();
        logger.LogInformation("✓ Varsayılan veriler oluşturuldu.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "✗ Veritabanı migration/seed sırasında hata: {Message}", ex.Message);
        // Uygulama başlayabilmeli, hata loglanmalı
    }
}

// 5. Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Authentication önce, Authorization sonra gelmeli
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// 6. Varsayılan rota
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Section}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
