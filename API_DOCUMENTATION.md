# İstanbul Seninle Admin Panel - API Dokumentasyonu 📚

## 🔗 API Base URL
```
Development: https://localhost:7xxx/api
Production: https://your-domain.com/api
```

## 🔐 Authentication
Tüm API endpoint'leri JWT Bearer token gerektirir.

```
Authorization: Bearer {jwt_token}
```

---

## 📱 Bildirim Endpoints

### 1. Tüm Bildirimleri Getir
```
GET /api/notifications
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "5 bildirim bulundu",
  "data": [
    {
      "id": 1,
      "title": "Pazartesi Etkinliği",
      "body": "Taksim'de yeni etkinlik açılacak",
      "targetAudience": "all",
      "isSent": true,
      "sentAt": "2026-03-26T10:30:00Z",
      "createdAt": "2026-03-25T15:00:00Z",
      "isTestMode": false
    }
  ],
  "timestamp": "2026-03-26T11:00:00Z"
}
```

---

### 2. Bildirim ID'ye Göre Getir
```
GET /api/notifications/{id}
```

**Örnek:**
```
GET /api/notifications/1
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Başarılı",
  "data": {
    "id": 1,
    "title": "Pazartesi Etkinliği",
    "body": "Taksim'de yeni etkinlik açılacak",
    "targetAudience": "all",
    "isSent": true,
    "sentAt": "2026-03-26T10:30:00Z",
    "createdAt": "2026-03-25T15:00:00Z",
    "isTestMode": false
  },
  "timestamp": "2026-03-26T11:00:00Z"
}
```

---

### 3. Gönderilmiş Bildirimleri Getir
```
GET /api/notifications/sent
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "3 gönderilmiş bildirim",
  "data": [
    { /* Bildirim nesnesi */ }
  ],
  "timestamp": "2026-03-26T11:00:00Z"
}
```

---

### 4. Beklemede Bildirimleri Getir
```
GET /api/notifications/pending
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "2 beklemede bildirim",
  "data": [
    { /* Bildirim nesnesi */ }
  ],
  "timestamp": "2026-03-26T11:00:00Z"
}
```

---

### 5. Bildirimi Gönder
```
POST /api/notifications/{id}/send
```

**Gerekli rol:** Admin veya SuperAdmin

**Örnek:**
```
POST /api/notifications/1/send
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Bildirim başarıyla gönderildi",
  "data": {
    "id": 1,
    "title": "Pazartesi Etkinliği",
    "body": "Taksim'de yeni etkinlik açılacak",
    "targetAudience": "all",
    "isSent": true,
    "sentAt": "2026-03-26T10:35:00Z",
    "createdAt": "2026-03-25T15:00:00Z",
    "isTestMode": false
  },
  "timestamp": "2026-03-26T11:00:00Z"
}
```

**Response (400 Bad Request):**
```json
{
  "success": false,
  "message": "Bu bildirim zaten gönderilmiş",
  "timestamp": "2026-03-26T11:00:00Z"
}
```

---

## 🧪 Test Bildirim Modu

Bildirim `IsTestMode = true` ile oluşturulduğunda:

1. **Backend'de:** Log kaydedilir (`Status = "Test"`)
2. **Dashboard'da:** "🧪 Test" badge gösterilir
3. **Firebase:** Test topic'ine gönderilir
4. **Başarı Oranı:** Hesaplanmaya katılmaz

**Test modunun amacı:**
- Bildirimleri göndermeden önce test etmek
- Firebase'e yetersiz istek göndermemek
- Güvenli test ortamı sağlamak

---

## 📊 Hedef Kitle Değerleri

| Değer | Anlam |
|-------|-------|
| `"all"` | Tüm kullanıcılar |
| `"guest"` | Sadece misafir kullanıcılar |
| `"regular"` | Sadece kayıtlı kullanıcılar |

---

## ❌ Hata Kodları

| HTTP | Kod | Anlam |
|-----|-----|-------|
| 400 | VALIDATION_ERROR | Geçerlilik hatası |
| 401 | UNAUTHORIZED | Yetkisiz erişim |
| 403 | FORBIDDEN | Yasak erişim |
| 404 | NOT_FOUND | Bulunamadı |
| 500 | INTERNAL_ERROR | Sunucu hatası |

---

## 🔄 Firebase Entegrasyonu (İlerde)

Mobil app geliştirilince Firebase entegrasyonu yapılacaktır:

1. **Mobil app** Firebase Cloud Messaging başlatır
2. **Device token** Backend'e gönderilir
3. **Bildirim gönderme** Firebase üzerinden yapılır
4. **Push notification** Mobil cihazlara ulaşır

**Bu değişiklikler:**
- ✅ API endpoint'leri **aynı kalacak**
- ✅ Response format'ı **aynı kalacak**
- ✅ Authentication **aynı kalacak**
- ❌ Arka planda sadece implementasyon değişecek

---

## 📝 CURL Örnekleri

### Login (Token Alma)
```bash
curl -X POST "https://localhost:7xxx/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "YourPassword123"
  }'
```

### Bildirimleri Listeleme
```bash
curl -X GET "https://localhost:7xxx/api/notifications" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Bildirim Gönderme
```bash
curl -X POST "https://localhost:7xxx/api/notifications/1/send" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

---

## 🚀 Swagger UI Kullanımı

Development ortamında:
```
https://localhost:7xxx/api-docs
```

Swagger UI'da:
1. "Authorize" butonuna tıkla
2. Bearer token gir
3. Endpoint'leri test et

---

## 📞 İletişim

Sorular için: developer@example.com
