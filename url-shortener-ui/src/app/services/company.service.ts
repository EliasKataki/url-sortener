import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// API'deki Token ve Url modellerine benzer interface'ler (veya doğrudan API modellerini paylaşmak daha iyi olabilir)
export interface Token {
  id: number;
  value: string;
  remainingUses: number;
  createdAt: string; // veya Date
  expiresAt?: string | null; // veya Date
}

export interface Url {
  id: number;
  longUrl: string;
  shortUrl: string;
  clickCount: number;
  createdAt: string; // veya Date
  expiresAt?: string | null; // eklendi
}

export interface Company {
  id: number;
  name: string;
  createdAt: string; // veya Date - Eklendi
  tokens?: Token[]; // Eklendi (API'den dizi olarak geliyor)
  urls?: Url[]; // Eklendi (API'den dizi olarak geliyor)
  // Eski alanlar kaldırıldı (tokenCount, totalUrls, totalClicks)
}

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private apiUrl = 'http://localhost:5161/api/company';

  constructor(private http: HttpClient) {}

  getCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(this.apiUrl);
  }

  addCompany(companyData: { companyName: string; tokenCount: number }): Observable<Company> {
    return this.http.post<Company>(this.apiUrl, companyData);
  }

  getCompanyDetails(companyId: number): Observable<Company> {
    return this.http.get<Company>(`${this.apiUrl}/${companyId}`);
  }

  // YENİ: Token kullanım hakkını güncelleme metodu
  updateTokenUses(tokenId: number, remainingUses: number): Observable<any> {
    const dto = { remainingUses: remainingUses };
    return this.http.put(`${this.apiUrl}/token/${tokenId}/uses`, dto);
  }

  // Firma silme
  deleteCompany(companyId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${companyId}`);
  }

  // Token silme
  deleteToken(tokenId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/token/${tokenId}`);
  }

  // URL silme
  deleteUrl(urlId: number): Observable<any> {
    return this.http.delete(`http://localhost:5161/${urlId}`);
  }

  // URL expiresAt güncelleme
  updateUrlExpiresAt(urlId: number, dto: { expiresAt: string | null }): Observable<any> {
    return this.http.put(`http://localhost:5161/${urlId}/expires`, dto);
  }
} 