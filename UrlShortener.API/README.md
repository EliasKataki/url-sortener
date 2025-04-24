# URL Kısaltma Projesi Dökümantasyonu

## Proje Hakkında

URL Kısaltma projesi, uzun URL'leri kısa ve kullanımı kolay bağlantılara dönüştüren bir web uygulamasıdır. Proje, .NET Core API ve veritabanı backend ile oluşturulmuştur. Uygulama, URL kısaltma, yönlendirme, istatistik izleme ve loglama özellikleri sunar.

## Sistem Gereksinimleri

- .NET 8.0 SDK
- SQL Server 2022
- Docker (veritabanı için)
- Modern bir web tarayıcısı

## Mimari Yapı

Proje iki ana bileşenden oluşur:

1. **Backend API (UrlShortener.API)**: .NET Core API projesi
2. **Veritabanı**: SQL Server 2022 (Docker konteynerinde çalışır)

### Veritabanı Şeması

Veritabanı aşağıdaki tablolardan oluşur:

- **Urls**: Kısaltılan URL'leri depolar
  - Id (int, PK)
  - LongUrl (nvarchar(max))
  - ShortUrl (nvarchar(max))
  - CreatedDate (datetime2)
  - ExpirationDate (datetime2)

- **UrlAccesses**: URL erişim kayıtlarını takip eder
  - Id (int, PK)
  - UrlId (int, FK)
  - AccessDate (datetime2)
  - IsSuccessful (bit)
  - ErrorMessage (nvarchar(max), nullable)
  - UserAgent (nvarchar(max), nullable)
  - IpAddress (nvarchar(max), nullable)

- **UrlLogs**: Sistemdeki tüm URL işlemlerini loglar
  - Id (int, PK)
  - LongUrl (nvarchar(max))
  - ShortUrl (nvarchar(max))
  - CreatedAt (datetime2)
  - Operation (nvarchar(max))
  - UserAgent (nvarchar(max), nullable)
  - IpAddress (nvarchar(max), nullable)
  - IsSuccessful (bit)
  - ErrorMessage (nvarchar(max), nullable)

## API Endpoint'leri

API aşağıdaki endpoint'leri sunar:

| HTTP Metodu | Endpoint | Açıklama |
|-------------|----------|----------|
| POST | /api/url/shorten | Uzun URL'yi kısaltır |
| GET | /api/url/{code} | Kısa URL'yi kullanarak yönlendirme yapar |
| GET | /api/url/stats/{fullUrl} | URL istatistiklerini görüntüler |
| GET | /api/url/list | Tüm URL'leri listeler |
| GET | /api/url/list/memory | Bellek tabanlı URL listesini görüntüler |
| GET | /api/url/list/database | Veritabanından URL listesini alır |
| GET | /api/url/logs | Sistem loglarını görüntüler |

### Örnek İstek ve Yanıtlar

#### URL Kısaltma (POST /api/url/shorten)

İstek:
```json
{
  "url": "https://www.example.com"
}
```

Yanıt:
```json
{
  "shortUrl": "http://localhost:5161/api/url/abC12345"
}
```

#### URL İstatistikleri (GET /api/url/stats/{fullUrl})

Yanıt:
```json
{
  "originalUrl": "https://www.example.com",
  "shortCode": "abC12345",
  "shortUrl": "http://localhost:5161/api/url/abC12345",
  "createdDate": "2025-04-22T10:30:00",
  "expirationDate": "2025-05-22T10:30:00",
  "totalAccesses": 5,
  "successfulAccesses": 4,
  "failedAccesses": 1,
  "lastAccess": "2025-04-22T15:45:00",
  "accessDetails": [
    {
      "accessDate": "2025-04-22T15:45:00",
      "isSuccessful": true,
      "userAgent": "Mozilla/5.0...",
      "ipAddress": "127.0.0.1"
    }
  ]
}
```

## Kurulum Adımları

### Veritabanı Kurulumu

1. Docker'ı yükleyin ve çalıştırın
2. SQL Server konteynerini başlatın:
```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SqlServer2024!" -p 1433:1433 --name mssql -d mcr.microsoft.com/mssql/server:2022-latest
```

### API Kurulumu

1. Projeyi klonlayın
2. API dizinine gidin: `cd UrlShortener.API`
3. Bağımlılıkları yükleyin: `dotnet restore`
4. Migration'ları uygulayın: `dotnet ef database update`
5. API'yi çalıştırın: `dotnet run`

## Geliştirici Notları

### CORS Yapılandırması

API, aşağıdaki origin'lerden gelen isteklere CORS desteği sağlar:
- http://localhost:4200
- http://localhost:3000
- http://localhost:8080
- http://127.0.0.1:4200
- http://127.0.0.1:3000
- http://127.0.0.1:8080

Yeni bir origin eklemek için `Program.cs` dosyasında CORS ayarlarını güncelleyin.

### URL İşleme Mantığı

1. URL kısaltma:
   - Uzun URL alınır
   - http:// veya https:// yoksa https:// eklenir
   - URL encode edilir
   - Benzersiz 8 karakterlik bir kod oluşturulur
   - Veritabanına kaydedilir
   - Kısa URL döndürülür

2. URL yönlendirme:
   - Kısa kod alınır
   - Veritabanında aranır
   - Süre kontrolü yapılır
   - İstatistikler güncellenir
   - Orijinal URL'ye yönlendirilir

## Hata Yönetimi

API, aşağıdaki hata durumlarını ele alır:

- Geçersiz URL formatı
- Bulunamayan URL kodları
- Süresi dolmuş URL'ler
- Veritabanı bağlantı hataları

Tüm hatalar loglanır ve uygun HTTP durum kodlarıyla birlikte hata mesajları döndürülür.

## Güvenlik Önlemleri

- SQL injection koruması (Entity Framework kullanımı)
- URL'lerin encode edilmesi
- Erişim logları ve IP adresi takibi

## Performans İyileştirmeleri

- Entity Framework için uygun indeksler
- API yanıt hızını optimize etmek için asenkron metodlar 