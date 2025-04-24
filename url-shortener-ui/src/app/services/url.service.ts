import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UrlService {
  private apiUrl = 'http://localhost:5161/api/url';

  constructor(private http: HttpClient) { }

  shortenUrl(longUrl: string): Observable<any> {
    const headers = new HttpHeaders().set('Content-Type', 'application/json');
    return this.http.post(`${this.apiUrl}/shorten`, { url: longUrl }, { 
      headers: headers,
      withCredentials: true 
    });
  }

  getUrlStats(shortUrl: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/stats/${shortUrl}`, { 
      withCredentials: true 
    });
  }
} 