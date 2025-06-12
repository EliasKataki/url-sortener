import { Component } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule, FormGroup } from '@angular/forms';
import { AuthService, ApiResponse } from '../services/auth.service';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  loading = false;
  error: string | null = null;
  success: string | null = null;
  form: FormGroup;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.form = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  submit() {
    this.error = null;
    this.success = null;
    if (this.form.invalid) return;
    this.loading = true;
    const value = {
      firstName: this.form.value.firstName || '',
      lastName: this.form.value.lastName || '',
      email: this.form.value.email || '',
      password: this.form.value.password || ''
    };
    this.auth.register(value).subscribe({
      next: (response: ApiResponse) => {
        console.log('Kayıt başarılı:', response);
        this.success = response.message || 'Kayıt başarılı! Giriş yapabilirsiniz.';
        this.loading = false;
        setTimeout(() => this.router.navigate(['/login']), 1500);
      },
      error: (err: HttpErrorResponse) => {
        console.error('Kayıt hatası:', err);
        this.loading = false;
        
        // HTTP 400 hata durumunda (BadRequest)
        if (err.status === 400) {
          // JSON yanıt içindeki mesajı almaya çalış
          if (err.error && err.error.message) {
            this.error = err.error.message;
          } else {
            this.error = 'Bu email adresi zaten kullanılıyor. Lütfen farklı bir email adresi deneyin.';
          }
          return;
        }
        
        // Ayrıştırma hatası durumunda
        if (err.error instanceof Error) {
          this.error = 'Sunucu yanıtı işlenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.';
          return;
        }
        
        // Diğer tüm durumlar için
        this.error = 'Kayıt işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.';
      }
    });
  }
} 