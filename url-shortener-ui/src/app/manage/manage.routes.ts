import { Routes } from '@angular/router';
import { AddCompanyComponent } from './add-company/add-company.component';
import { CompaniesComponent } from './companies/companies.component';
import { ManageComponent } from './manage.component';
import { CompanyDetailComponent } from './company-detail/company-detail.component';

export const MANAGE_ROUTES: Routes = [
  {
    path: '', // /manage ana yolu ManageComponent'i değil, alt rotaları göstermeli
    component: ManageComponent, // Ana çerçeve için ManageComponent kullanılabilir
    children: [
      { path: 'add-company', component: AddCompanyComponent }, // /manage/add-company
      { path: 'companies', component: CompaniesComponent }, // /manage/companies
      { path: 'company-details/:id', component: CompanyDetailComponent },
      { path: '', redirectTo: 'companies', pathMatch: 'full' } // /manage boşsa companies'e yönlendir
    ]
  }
]; 