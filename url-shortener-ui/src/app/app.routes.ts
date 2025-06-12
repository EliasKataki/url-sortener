import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { authGuard } from './guards/auth.guard';
import { UsersComponent } from './manage/users/users.component';
import { inject } from '@angular/core';
import { AuthService } from './services/auth.service';
import { Router } from '@angular/router';

export const routes: Routes = [
  { 
    path: '', 
    resolve: {
      auth: () => {
        const authService = inject(AuthService);
        const router = inject(Router);
        
        if (authService.isAuthenticated()) {
          router.navigate(['/home']);
          return true;
        }
        return true;
      }
    },
    redirectTo: 'login',
    pathMatch: 'full' 
  },
  { 
    path: 'home', 
    component: HomeComponent, 
    canActivate: [authGuard] 
  },
  { 
    path: 'login',
    resolve: {
      auth: () => {
        const authService = inject(AuthService);
        const router = inject(Router);
        
        if (authService.isAuthenticated()) {
          router.navigate(['/home']);
          return false;
        }
        return true;
      }
    }, 
    component: LoginComponent
  },
  { 
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'manage',
    loadChildren: () => import('./manage/manage.routes').then(m => m.MANAGE_ROUTES),
    canActivate: [authGuard]
  },
  { 
    path: '**', 
    redirectTo: 'login' 
  }
];
