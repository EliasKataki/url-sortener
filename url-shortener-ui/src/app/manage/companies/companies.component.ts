import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { CompanyService, Company } from '../../services/company.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-companies',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './companies.component.html',
  styleUrls: ['./companies.component.scss']
})
export class CompaniesComponent implements OnInit {
  companies: Company[] = [];
  selectedCompany: Company | null = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(
    private companyService: CompanyService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadCompanies();
  }

  get isSuperAdmin(): boolean {
    return this.authService.isSuperAdmin();
  }

  loadCompanies(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.companyService.getCompanies().subscribe({
      next: (data: Company[]) => {
        this.companies = data;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Firmalar yüklenirken hata:', error);
        this.errorMessage = 'Firmalar yüklenirken bir hata oluştu.';
        this.isLoading = false;
      }
    });
  }

  selectCompany(company: Company) {
    this.selectedCompany = company;
  }

  getLocalTime(utcString: string): string {
    if (!utcString) return '';
    const date = new Date(utcString);
    date.setHours(date.getHours() + 3); // Türkiye için 3 saat ekle
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    const hour = String(date.getHours()).padStart(2, '0');
    const minute = String(date.getMinutes()).padStart(2, '0');
    return `${day}.${month}.${year} ${hour}:${minute}`;
  }
} 