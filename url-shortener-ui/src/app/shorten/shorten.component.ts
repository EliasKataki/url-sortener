import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // Gerekli importlar
import { FormsModule } from '@angular/forms'; // FormsModule form için gerekli
import { UrlService } from '../services/url.service'; // Servisi import edelim
import { CompanyService, Company, Token } from '../services/company.service';

@Component({
  selector: 'app-shorten',
  standalone: true,
  imports: [CommonModule, FormsModule], // Modülleri ekleyelim
  templateUrl: './shorten.component.html',
  styleUrls: ['./shorten.component.scss']
})
export class ShortenComponent {
  longUrl: string = '';
  shortUrl: string = '';
  expiresAt: string = '';
  todayString: string = new Date().toISOString().split('T')[0];
  stats: any = null;
  error: string = '';

  companies: Company[] = [];
  selectedCompanyId: number | null = null;
  tokens: Token[] = [];
  selectedTokenValue: string | null = null;

  constructor(private urlService: UrlService, private companyService: CompanyService) {
    this.loadCompanies();
  }

  loadCompanies() {
    this.companyService.getCompanies().subscribe({
      next: (companies) => {
        this.companies = companies;
      },
      error: (err) => {
        this.error = 'Firma listesi alınamadı.';
      }
    });
  }

  onCompanyChange() {
    const company = this.companies.find(c => c.id == this.selectedCompanyId);
    this.tokens = company?.tokens || [];
    this.selectedTokenValue = null;
  }

  shortenUrl() {
    if (!this.longUrl || !this.expiresAt || !this.selectedCompanyId || !this.selectedTokenValue) {
      this.error = 'Lütfen tüm alanları doldurun (URL, firma, token, tarih).';
      return;
    }
    this.urlService.shortenUrl(this.longUrl, this.selectedTokenValue, this.expiresAt).subscribe({
      next: (response: any) => {
        this.shortUrl = response.shortUrl;
        this.error = '';
        this.longUrl = '';
        this.expiresAt = '';
        this.selectedCompanyId = null;
        this.selectedTokenValue = null;
        this.tokens = [];
      },
      error: (error: any) => {
        this.error = 'URL kısaltma işlemi başarısız oldu.';
        this.shortUrl = '';
        this.stats = null;
        console.error(error);
      }
    });
  }

  getStats() {
    // Bu method API ile uyumlu olmadığı için geçici olarak devre dışı bırakıldı veya silinebilir.
    /* 
    // shortUrl boş değilse istatistikleri al
    if (this.shortUrl) {
      this.urlService.getUrlStats(this.shortUrl).subscribe({
        next: (response: any) => {
          this.stats = response;
        },
        error: (error: any) => {
          this.stats = null; // Hata durumunda istatistikleri temizle
          console.error('İstatistik alınamadı:', error);
        }
      });
    } else {
      this.stats = null; // shortUrl boşsa istatistikleri temizle
    }
    */
  }

  copyToClipboard(urlToCopy: string) {
    if (urlToCopy) {
      navigator.clipboard.writeText(urlToCopy)
        .then(() => { 
          console.log('Link kopyalandı:', urlToCopy);
         })
        .catch(err => console.error('Kopyalanamadı:', err));
    }
  }

  getShortUrl(): string {
    if (!this.shortUrl) return '';
    // Eğer shortUrl tam url ise direkt döndür, değilse ana domain ile birleştir
    if (this.shortUrl.startsWith('http')) return this.shortUrl;
    return `http://localhost:5161/${this.shortUrl}`;
  }
} 