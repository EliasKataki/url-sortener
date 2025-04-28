import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router'; // ActivatedRoute ID'yi almak için, RouterModule geri linki için
import { Company, CompanyService, Token, Url } from '../../services/company.service'; // Service ve interface'ler
import { UrlService } from '../../services/url.service'; // UrlService eklendi
import { Observable, switchMap, catchError, of } from 'rxjs'; // catchError ve of eklendi
import { FormsModule } from '@angular/forms'; // FormsModule eklendi

@Component({
  selector: 'app-company-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule], // FormsModule eklendi
  templateUrl: './company-detail.component.html',
  styleUrls: ['./company-detail.component.scss']
})
export class CompanyDetailComponent implements OnInit {
  company$: Observable<Company | null> | undefined;
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

  constructor(
    private route: ActivatedRoute,
    private companyService: CompanyService,
    private urlService: UrlService // UrlService enjekte edildi
  ) {}

  ngOnInit(): void {
    this.company$ = this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (id) {
          this.errorMessage = '';
          // Hata yakalama eklendi
          return this.companyService.getCompanyDetails(+id).pipe(
             catchError(err => {
                console.error('Firma detayları alınırken hata:', err);
                this.errorMessage = 'Firma detayları alınamadı.';
                return of(null); // Hata durumunda null observable dön
             })
          ); 
        } else {
          this.errorMessage = 'Firma ID bulunamadı.';
          return of(null); // Hata durumunda null observable dön
        }
      })
    );
    // Tokenlar için sayfalama
    this.company$.subscribe(company => {
      if (company && company.tokens) {
        this.allTokens = company.tokens;
        this.totalPages = Math.ceil(this.allTokens.length / this.pageSize);
        this.setPage(1);
      } else {
        this.allTokens = [];
        this.pagedTokens = [];
        this.totalPages = 1;
        this.currentPage = 1;
      }
    });
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
        // Optionally revert the value in the input?
        // this.refreshCompanyDetails(); // Revert the whole data if needed
        return;
    }

    console.log('Güncellenecek Token:', token.id, 'Yeni Kalan Hak:', token.remainingUses);
    this.updatingTokenId = token.id;
    this.updateTokenError = '';
    this.updateTokenSuccess = '';

    this.companyService.updateTokenUses(token.id, token.remainingUses).subscribe({
      next: () => {
          this.updateTokenSuccess = `Token #${token.id} kalan hakkı başarıyla güncellendi.`;
          console.log('Token güncellendi');
          this.updatingTokenId = null;
          // Başarı mesajını bir süre sonra temizle
          setTimeout(() => { this.updateTokenSuccess = ''; }, 3000);
          // Başarılı güncelleme sonrası listeyi yenilemeye gerek yok gibi, çünkü [(ngModel)] zaten güncel.
      },
      error: (error: any) => {
          this.updateTokenError = `Token #${token.id} güncellenirken hata oluştu.`;
          if (error?.error) { // API'den gelen hatayı göster
            this.updateTokenError += ` Detay: ${ typeof error.error === 'string' ? error.error : JSON.stringify(error.error) }`;
          }
          console.error('Token güncellenemedi', error);
          this.updatingTokenId = null;
          // Hata durumunda eski değeri geri yüklemek için listeyi yenileyebiliriz
          this.refreshCompanyDetails(); 
          // Hata mesajını bir süre sonra temizle
          setTimeout(() => { this.updateTokenError = ''; }, 5000);
      }
    });
  }

  // Firma için URL kısaltma alanı için özellikler
  newLongUrl: string = '';
  selectedTokenValue: string | null = null;
  shorteningResult: string = '';
  shorteningError: string = '';

  shortenUrlForCompany(): void {
    this.shorteningError = '';
    this.shorteningResult = '';
    if (!this.newLongUrl || !this.selectedTokenValue) {
      this.shorteningError = 'Lutfen kisaltilacak URL\'yi ve kullanilacak token\'i secin.';
      return;
    }

    this.urlService.shortenUrl(this.newLongUrl, this.selectedTokenValue).subscribe({
      next: (response: any) => {
        this.shorteningResult = response.shortUrl; // API yanıtından shortUrl alınıyor
        this.newLongUrl = ''; // Formu temizle
        // Başarı mesajı zaten HTML'de gösteriliyor.
        // Firma bilgisini yenilemek gerekebilir (token hakkı düştü)
        this.refreshCompanyDetails(); 
      },
      error: (error: any) => {
        console.error('Firma adına URL kısaltılırken hata:', error);
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
          console.log('Link kopyalandı:', urlToCopy);
         })
        .catch(err => console.error('Kopyalanamadı:', err));
    }
  }

  refreshCompanyDetails(): void {
     // Mevcut ID ile firma detaylarını tekrar çekmek için ngOnInit'teki logic tekrar çalıştırılır
     // Daha iyi bir yöntem state management kullanmak olabilir ama şimdilik bu yeterli
     this.ngOnInit(); 
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
    this.openModal(
      'Firma Sil',
      'Bu firmayı ve tüm ilişkili verileri (tokenlar, url, tıklama vs.) silmek istediğinize emin misiniz?',
      (result: boolean) => {
        if (!result) return;
        this.companyService.deleteCompany(companyId).subscribe({
          next: () => {
            window.location.href = '/manage/companies';
          },
          error: () => {
            alert('Firma silinirken hata oluştu!');
          }
        });
      }
    );
  }

  deleteToken(tokenId: number) {
    this.openModal(
      'Token Sil',
      'Bu tokenı silmek istediğinize emin misiniz?',
      (result: boolean) => {
        if (!result) return;
        this.companyService.deleteToken(tokenId).subscribe({
          next: () => {
            this.refreshCompanyDetails();
          },
          error: () => {
            alert('Token silinirken hata oluştu!');
          }
        });
      }
    );
  }

  deleteUrl(urlId: number) {
    this.openModal(
      'URL Sil',
      'Bu URL kaydını silmek istediğinize emin misiniz?',
      (result: boolean) => {
        if (!result) return;
        this.companyService.deleteUrl(urlId).subscribe({
          next: () => {
            this.refreshCompanyDetails();
          },
          error: () => {
            alert('URL silinirken hata oluştu!');
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
    let expiresAt = url.expiresAt ? new Date(url.expiresAt) : null;
    if (expiresAt) {
      expiresAt.setHours(0, 0, 0, 0);
    }
    const isoDate = expiresAt ? expiresAt.toISOString() : null;
    this.companyService.updateUrlExpiresAt(url.id, { expiresAt: isoDate }).subscribe({
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
    return date.toLocaleString('tr-TR', { timeZone: 'Europe/Istanbul', hour12: false });
  }

  getLocalTime(utcString: string): string {
    if (!utcString) return '';
    const date = new Date(utcString);
    date.setHours(date.getHours() + 3); // Türkiye için 3 saat ekle
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hour = String(date.getHours()).padStart(2, '0');
    const minute = String(date.getMinutes()).padStart(2, '0');
    return `${day}.${month}.${year} ${hour}:${minute}`;
  }
} 