import { Component } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-manage',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './manage.component.html',
  styleUrls: ['./manage.component.scss']
})
export class ManageComponent {
  constructor(private router: Router) {}

  navigateToAddCompany() {
    this.router.navigate(['/manage/add-company']);
  }

  navigateToViewCompanies() {
    this.router.navigate(['/manage/companies']);
  }
} 