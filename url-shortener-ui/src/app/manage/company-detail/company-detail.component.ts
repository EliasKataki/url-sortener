import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router'; // ActivatedRoute ID'yi almak için, RouterModule geri linki için
import { Company, CompanyService, Token, Url } from '../../services/company.service'; // Service ve interface'ler
import { UrlService } from '../../services/url.service'; // UrlService eklendi
import { Observable, switchMap, catchError, of } from 'rxjs'; // catchError ve of eklendi
import { FormsModule } from '@angular/forms'; // FormsModule eklendi
import * as L from 'leaflet';

@Component({
  selector: 'app-company-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule], // FormsModule eklendi
  templateUrl: './company-detail.component.html',
  styleUrls: ['./company-detail.component.scss']
})
export class CompanyDetailComponent implements OnInit {
  company$: Observable<Company | null> = of(null);
  errorMessage: string = '';
  // Token güncelleme için ek özellikler
  updatingTokenId: number | null = null;
  updateTokenError: string = '';
  updateTokenSuccess: string = '';

  // Sayfalama için ek alanlar
  pagedTokens: Token[] = [];
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 1;
  allTokens: Token[] = [];

  todayString: string = new Date().toISOString().split('T')[0];

  private map: L.Map | null = null;
  private markersLayer: L.LayerGroup | null = null;

  selectedUrlId: number | null = null;

  konumUyari: string = '';
  manuelKonum: {lat: number, lng: number} | null = null;

  constructor(
    private route: ActivatedRoute,
    private companyService: CompanyService,
    private urlService: UrlService // UrlService enjekte edildi
  ) {}

  ngOnInit(): void {
    console.log('[INIT] CompanyDetailComponent başlatıldı');
    this.company$ = this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (id) {
          this.errorMessage = '';
          console.log(`[API] Firma detayları isteniyor. ID: ${id}`);
          return this.companyService.getCompanyDetails(+id).pipe(
             catchError(err => {
                console.error(`[HATA][API] Firma detayları alınamadı. ID: ${id}`, err);
                this.errorMessage = 'Firma detayları alınamadı.';
                return of(null);
             })
          ); 
        } else {
          this.errorMessage = 'Firma ID bulunamadı.';
          console.warn('[UYARI] Firma ID bulunamadı.');
          return of(null);
        }
      })
    );

    this.company$.subscribe(company => {
      if (company && company.tokens) {
        this.allTokens = company.tokens;
        this.totalPages = Math.ceil(this.allTokens.length / this.pageSize);
        this.setPage(1);
        console.log(`[TOKEN] Toplam token: ${company.tokens.length}`);
      } else {
        this.allTokens = [];
        this.pagedTokens = [];
        this.totalPages = 1;
        this.currentPage = 1;
        console.log('[TOKEN] Token bulunamadı veya firma yok.');
      }
      setTimeout(() => this.initOrUpdateMap(company), 0);
    });
  }

  public initOrUpdateMap(company: Company | null) {
    console.log('initOrUpdateMap çağrıldı', { company, selectedUrlId: this.selectedUrlId });
    
    if (typeof window === 'undefined') {
      console.log('window undefined, harita başlatılamıyor');
      return;
    }
    
    const mapElement = document.getElementById('map');
    console.log('Map elementi:', mapElement);
    
    if (!mapElement) {
      console.log('Map elementi bulunamadı');
      if (this.map) {
        console.log('Eski harita temizleniyor');
        this.map.remove();
        this.map = null;
      }
      return;
    }
    
    if (!company) {
      console.log('Company null, harita başlatılamıyor');
      return;
    }

    // Eski harita varsa temizle
    if (this.map) {
      console.log('Eski harita temizleniyor');
      this.map.remove();
      this.map = null;
    }

    console.log('Yeni harita başlatılıyor');
    this.map = L.map('map').setView([39.0, 35.0], 5);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap'
    }).addTo(this.map);
    this.markersLayer = L.layerGroup().addTo(this.map);

    // Markerları güncelle
    if (this.markersLayer) {
      console.log('Markerlar temizleniyor');
      this.markersLayer.clearLayers();
    }
    
    const allClicks: any[] = [];
    if (company.urls) {
      let urlsToShow = company.urls;
      if (this.selectedUrlId) {
        console.log('Seçili URL için markerlar ekleniyor:', this.selectedUrlId);
        urlsToShow = company.urls.filter(u => u.id === this.selectedUrlId);
      } else {
        console.log('Tüm URL\'ler için markerlar ekleniyor');
      }
      
      for (const url of urlsToShow) {
        if (url.clicks) {
          for (const click of url.clicks) {
            if (
              click.latitude !== null &&
              click.latitude !== undefined &&
              click.longitude !== null &&
              click.longitude !== undefined
            ) {
              allClicks.push({
                lat: Number(click.latitude),
                lng: Number(click.longitude),
                url: url.shortUrl,
                time: click.clickedAt,
                markerType: click.markerType
              });
            }
          }
        }
      }
    }
    
    console.log('Toplam tıklama sayısı:', allClicks.length);
    
    allClicks.forEach(click => {
      let icon = redIcon;
      if (click.markerType === 'ip') {
        icon = greyIcon;
      }
      const marker = L.marker([click.lat, click.lng], { icon });
      marker.bindPopup(`URL: ${click.url}<br>Tarih: ${click.time}<br>Tip: ${click.markerType === 'ip' ? 'Yaklaşık (IP)' : 'Hassas (GPS)'}`);
      this.markersLayer?.addLayer(marker);
    });
    
    if (allClicks.length > 0 && this.map) {
      console.log('Harita sınırları ayarlanıyor');
      const group = new L.LatLngBounds(allClicks.map(c => [c.lat, c.lng]));
      this.map.fitBounds(group, { padding: [30, 30] });
    } else if (this.map) {
      console.log('Varsayılan harita görünümü ayarlanıyor');
      this.map.setView([39.0, 35.0], 5); // Türkiye ortası
    }

    if (this.map) {
      this.map.on('click', (e: any) => {
        if (confirm('Bu noktayı manuel konum olarak kaydetmek istiyor musunuz?')) {
          this.manuelKonum = {lat: e.latlng.lat, lng: e.latlng.lng};
          this.konumUyari = 'Manuel olarak seçilen konum kaydedilecek.';
          // Burada backend'e manuel konum kaydı için yönlendirme yapılabilir veya özel bir endpoint ile kaydedilebilir.
        }
      });
    }
  }

  setPage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    const start = (page - 1) * this.pageSize;
    const end = start + this.pageSize;
    this.pagedTokens = this.allTokens.slice(start, end);
  }

  updateTokenUses(token: Token): void {
    if (token.remainingUses < 0) {
        this.updateTokenError = `Token #${token.id} için kalan hak negatif olamaz.`;
        console.warn(`[TOKEN][UYARI] Token #${token.id} için kalan hak negatif!`);
        return;
    }
    console.log(`[TOKEN] Token #${token.id} kalan hak güncelleniyor: ${token.remainingUses}`);
    this.updatingTokenId = token.id;
    this.updateTokenError = '';
    this.updateTokenSuccess = '';

    this.companyService.updateTokenUses(token.id, token.remainingUses).subscribe({
      next: () => {
          this.updateTokenSuccess = `Token #${token.id} kalan hakkı başarıyla güncellendi.`;
          console.log(`[TOKEN][BAŞARILI] Token #${token.id} güncellendi.`);
          this.updatingTokenId = null;
          setTimeout(() => { this.updateTokenSuccess = ''; }, 3000);
      },
      error: (error: any) => {
          this.updateTokenError = `Token #${token.id} güncellenirken hata oluştu.`;
          if (error?.error) {
            this.updateTokenError += ` Detay: ${ typeof error.error === 'string' ? error.error : JSON.stringify(error.error) }`;
          }
          console.error(`[TOKEN][HATA] Token #${token.id} güncellenemedi`, error);
          this.updatingTokenId = null;
          this.refreshCompanyDetails(); 
          setTimeout(() => { this.updateTokenError = ''; }, 5000);
      }
    });
  }

  // Firma için URL kısaltma alanı için özellikler
  newLongUrl: string = '';
  newExpiresAt: string = '';
  selectedTokenValue: string | null = null;
  shorteningResult: string = '';
  shorteningError: string = '';

  shortenUrlForCompany(): void {
    this.shorteningError = '';
    this.shorteningResult = '';
    if (!this.newLongUrl || !this.selectedTokenValue || !this.newExpiresAt) {
      this.shorteningError = 'Lütfen kısaltılacak URL\'yi, son kullanma tarihini ve kullanılacak token\'i seçin.';
      console.warn('[URL KISALTMA][UYARI] URL, son kullanma tarihi veya token seçilmedi.');
      return;
    }
    console.log(`[URL KISALTMA] Kullanıcı yeni URL kısaltıyor: ${this.newLongUrl}, Token: ${this.selectedTokenValue}, Son Kullanma: ${this.newExpiresAt}`);
    this.urlService.shortenUrl(this.newLongUrl, this.selectedTokenValue, this.newExpiresAt).subscribe({
      next: (response: any) => {
        this.shorteningResult = response.shortUrl;
        this.newLongUrl = '';
        this.newExpiresAt = '';
        console.log(`[URL KISALTMA][BAŞARILI] Kısa URL oluşturuldu: ${this.shorteningResult}`);
        this.refreshCompanyDetails(); 
      },
      error: (error: any) => {
        console.error('[URL KISALTMA][HATA] Firma adına URL kısaltılırken hata:', error);
        this.shorteningError = 'URL kisaltilirken bir hata olustu.';
         if (error?.error) {
          this.shorteningError += ` Detay: ${ typeof error.error === 'string' ? error.error : JSON.stringify(error.error) }`;
        }
      }
    });
  }

  // Kopyala fonksiyonunu bu componente de ekleyelim
  copyToClipboard(urlToCopy: string) {
    if (urlToCopy) {
      navigator.clipboard.writeText(urlToCopy)
        .then(() => { 
          console.log('[KOPYALA] Link kopyalandı:', urlToCopy);
         })
        .catch(err => console.error('[KOPYALA][HATA] Kopyalanamadı:', err));
    }
  }

  refreshCompanyDetails() {
    this.company$ = this.route.params.pipe(
      switchMap(params => {
        const id = +params['id'];
        return this.companyService.getCompanyDetails(id).pipe(
          catchError(error => {
            this.errorMessage = 'Firma detayları alınamadı.';
            console.error(error);
            return of(null);
          })
        );
      })
    );
  }

  modalOpen = false;
  modalTitle = '';
  modalMessage = '';
  modalCallback: ((result: boolean) => void) | null = null;

  openModal(title: string, message: string, callback: (result: boolean) => void) {
    this.modalTitle = title;
    this.modalMessage = message;
    this.modalCallback = callback;
    this.modalOpen = true;
  }

  closeModal(result: boolean) {
    this.modalOpen = false;
    if (this.modalCallback) {
      this.modalCallback(result);
      this.modalCallback = null;
    }
  }

  deleteCompany(companyId: number) {
    console.warn(`[FİRMA][SİLME] Firma siliniyor. ID: ${companyId}`);
    this.openModal(
      'Firma Sil',
      'Bu firmayı ve tüm ilişkili verileri (tokenlar, url, tıklama vs.) silmek istediğinize emin misiniz?',
      (result: boolean) => {
        if (!result) return;
        this.companyService.deleteCompany(companyId).subscribe({
          next: () => {
            console.log(`[FİRMA][SİLME][BAŞARILI] Firma silindi. ID: ${companyId}`);
            window.location.href = '/manage/companies';
          },
          error: () => {
            console.error('[FİRMA][SİLME][HATA] Firma silinirken hata oluştu!');
          }
        });
      }
    );
  }

  deleteToken(tokenId: number) {
    console.warn(`[TOKEN][SİLME] Token siliniyor. ID: ${tokenId}`);
    this.openModal(
      'Token Sil',
      'Bu tokenı silmek istediğinize emin misiniz?',
      (result: boolean) => {
        if (!result) return;
        this.companyService.deleteToken(tokenId).subscribe({
          next: () => {
            console.log(`[TOKEN][SİLME][BAŞARILI] Token silindi. ID: ${tokenId}`);
            this.refreshCompanyDetails();
          },
          error: () => {
            console.error('[TOKEN][SİLME][HATA] Token silinirken hata oluştu!');
          }
        });
      }
    );
  }

  deleteUrl(urlId: number) {
    console.warn(`[URL][SİLME] URL siliniyor. ID: ${urlId}`);
    this.openModal(
      'URL Sil',
      'Bu URL kaydını silmek istediğinize emin misiniz?',
      (result: boolean) => {
        if (!result) return;
        this.companyService.deleteUrl(urlId).subscribe({
          next: () => {
            console.log(`[URL][SİLME][BAŞARILI] URL silindi. ID: ${urlId}`);
            this.refreshCompanyDetails();
          },
          error: () => {
            console.error('[URL][SİLME][HATA] URL silinirken hata oluştu!');
          }
        });
      }
    );
  }

  getShortUrl(): string {
    if (!this.shorteningResult) return '';
    if (this.shorteningResult.startsWith('http')) return this.shorteningResult;
    return `http://localhost:5161/${this.shorteningResult}`;
  }

  getFullShortUrl(shortCode: string): string {
    if (!shortCode) return '';
    if (shortCode.startsWith('http')) return shortCode;
    return `http://localhost:5161/${shortCode}`;
  }

  updateUrlExpiresAt(url: Url) {
    const expiresAt: string | null = url.expiresAt ? String(url.expiresAt) : null;
    this.companyService.updateUrlExpiresAt(url.id, { expiresAt }).subscribe({
      next: () => {
        this.refreshCompanyDetails();
      },
      error: (err) => {
        alert('Bitiş tarihi güncellenemedi!');
        console.error(err);
      }
    });
  }

  getIstanbulTime(utcString: string): string {
    if (!utcString) return '';
    const date = new Date(utcString);
    return date.toLocaleString('tr-TR', { 
      timeZone: 'Europe/Istanbul',
      hour12: false,
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  getLocalTime(utcString: string): string {
    if (!utcString) return '';
    const date = new Date(utcString);
    return date.toLocaleString('tr-TR', { 
      timeZone: 'Europe/Istanbul',
      hour12: false,
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  handleClick(url: Url, event: Event) {
    event.preventDefault();
    this.konumUyari = '';
    this.manuelKonum = null;
    const options = {
      enableHighAccuracy: true,
      timeout: 5000,
      maximumAge: 0
    };
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const lat = position.coords.latitude;
          const lng = position.coords.longitude;
          const accuracy = position.coords.accuracy;
          let markerType = 'gps';
          if (accuracy > 50) {
            this.konumUyari = `Konumunuzun hassasiyeti düşük (${Math.round(accuracy)} m). Yaklaşık konum alınacak.`;
            this.getIpBasedLocation(url);
            return;
          }
          this.konumUyari = `Konumunuzun hassasiyeti: ${Math.round(accuracy)} metre. (Hassas/GPS)`;
          this.redirectWithLocation(url, lat, lng, markerType);
        },
        (error) => {
          this.konumUyari = 'Konum alınamadı, yaklaşık konum alınacak.';
          this.getIpBasedLocation(url);
        },
        options
      );
    } else {
      this.konumUyari = 'Tarayıcı konum desteği yok, yaklaşık konum alınacak.';
      this.getIpBasedLocation(url);
    }
  }

  private getIpBasedLocation(url: Url) {
    // ipgeolocation.io örnek API anahtarı ile (kendi anahtarınızı kullanın)
    fetch('https://api.ipgeolocation.io/ipgeo?apiKey=demo')
      .then(response => response.json())
      .then(data => {
        const lat = parseFloat(data.latitude);
        const lng = parseFloat(data.longitude);
        const markerType = 'ip';
        this.konumUyari = 'Yaklaşık konum tespit edildi (IP tabanlı). Haritada sapma olabilir.';
        this.redirectWithLocation(url, lat, lng, markerType);
      })
      .catch(error => {
        this.konumUyari = 'Konum alınamadı. Lütfen tekrar deneyin.';
        alert('Konum alınamadı. Lütfen tekrar deneyin.');
      });
  }

  private redirectWithLocation(url: Url, lat: number, lng: number, markerType: string) {
    const redirectUrl = this.getFullShortUrl(url.shortUrl) + `?latitude=${lat}&longitude=${lng}&markerType=${markerType}`;
    window.open(redirectUrl, '_blank');
    setTimeout(() => {
      this.refreshCompanyDetails();
    }, 2000);
  }

  toggleUrlSelection(urlId: number, company: Company) {
    if (this.selectedUrlId === urlId) {
      this.selectedUrlId = null;
    } else {
      this.selectedUrlId = urlId;
    }
    setTimeout(() => this.initOrUpdateMap(company), 0);
  }
}

// Kırmızı marker ikonu tanımı
const redIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
});

// Gri marker ikonu tanımı (IP tabanlı için)
const greyIcon = new L.Icon({
  iconUrl: 'https://raw.githubusercontent.com/Concept211/Google-Maps-Markers/master/images/marker_grey.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
}); 