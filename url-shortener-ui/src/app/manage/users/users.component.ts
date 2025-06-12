import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { UserService, User } from '../../services/user.service';
import { HttpClientModule } from '@angular/common/http';
import { Company, CompanyService } from '../../services/company.service';
import { AuthService } from '../../services/auth.service';
import { filter, take } from 'rxjs/operators';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, HttpClientModule],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4"><i class="fas fa-users me-2 text-warning"></i> Kullanıcı Yönetimi</h2>
      
      <div class="card shadow-sm">
        <div class="card-body p-0">
          <div class="table-responsive">
            <table class="table table-hover align-middle mb-0">
              <thead class="bg-light">
                <tr>
                  <th class="px-4 py-3">Ad Soyad</th>
                  <th class="px-4 py-3">Email</th>
                  <th class="px-4 py-3">Rol</th>
                  <th class="px-4 py-3">Durum</th>
                  <th class="px-4 py-3">Son Giriş</th>
                  <th class="px-4 py-3 text-end">İşlemler</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let user of filteredUsers">
                  <td class="px-4 py-3">
                    <div class="d-flex align-items-center">
                      <div class="avatar-circle" [style.background-color]="getAvatarColor(user.firstName)">
                        {{ getInitials(user.firstName, user.lastName) }}
                      </div>
                      <div class="ms-3">
                        {{ user.firstName }} {{ user.lastName }}
                      </div>
                    </div>
                  </td>
                  <td class="px-4 py-3">{{ user.email }}</td>
                  <td class="px-4 py-3">
                    <select 
                      class="form-select form-select-sm w-auto" 
                      [(ngModel)]="user.roleId" 
                      (change)="updateUserRole(user)"
                      [disabled]="user.roleId === 1"
                    >
                      <option [value]="2">Admin</option>
                      <option [value]="3">Kullanıcı</option>
                    </select>
                  </td>
                  <td class="px-4 py-3">
                    <div class="form-check form-switch">
                      <input 
                        class="form-check-input" 
                        type="checkbox" 
                        [(ngModel)]="user.isActive" 
                        (change)="updateUserStatus(user)"
                        [id]="'status-' + user.id"
                        [disabled]="user.roleId === 1"
                      >
                      <label class="form-check-label" [for]="'status-' + user.id">
                        {{ user.isActive ? 'Aktif' : 'Pasif' }}
                      </label>
                    </div>
                  </td>
                  <td class="px-4 py-3">
                    <span [class.text-muted]="!user.lastLoginAt">
                      {{ user.lastLoginAt ? (user.lastLoginAt | date:'dd.MM.yyyy HH:mm':'tr-TR') : 'Henüz Giriş Yapmadı' }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-end">
                    <button class="btn btn-outline-primary btn-sm" (click)="openCompanyModal(user)">
                      <i class="fas fa-link me-1"></i> Firma Eşle
                    </button>
                    <button 
                      class="btn btn-outline-danger btn-sm" 
                      (click)="deleteUser(user.id)"
                      [disabled]="user.roleId === 1"
                    >
                      <i class="fas fa-trash me-1"></i>
                      Sil
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal HTML'i -->
    <div class="modal fade show" tabindex="-1" [ngStyle]="{display: showCompanyModal ? 'block' : 'none', background: 'rgba(0,0,0,0.3)'}" *ngIf="showCompanyModal">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Firma Eşleştir</h5>
            <button type="button" class="btn-close" (click)="closeCompanyModal()"></button>
          </div>
          <div class="modal-body">
            <div *ngFor="let company of companies">
              <div class="form-check">
                <input class="form-check-input" type="checkbox"
                  [id]="'modal-company-' + company.id"
                  [checked]="tempSelectedCompanyIds.includes(company.id)"
                  (change)="onModalCompanyCheckboxChange(company.id, $event)" />
                <label class="form-check-label" [for]="'modal-company-' + company.id">{{ company.name }}</label>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" (click)="closeCompanyModal()">Kapat</button>
            <button type="button" class="btn btn-success" (click)="saveCompanyMatch()">Kaydet</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .form-switch .form-check-input {
      cursor: pointer;
    }
    .table td {
      vertical-align: middle;
    }
    .avatar-circle {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-weight: 500;
      font-size: 14px;
    }
    .search-box {
      width: 300px;
    }
    .table thead th {
      font-weight: 600;
      font-size: 0.875rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }
    .form-select {
      border-radius: 20px;
      padding: 0.25rem 2rem 0.25rem 1rem;
    }
    .btn-outline-danger {
      border-radius: 20px;
    }
    .form-check-input:checked {
      background-color: #198754;
      border-color: #198754;
    }
  `]
})
export class UsersComponent implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  searchTerm: string = '';
  companies: Company[] = [];

  // Modal için değişkenler
  selectedUserForCompanyMatch: User | null = null;
  tempSelectedCompanyIds: number[] = [];
  showCompanyModal = false;

  constructor(
    private userService: UserService, 
    private companyService: CompanyService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    // Auth service'den token hazır olduğunda kullanıcıları yükle
    this.authService.authReady$
      .pipe(
        filter(ready => ready),
        take(1)
      )
      .subscribe(() => {
        this.loadUsers();
        this.loadCompanies();
      });
  }

  loadUsers() {
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.users = users.filter(user => user.roleId !== 1);
        this.filterUsers();
      },
      error: (error: Error) => {
        console.error('Kullanıcılar yüklenirken hata:', error);
      }
    });
  }

  loadCompanies() {
    this.companyService.getCompanies().subscribe({
      next: (companies) => {
        this.companies = companies;
      },
      error: (error: Error) => {
        console.error('Firmalar yüklenirken hata:', error);
      }
    });
  }

  filterUsers() {
    if (!this.searchTerm) {
      this.filteredUsers = this.users;
      return;
    }

    const searchTermLower = this.searchTerm.toLowerCase();
    this.filteredUsers = this.users.filter(user => 
      user.firstName.toLowerCase().includes(searchTermLower) ||
      user.lastName.toLowerCase().includes(searchTermLower) ||
      user.email.toLowerCase().includes(searchTermLower)
    );
  }

  getInitials(firstName: string, lastName: string): string {
    return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
  }

  getAvatarColor(name: string): string {
    const colors = [
      '#ff7a00', '#1976d2', '#d81b60', '#43a047', '#fbc02d', '#8e24aa', '#00acc1', '#e64a19'
    ];
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  }

  getRoleBadgeClass(roleId: number): string {
    if (roleId === 2) return 'role-badge admin';
    if (roleId === 3) return 'role-badge user';
    return 'role-badge';
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'status-badge' : 'status-badge inactive';
  }

  updateUserRole(user: User) {
    this.userService.updateUserRole(user.id, user.roleId).subscribe({
      next: () => {
        console.log('Kullanıcı rolü güncellendi');
      },
      error: (error: Error) => {
        console.error('Rol güncellenirken hata:', error);
        this.loadUsers();
      }
    });
  }

  updateUserStatus(user: User) {
    this.userService.updateUserStatus(user.id, user.isActive).subscribe({
      next: () => {
        console.log('Kullanıcı durumu güncellendi');
      },
      error: (error: Error) => {
        console.error('Durum güncellenirken hata:', error);
        this.loadUsers();
      }
    });
  }

  deleteUser(userId: string) {
    if (confirm('Bu kullanıcıyı silmek istediğinize emin misiniz?')) {
      this.userService.deleteUser(userId).subscribe({
        next: () => {
          console.log('Kullanıcı silindi');
          this.loadUsers();
        },
        error: (error: Error) => {
          console.error('Kullanıcı silinirken hata:', error);
        }
      });
    }
  }

  openCompanyModal(user: User) {
    this.selectedUserForCompanyMatch = user;
    this.tempSelectedCompanyIds = user.companyIds ? [...user.companyIds] : [];
    this.showCompanyModal = true;
  }

  closeCompanyModal() {
    this.showCompanyModal = false;
    this.selectedUserForCompanyMatch = null;
    this.tempSelectedCompanyIds = [];
  }

  onModalCompanyCheckboxChange(companyId: number, event: any) {
    if (event.target.checked) {
      this.tempSelectedCompanyIds.push(companyId);
    } else {
      const index = this.tempSelectedCompanyIds.indexOf(companyId);
      if (index > -1) {
        this.tempSelectedCompanyIds.splice(index, 1);
      }
    }
  }

  saveCompanyMatch() {
    if (this.selectedUserForCompanyMatch) {
      this.userService.updateUserCompanies(this.selectedUserForCompanyMatch.id, this.tempSelectedCompanyIds).subscribe({
        next: () => {
          if (this.selectedUserForCompanyMatch) {
            this.selectedUserForCompanyMatch.companyIds = [...this.tempSelectedCompanyIds];
          }
          this.closeCompanyModal();
        },
        error: (error: Error) => {
          console.error('Firma eşleştirme hatası:', error);
        }
      });
    }
  }
} 