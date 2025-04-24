import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UrlService } from './services/url.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, FormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  longUrl: string = '';
  shortUrl: string = '';
  stats: any = null;
  error: string = '';

  constructor(private urlService: UrlService) {}

  shortenUrl() {
    this.urlService.shortenUrl(this.longUrl).subscribe({
      next: (response: any) => {
        this.shortUrl = response.shortUrl;
        this.error = '';
        this.getStats();
      },
      error: (error) => {
        this.error = 'URL kısaltma işlemi başarısız oldu.';
        console.error(error);
      }
    });
  }

  getStats() {
    if (this.shortUrl) {
      this.urlService.getUrlStats(this.shortUrl).subscribe({
        next: (response: any) => {
          this.stats = response;
        },
        error: (error) => {
          console.error(error);
        }
      });
    }
  }

  copyToClipboard() {
    navigator.clipboard.writeText(this.shortUrl);
  }
}
