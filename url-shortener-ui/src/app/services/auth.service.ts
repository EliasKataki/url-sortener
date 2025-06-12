import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    roleId: number;
    isActive: boolean;
    lastLoginAt: string;
    createdAt: string;
    companyIds: number[];
  }
}

export interface ApiResponse {
  success: boolean;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private apiUrl = 'http://localhost:5161/api/auth';
  private authReadySubject = new BehaviorSubject<boolean>(false);
  public authReady$ = this.authReadySubject.asObservable();

  constructor(private http: HttpClient) {
    // Constructor'da token kontrolü yap
    const token = localStorage.getItem('token');
    if (token) {
      this.authReadySubject.next(true);
    }
  }

  register(data: RegisterRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(`${this.apiUrl}/register`, data);
  }

  login(data: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, data).pipe(
      map(response => {
        console.log('Login response:', response);
        if (response.user) {
          this.setAuthData(response.token, response.user);
        }
        return response;
      })
    );
  }

  setAuthData(token: string, userInfo: any): void {
    console.log('Setting auth data, userInfo:', userInfo);
    localStorage.setItem('token', token);
    localStorage.setItem('userInfo', JSON.stringify({
      ...userInfo,
      companyIds: userInfo.companyIds || []
    }));
    this.authReadySubject.next(true);
  }

  isAuthenticated(): boolean {
    const isAuth = !!localStorage.getItem('token');
    if (isAuth) {
      this.authReadySubject.next(true);
    }
    return isAuth;
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userInfo');
    this.authReadySubject.next(false);
  }

  // Kullanıcı rollerini kontrol etmek için yeni metodlar
  getCurrentUser(): any {
    const userInfo = localStorage.getItem('userInfo');
    if (userInfo) {
      try {
        return JSON.parse(userInfo);
      } catch {
        return null;
      }
    }
    return null;
  }

  isSuperAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.roleId === 1;
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.roleId === 2;
  }

  isUser(): boolean {
    const user = this.getCurrentUser();
    return user?.roleId === 3;
  }

  getUserCompanies(): number[] {
    const user = this.getCurrentUser();
    console.log('Getting user companies from storage:', user);
    return user?.companyIds || [];
  }
} 