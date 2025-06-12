import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-manage',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './manage.component.html',
  styles: [`
    .btn-outline-primary {
      color: #ff6b00;
      border-color: #ff6b00;
      background-color: transparent;
      transition: all 0.2s ease-in-out;
    }

    .btn-outline-primary:hover {
      color: white;
      background-color: #ff6b00;
      border-color: #ff6b00;
    }

    .btn-outline-primary.active {
      color: white !important;
      background-color: #ff6b00 !important;
      border-color: #ff6b00 !important;
      box-shadow: 0 0 0 0.25rem rgba(255, 107, 0, 0.25);
    }

    .btn-outline-primary:focus {
      box-shadow: 0 0 0 0.25rem rgba(255, 107, 0, 0.25);
    }

    .btn-outline-primary:active {
      color: white !important;
      background-color: #ff6b00 !important;
      border-color: #ff6b00 !important;
    }

    .btn-outline-primary:disabled {
      color: #ff6b00;
      background-color: transparent;
    }
  `]
})
export class ManageComponent {
  constructor(
    private router: Router,
    private authService: AuthService
  ) {}

  get isSuperAdmin(): boolean {
    return this.authService.isSuperAdmin();
  }

  navigateToAddCompany() {
    if (this.isSuperAdmin) {
      this.router.navigate(['/manage/add-company']);
    }
  }

  navigateToViewCompanies() {
    this.router.navigate(['/manage/companies']);
  }
} 