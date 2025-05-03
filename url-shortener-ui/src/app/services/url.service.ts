import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UrlService {
  private apiUrl = 'http://localhost:5161';

  constructor(private http: HttpClient) { }

  shortenUrl(longUrl: string, token: string | null, expiresAt: string): Observable<any> {
    const dto = { longUrl: longUrl, token: token ?? "", expiresAt: expiresAt };
    return this.http.post(this.apiUrl, dto);
  }

  getUrlDetailsById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/details/${id}`);
  }

  /*
  getUrlStats(shortUrl: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/stats/${shortUrl}`);
  }
  */
} 