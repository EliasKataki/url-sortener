import { Component } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loading = false;
  error: string | null = null;
  form: FormGroup;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  submit() {
    this.error = null;
    if (this.form.invalid) return;
    this.loading = true;
    const value = {
      email: this.form.value.email || '',
      password: this.form.value.password || ''
    };
    this.auth.login(value).subscribe({
      next: (response) => {
        console.log('Giriş başarılı:', response);
        
        // Token ve kullanıcı bilgilerini kaydet
        const userInfo = {
          firstName: response.user.firstName,
          lastName: response.user.lastName,
          email: response.user.email,
          roleId: response.user.roleId,
          isActive: response.user.isActive
        };
        this.auth.setAuthData(response.token, userInfo);
        
        this.loading = false;
        this.router.navigate(['/home']);
      },
      error: (err: HttpErrorResponse) => {
        console.error('Giriş hatası:', err);
        
        if (err.error === 'Email veya şifre hatalı.') {
          this.error = 'Email veya şifre hatalı. Lütfen bilgilerinizi kontrol edin.';
        } 
        else if (err.error && typeof err.error === 'string') {
          this.error = err.error;
        } else if (err.status === 401) {
          this.error = 'Email veya şifre hatalı. Lütfen bilgilerinizi kontrol edin.';
        } else if (err.message) {
          this.error = `Giriş başarısız: ${err.message}`;
        } else {
          this.error = 'Giriş başarısız. Lütfen daha sonra tekrar deneyin.';
        }
        
        this.loading = false;
      }
    });
  }
} 