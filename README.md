# URL Kısaltma Projesi Dökümantasyonu

## Proje Hakkında

URL Kısaltma projesi, uzun URL'leri kısa ve kullanımı kolay bağlantılara dönüştüren bir web uygulamasıdır. Proje, .NET Core API ve Angular tabanlı bir arayüz ile oluşturulmuştur. Uygulama, URL kısaltma, yönlendirme, istatistik izleme, loglama, token yönetimi, firma yönetimi ve kısa linkler için bitiş tarihi (expiresAt) belirleme gibi gelişmiş özellikler sunar.

## Sistem Gereksinimleri

- .NET 8.0 SDK
- SQL Server 2022
- Docker (veritabanı için)
- Node.js ve npm (UI için)
- Angular CLI (UI için)
- Modern bir web tarayıcısı

## Uygulamayı Çalıştırma

Uygulama iki ayrı bileşenden oluştuğu için (API ve UI), iki ayrı terminal penceresi kullanarak çalıştırmanız gerekmektedir.

### 1. Terminal - API'yi Çalıştırma:
```bash
cd UrlShortener.API
dotnet restore
dotnet ef database update
dotnet run
```
API başarıyla başladığında http://localhost:5161 adresinde çalışacak ve Swagger UI'a http://localhost:5161/swagger adresinden erişebileceksiniz.

### 2. Terminal - Angular UI'ı Çalıştırma:
```bash
cd url-shortener-ui
npm install
ng serve
```
UI başarıyla başladığında http://localhost:4200 adresinde çalışacaktır.

**Not:** Angular CLI yüklü değilse, önce şu komutu çalıştırın:
```bash
npm install -g @angular/cli
```

## Mimari Yapı

Proje iki ana bileşenden oluşur:

1. **Backend API (UrlShortener.API)**: .NET Core API projesi
2. **Frontend (url-shortener-ui)**: Angular tabanlı kullanıcı arayüzü
3. **Veritabanı**: SQL Server 2022 (Docker konteynerinde çalışır)

### Veritabanı Şeması

Veritabanı aşağıdaki tablolardan oluşur:

- **Urls**: Kısaltılan URL'leri depolar
  - Id (int, PK)
  - LongUrl (nvarchar(max))
  - ShortUrl (nvarchar(max))
  - CreatedAt (datetime2)
  - ExpiresAt (datetime2, nullable)
  - ClickCount (int)
  - CompanyId (int, nullable, FK)

- **Tokens**: Firma bazlı token yönetimi
  - Id (int, PK)
  - Value (nvarchar)
  - RemainingUses (int)
  - CompanyId (int, FK)
  - CreatedAt (datetime2)
  - ExpiresAt (datetime2)

- **Companies**: Firma yönetimi
  - Id (int, PK)
  - Name (nvarchar)
  - CreatedAt (datetime2)

- **UrlClicks**: URL tıklama kayıtları
  - Id (int, PK)
  - UrlId (int, FK)
  - ClickedAt (datetime2)
  - IpAddress (nvarchar)
  - UserAgent (nvarchar)
  - Latitude (float, nullable)
  - Longitude (float, nullable)

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
| POST | / | Uzun URL'yi kısaltır (token opsiyonel) |
| GET | /{shortUrl} | Kısa URL'yi kullanarak yönlendirme yapar (expiresAt kontrolü dahil) |
| PUT | /{id}/expires | Kısa URL'nin bitiş tarihini günceller |
| DELETE | /{id} | Kısa URL'yi siler |
| GET | /details/{id} | Kısa URL detaylarını getirir |
| GET | /stats/{fullUrl} | URL istatistiklerini görüntüler |
| GET | /list | Tüm URL'leri listeler |
| GET | /list/memory | Bellek tabanlı URL listesini görüntüler |
| GET | /list/database | Veritabanından URL listesini alır |
| GET | /logs | Sistem loglarını görüntüler |

### Örnek İstek ve Yanıtlar

#### URL Kısaltma (POST /)

İstek:
```json
{
  "longUrl": "https://www.example.com",
  "token": "firmanintokeni" // opsiyonel
}
```
Yanıt:
```json
{
  "id": 1,
  "longUrl": "https://www.example.com",
  "shortUrl": "UJ15L2",
  "createdAt": "2025-04-28T08:17:00Z",
  "companyId": 1
}
```

#### Bitiş Tarihi Güncelleme (PUT /{id}/expires)
İstek:
```json
{
  "expiresAt": "2025-05-11T00:00:00.000Z"
}
```
Yanıt: `204 No Content`

#### Kısa Linke Tıklama (GET /{shortUrl})
- Eğer expiresAt dolmamışsa: Uzun linke yönlendirir.
- Eğer expiresAt geçmişse: `410 Gone` ve "Bu kısa linkin süresi doldu." mesajı döner.

## Frontend Özellikleri
- Firma ve token yönetimi
- Kısa linklerin bitiş tarihi (expiresAt) ayarlanabilir ve güncellenebilir
- Kısa linklerin tıklanma sayısı ve detayları görüntülenebilir
- Kullanıcı dostu, responsive Angular arayüzü

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

### Frontend Kurulumu

1. UI dizinine gidin: `cd url-shortener-ui`
2. Bağımlılıkları yükleyin: `npm install`
3. Angular uygulamasını başlatın: `ng serve`

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
   - Benzersiz 6 karakterlik bir kod oluşturulur
   - Veritabanına kaydedilir
   - Kısa URL döndürülür

2. URL yönlendirme:
   - Kısa kod alınır
   - Veritabanında aranır
   - expiresAt kontrolü yapılır
   - İstatistikler güncellenir
   - Orijinal URL'ye yönlendirilir

## Hata Yönetimi

API, aşağıdaki hata durumlarını ele alır:

- Geçersiz URL formatı
- Bulunamayan URL kodları
- Süresi dolmuş URL'ler (410 Gone)
- Veritabanı bağlantı hataları

Tüm hatalar loglanır ve uygun HTTP durum kodlarıyla birlikte hata mesajları döndürülür.

## Güvenlik Önlemleri

- SQL injection koruması (Entity Framework kullanımı)
- URL'lerin encode edilmesi
- Erişim logları ve IP adresi takibi

## Performans İyileştirmeleri

- Entity Framework için uygun indeksler
- API yanıt hızını optimize etmek için asenkron metodlar 

## Loglama Sistemi

### Backend (API) Logları
- Tüm önemli işlemler (URL oluşturma, yönlendirme, silme, token güncelleme, firma işlemleri, hata durumları) otomatik olarak loglanır.
- Loglar hem konsola hem de `UrlShortener.API/logs/` klasöründe günlük dosyalar halinde tutulur.
- Log formatı örneği:
  ```
  [2025-04-30 10:37:55.147 +03:00 INF] [COMPANY][CREATE] Yeni firma oluşturuluyor: Acme, Token adedi: 5
  [2025-04-30 10:37:55.234 +03:00 ERR] [TOKEN][UPDATE][INVALID] Negatif kullanım hakkı girildi. TokenID: 42
  ```
- Loglar sayesinde API'de ne olduğu, hangi işlemin hangi ID ile yapıldığı ve hata/başarı durumu kolayca takip edilebilir.

### Frontend (Angular) Logları
- Tüm önemli kullanıcı işlemleri, hata ve uyarılar tarayıcı konsoluna açıklayıcı şekilde loglanır.
- Örnek loglar:
  ```
  [TOKEN] Token #5 kalan hak güncelleniyor: 10
  [URL][SİLME][BAŞARILI] URL silindi. ID: 123
  [HARİTA][TIKLAMA][UYARI] Konum alınamadı, klasik yönlendirme yapılıyor.
  ```
- Geliştirici konsolunu açarak (F12) tüm işlemleri adım adım takip edebilirsiniz. 