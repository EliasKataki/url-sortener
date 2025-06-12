import { Routes } from '@angular/router';
import { AddCompanyComponent } from './add-company/add-company.component';
import { CompaniesComponent } from './companies/companies.component';
import { ManageComponent } from './manage.component';
import { CompanyDetailComponent } from './company-detail/company-detail.component';
import { UsersComponent } from './users/users.component';

export const MANAGE_ROUTES: Routes = [
  {
    path: '',
    component: ManageComponent,
    children: [
      { path: 'add-company', component: AddCompanyComponent },
      { path: 'companies', component: CompaniesComponent },
      { path: 'company/:id', component: CompanyDetailComponent },
      { path: 'users', component: UsersComponent },
      { path: '', redirectTo: 'companies', pathMatch: 'full' }
    ]
  }
]; 