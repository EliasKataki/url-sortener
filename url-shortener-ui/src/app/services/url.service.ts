import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UrlService {
  // API URL yapılandırması
  private readonly apiUrl: string;

  constructor(private http: HttpClient) {
    // Eğer özel bir konfigürasyon olsaydı buradan alınabilirdi
    // Şimdilik varsayılan değeri kullanıyoruz
    this.apiUrl = 'http://localhost:5161';
  }

  shortenUrl(longUrl: string, token: string | null, expiresAt: string | null): Observable<any> {
    const dto = { longUrl: longUrl, token: token ?? "", expiresAt: expiresAt };
    return this.http.post(this.apiUrl, dto);
  }

  getUrlDetailsById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/details/${id}`);
  }

  deleteUrl(urlId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${urlId}`);
  }

  updateUrlExpiresAt(urlId: number, dto: { expiresAt: string | null }): Observable<any> {
    return this.http.put(`${this.apiUrl}/${urlId}/expires`, dto);
  }

  /*
  getUrlStats(shortUrl: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/stats/${shortUrl}`);
  }
  */
} 