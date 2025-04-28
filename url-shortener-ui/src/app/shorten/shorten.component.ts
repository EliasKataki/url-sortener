import { Component } from '@angular/core';
import { CommonModule } from '@angular/common'; // Gerekli importlar
import { FormsModule } from '@angular/forms'; // FormsModule form için gerekli
import { UrlService } from '../services/url.service'; // Servisi import edelim

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
  stats: any = null;
  error: string = '';

  constructor(private urlService: UrlService) {}

  shortenUrl() {
    this.urlService.shortenUrl(this.longUrl, null).subscribe({
      next: (response: any) => {
        this.shortUrl = response.shortUrl;
        this.error = '';
      },
      error: (error: any) => {
        this.error = 'URL kısaltma işlemi başarısız oldu.';
        this.shortUrl = ''; // Hata durumunda kısa URL'yi temizle
        this.stats = null; // Hata durumunda istatistikleri temizle
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