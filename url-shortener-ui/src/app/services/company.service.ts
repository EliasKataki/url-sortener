import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { map, throwError } from 'rxjs';

// API'deki Token ve Url modellerine benzer interface'ler (veya doğrudan API modellerini paylaşmak daha iyi olabilir)
export interface Token {
  id: number;
  value: string;
  remainingUses: number;
  createdAt: string; // veya Date
  expiresAt: string | null; // veya Date
}

export interface UrlClick {
  id: number;
  ipAddress: string;
  userAgent: string;
  country: string;
  city: string;
  clickedAt: string;
  latitude?: number;
  longitude?: number;
  markerType?: string; // GPS veya IP
}

export interface Url {
  id: number;
  longUrl: string;
  shortUrl: string;
  clickCount: number;
  createdAt: string; // veya Date
  expiresAt: string | null; // eklendi
  clicks?: UrlClick[];
  isForever?: boolean; // UI için eklendi
}

export interface Company {
  id: number;
  name: string;
  createdAt: string;
  tokens?: Token[];
  urls?: Url[];
}

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private readonly apiUrl: string;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {
    this.apiUrl = `${environment.apiUrl}/company`;
  }

  getCompanies(): Observable<Company[]> {
    return this.http.get<Company[]>(this.apiUrl);
  }

  getCompanyById(id: number): Observable<Company> {
    return this.http.get<Company>(`${this.apiUrl}/${id}`);
  }

  createCompany(companyName: string, tokenCount: number): Observable<Company> {
    return this.http.post<Company>(this.apiUrl, { companyName, tokenCount });
  }

  updateTokenUses(tokenId: number, remainingUses: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/token/${tokenId}/uses`, { remainingUses });
  }

  deleteToken(tokenId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/token/${tokenId}`);
  }

  deleteCompany(companyId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${companyId}`);
  }
} 