import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterModule, Router, NavigationEnd } from '@angular/router';
import { AuthService } from './services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  isHome = true;
  isLoggedIn = false;
  userName = '';
  userRole = '';
  
  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    this.router.events.subscribe(() => {
      this.isHome = this.router.url === '/home';
    });
  }

  ngOnInit() {
    // Sayfa yüklendiğinde ve her navigasyonda kullanıcı durumunu kontrol et
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe(() => {
      this.checkLoginStatus();
    });
    
    this.checkLoginStatus();
  }

  get isSuperAdmin(): boolean {
    return this.authService.isSuperAdmin();
  }

  checkLoginStatus() {
    this.isLoggedIn = this.authService.isAuthenticated();
    
    if (this.isLoggedIn) {
      // LocalStorage'den kullanıcı bilgilerini alabiliriz
      const userInfo = localStorage.getItem('userInfo');
      if (userInfo) {
        try {
          const user = JSON.parse(userInfo);
          this.userName = user.firstName || 'Kullanıcı';
          this.userRole = this.getRoleName(user.roleId);
        } catch (e) {
          this.userName = 'Kullanıcı';
          this.userRole = '';
        }
      }
    }
  }

  getRoleName(roleId: number): string {
    switch (roleId) {
      case 1:
        return 'SUPERADMIN';
      case 2:
        return 'ADMIN';
      case 3:
        return 'USER';
      default:
        return '';
    }
  }

  logout() {
    this.authService.logout();
    this.isLoggedIn = false;
    this.userName = '';
    this.userRole = '';
    this.router.navigate(['/login']);
  }
}
