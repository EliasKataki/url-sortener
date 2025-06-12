# Octa Shortener - URL Kısaltma ve Yönetim Sistemi

## Projenin Amacı

Octa Shortener, uzun URL'leri kısa ve kolay paylaşılabilir bağlantılara dönüştüren, şirket bazlı token yönetimi, kullanıcı rolleri ve detaylı tıklama istatistikleri sunan modern bir web uygulamasıdır. Proje, .NET 8.0 tabanlı bir API ve Angular tabanlı bir yönetim paneli içerir.

## Temel Özellikler
- Uzun URL'leri kısa linklere dönüştürme
- Şirket bazlı token yönetimi (token ile kısaltma yetkisi)
- Kullanıcı rolleri: SuperAdmin, Admin, User
- Rol bazlı yetkilendirme ve arayüz
- Tıklama istatistikleri ve harita üzerinde görselleştirme
- Konum zorunluluğu: Kısa linke tıklayan herkesin konum bilgisinin alınması zorunludur, aksi halde yönlendirme yapılmaz
- Modern, responsive ve kullanıcı dostu arayüz

## Sistem Gereksinimleri
- .NET 8.0 SDK
- Node.js 18+
- Angular CLI 19+
- SQL Server 2022 (Docker ile kolay kurulum önerilir)
- Modern bir web tarayıcısı

## Proje Yapısı
- **UrlShortener.API/** : .NET 8.0 tabanlı backend API
- **url-shortener-ui/** : Angular tabanlı frontend yönetim paneli

---

## Kurulum ve Çalıştırma

### 1. Veritabanı (SQL Server) Kurulumu
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SqlServer2024!" -p 1433:1433 --name mssql -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Backend (API) Kurulumu
```bash
cd UrlShortener.API
dotnet restore
dotnet ef database update
dotnet run
```
API varsayılan olarak `http://localhost:5161` adresinde çalışır.

### 3. Frontend (Angular) Kurulumu
```bash
cd url-shortener-ui
npm install
ng serve
```
Uygulama varsayılan olarak `http://localhost:4200` adresinde çalışır.

---

## Kullanıcı Rolleri ve Yetkiler
- **SuperAdmin:** Tüm şirket ve kullanıcı yönetimi, token işlemleri, link oluşturma ve silme yetkisi.
- **Admin:** Kendi şirketi için token ve link yönetimi, istatistik görüntüleme.
- **User:** Sadece admin/süper admin tarafından oluşturulmuş kısa linkleri görebilir ve tıklayabilir. Token, harita, link oluşturma, silme ve düzenleme yetkisi yoktur.

### Rol Bazlı Arayüz
- User'lar sade ve kısıtlı bir arayüz görür.
- Admin ve SuperAdmin'ler tüm yönetim ve istatistik panellerine erişebilir.

---

## Konum Zorunluluğu ve Güvenlik
- Kısa linke tıklayan herkesin konum bilgisinin (latitude, longitude, markerType) alınması zorunludur.
- Konum alınamazsa (ör. gizli sekme, izin verilmemesi), **yönlendirme yapılmaz** ve kullanıcıya uyarı gösterilir.
- Bu kontrol hem frontend'de hem de backend'de zorunlu tutulmuştur.
- Tüm tıklamalar IP, user-agent ve konum ile birlikte loglanır.

---

## API Endpointleri (Özet)
- `POST /api/url/shorten` : Uzun URL'yi kısaltır (token ile)
- `GET /{shortUrl}?latitude=...&longitude=...&markerType=...` : Kısa link yönlendirmesi (konum zorunlu)
- `GET /api/url/details/{id}` : Kısa link detayları ve istatistikleri
- `POST /api/auth/login` : Kullanıcı girişi
- `POST /api/auth/register` : Kullanıcı kaydı (SuperAdmin yetkisiyle)
- `GET /api/company` : Şirket ve token yönetimi

---

## Frontend (Angular) Kullanımı
- `ng serve` ile başlatılır.
- Rol bazlı arayüz otomatik olarak açılır.
- User'lar sadece kısa linkleri görebilir ve tıklayabilir.
- Admin/SuperAdmin token, link, şirket ve kullanıcı yönetimi yapabilir.
- Tüm işlemler için API ile güvenli iletişim sağlanır.

---

## Gelişmiş Özellikler ve Modern UX
- Responsive ve mobil uyumlu tasarım
- Kullanıcıya özel uyarı ve hata mesajları
- Token ve link işlemlerinde anlık bildirimler
- Harita üzerinde tıklama görselleştirmesi (admin/süper admin için)
- Kapsamlı loglama ve hata yönetimi

---

## Güvenlik ve Performans
- SQL injection ve XSS koruması
- JWT tabanlı kimlik doğrulama (gerekirse eklenebilir)
- API rate limit ve CORS ayarları
- Asenkron işlemler ve hızlı yanıt süreleri

---

## Sıkça Sorulan Sorular

**S: Konum izni vermezsem linke gidebilir miyim?**
C: Hayır, konum izni olmadan kısa linkin yönlendirdiği uzun linke erişemezsiniz.

**S: User rolü ile link oluşturabilir miyim?**
C: Hayır, sadece admin ve süper adminler link oluşturabilir.

**S: Tüm tıklamalar kaydediliyor mu?**
C: Evet, IP, user-agent ve konum ile birlikte tüm tıklamalar loglanır.

---

## Katkı ve Geliştirme
- Kodunuzu forkladıktan sonra PR gönderebilirsiniz.
- Hataları veya önerileri issue olarak açabilirsiniz.

---

## Lisans
MIT 