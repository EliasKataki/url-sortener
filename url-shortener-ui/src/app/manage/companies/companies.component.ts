import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Company, CompanyService } from '../../services/company.service';
import { RouterModule } from '@angular/router';

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

  constructor(private companyService: CompanyService) {}

  ngOnInit(): void {
    this.loadCompanies();
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
} 