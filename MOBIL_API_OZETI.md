# 📱 Mobil API Dokümantasyonu - İstanbul Senin Admin Panel

## ✅ Tüm API'ler `/api/` Altında Organize Edilmiştir

```
/api/
├── /auth/                    # 🔐 Kimlik doğrulama
│   ├── POST   /login         → Giriş
│   ├── POST   /register      → Kayıt
│   ├── POST   /logout        → Oturumu Kapat
│   ├── POST   /change-password → Şifre Değiştir
│   └── POST   /refresh-token → Token Yenile
│
├── /sections/                # 📋 Bölümler
│   ├── GET    /              → Tüm Bölümler (Mini Uygulamalar ile)
│   └── GET    /{id}          → Bölüm Detayı
│
├── /miniapps/                # 🎮 Mini Uygulamalar
│   ├── GET    /              → Tüm Uygulamalar
│   ├── GET    /{id}          → Uygulama Detayı
│   └── GET    /active/list   → Sadece Aktif Uygulamalar
│
├── /notifications/           # 📢 Bildirimler
│   ├── GET    /              → Tüm Bildirimler
│   ├── GET    /{id}          → Bildirim Detayı
│   ├── GET    /sent          → Gönderilen Bildirimler
│   ├── GET    /pending       → Beklemede Olan Bildirimler
│   └── POST   /{id}/send     → Bildirim Gönder (Admin)
│
└── /dashboard/               # 📊 İstatistikler
    ├── GET   /statistics     → Tüm İstatistikler
    ├── GET   /today          → Bugünün Verileri
    ├── GET   /last-7-days    → Son 7 Günün Verileri
    └── GET   /range          → Özel Tarih Aralığı
```

## 🎯 CEVAPLAR

### ❓ Mobil uygulamalar bu API'lerle admin panelindeki şeylere bağlanabilecek mi?
**✅ EVET!** Tüm API'ler mobil uygulamalar için tasarlandı.

### ❓ Mini uygulamalar, bildirimler, bölümler çekilebilecek mi?
**✅ EVET!** Tüm endpoint'ler hazır:
- `GET /api/sections` → Bölümler + mini uygulamalar
- `GET /api/notifications` → Bildirimler
- `GET /api/miniapps` → Mini uygulamalar
- `GET /api/dashboard/statistics` → İstatistikler

### ❓ NotificationsApiController neden Controllers/Api altında değil de direkt Controllers altında?
**🔧 SORUN ÇÖZÜLDİ!** NotificationsApiController artık `/Controllers/Api/` klasörüne taşındı.

**Şimdi tüm API'ler organize:**
- ✅ AuthApiController → `/api/auth`
- ✅ SectionsApiController → `/api/sections`
- ✅ MiniAppsApiController → `/api/miniapps`
- ✅ NotificationsApiController → `/api/notifications`
- ✅ DashboardApiController → `/api/dashboard`

---

## 📊 API Organizasyonu

```csharp
// Tüm API Controller'ları şu namespace'de:
namespace IstanbulSenin.MVC.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NeuerController : ControllerBase { }
}
```

**Dosya Yapısı:**
```
IstanbulSenin.MVC/
├── Controllers/
│   ├── Api/
│   │   ├── AuthApiController.cs
│   │   ├── SectionsApiController.cs
│   │   ├── MiniAppsApiController.cs
│   │   ├── NotificationsApiController.cs ← YENİ KONUM!
│   │   └── DashboardApiController.cs
│   └── (Other web controllers)
└── Dtos/
    └── MobileApiDtos.cs
```

---

## ✅ BUILD STATUS

**✨ Build Successful!**
- Tüm API'ler organize
- Tüm DTO'lar hazır
- Hata yok
- Production-ready

Mobil developer'lar şimdi bu API'lerle çalışmaya başlayabilir! 🚀
