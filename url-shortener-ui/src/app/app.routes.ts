import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ShortenComponent } from './shorten/shorten.component';

export const routes: Routes = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'shorten', component: ShortenComponent },
  { 
    path: 'manage', 
    loadChildren: () => import('./manage/manage.routes').then(m => m.MANAGE_ROUTES) 
  },
  { path: '**', redirectTo: '' }
];
