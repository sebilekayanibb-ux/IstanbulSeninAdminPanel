using IstanbulSenin.BLL.Services.AdminUsers;
using IstanbulSenin.BLL.Services.Auth;
using IstanbulSenin.BLL.Services.Dashboard;
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
using IstanbulSenin.MVC.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure())
    .ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<TurkishIdentityErrorDescriber>()
.AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = AppConstants.Paths.LoginPath;
    options.AccessDeniedPath = AppConstants.Paths.AccessDeniedPath;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.Zero;
});

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "İstanbul Senin Admin Panel API",
        Version = "v1",
        Description = "Mobil app ve 3. party entegrasyonlar için REST API"
    });

    var xmlFile = Path.Combine(AppContext.BaseDirectory, "IstanbulSenin.MVC.xml");
    if (File.Exists(xmlFile))
        c.IncludeXmlComments(xmlFile);
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<IMiniAppItemService, MiniAppItemService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<INotificationLogService, NotificationLogService>();
builder.Services.AddScoped<INotificationSendingService, MockNotificationSender>();
builder.Services.AddScoped<ISeedingService, SeedingService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

var app = builder.Build();

// Veritabanı migrationları ve başlangıç verilerini uygula
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        logger.LogInformation("Database migrations started...");
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
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "İstanbul Senin API v1");
        c.RoutePrefix = "swagger";
    });
}

// ✅ GÜVENLIK: Global Exception Handler
app.UseGlobalExceptionHandler();

// ✅ GÜVENLIK: Rate Limiting (Brute-force koruması)
app.UseRateLimiting();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Section}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
