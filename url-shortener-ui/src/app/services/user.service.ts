import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roleId: number;
  isActive: boolean;
  lastLoginAt: string | null;
  companyIds?: number[];
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl: string;

  constructor(private http: HttpClient) {
    this.apiUrl = `${environment.apiUrl}/users`;
  }

  getAllUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  updateUserRole(userId: string, roleId: number): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${userId}/role`, { roleId });
  }

  updateUserStatus(userId: string, isActive: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${userId}/status`, { isActive });
  }

  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${userId}`);
  }

  updateUserCompanies(userId: string, companyIds: number[]): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${userId}/companies`, companyIds);
  }
} 