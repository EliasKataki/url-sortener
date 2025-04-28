import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Company, CompanyService } from '../../services/company.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-company',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './add-company.component.html',
  styleUrls: ['./add-company.component.scss']
})
export class AddCompanyComponent {
  companyName: string = '';
  tokenCount: number = 1;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(private companyService: CompanyService, private router: Router) {}

  saveCompany() {
    this.errorMessage = '';
    this.successMessage = '';
    if (!this.companyName || this.tokenCount < 1) {
      this.errorMessage = 'Lütfen geçerli firma adı ve token sayısı girin.';
      return;
    }

    const companyData = { companyName: this.companyName, tokenCount: this.tokenCount };

    this.companyService.addCompany(companyData).subscribe({
      next: (response: Company) => {
        this.successMessage = `Firma '${response.name}' başarıyla eklendi.`;
        this.companyName = '';
        this.tokenCount = 1;
      },
      error: (error: any) => {
        console.error('Firma eklenirken hata:', error);
        this.errorMessage = 'Firma eklenirken bir hata oluştu. API yanıtını kontrol edin.';
        if (error?.error?.message) {
          this.errorMessage += ` Detay: ${error.error.message}`;
        }
      }
    });
  }
} 