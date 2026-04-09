import { Component, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { CardModule } from 'primeng/card';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';
import { MessageService } from 'primeng/api';
import { TranslateService } from '@ngx-translate/core';

declare var google: any;

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, MessageModule, CardModule, TranslateModule],
  providers: [MessageService],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements AfterViewInit {
  displayName = '';
  email = '';
  password = '';
  phoneNumber = '';
  error = '';
  loading = false;

  constructor(
    private authService: AuthService, 
    private router: Router,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngAfterViewInit(): void {
    this.initGoogle();

    this.translate.onLangChange.subscribe(() => {
      this.renderGoogleButton();
    });
  }

  initGoogle() {
    if (typeof google === 'undefined' || !google.accounts) {
      setTimeout(() => this.initGoogle(), 100);
      return;
    }

    google.accounts.id.initialize({
      client_id: '331442573652-kqe2go0r9fvcsqgkikii5caukc5792u8.apps.googleusercontent.com',
      callback: this.handleGoogleCredentialResponse.bind(this),
      auto_select: false,
      cancel_on_tap_outside: true
    });

    this.renderGoogleButton();
  }

  renderGoogleButton() {
    const googleBtnElement = document.getElementById('google-btn-register');
    if (googleBtnElement && typeof google !== 'undefined' && google.accounts?.id) {
      googleBtnElement.innerHTML = '';
      
      const activeLang = this.translate.currentLang || this.translate.getDefaultLang() || localStorage.getItem('lang') || 'en';
      const localeCode = activeLang.toLowerCase().includes('ar') ? 'ar' : 'en';

      google.accounts.id.renderButton(googleBtnElement, {
        theme: 'outline',
        size: 'large',
        width: '100%',
        text: 'continue_with',
        locale: localeCode
      });
    }
  }

  handleGoogleCredentialResponse(response: any) {
    if (response && response.credential) {
      this.loading = true;
      this.authService.googleLogin(response.credential).subscribe({
        next: (user) => {
          this.loading = false;
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('TOAST.SUCCESS') || 'Success',
            detail: this.translate.instant('TOAST.REGISTER_SUCCESS') || 'Generated Account Successfully via Google',
            life: 3000
          });
          this.router.navigate([this.authService.isAdmin(user?.token ?? null) ? '/admin' : '/']);
        },
        error: (err) => {
          this.loading = false;
          this.error = 'Failed to continue via Google';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: this.error,
            life: 3000
          });
        }
      });
    }
  }

  register(): void {
    if (this.loading) return;
    this.error = '';
    this.loading = true;
    this.authService.register(this.displayName, this.email, this.password, this.phoneNumber).subscribe({
      next: () => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('TOAST.SUCCESS'),
          detail: this.translate.instant('TOAST.REGISTER_SUCCESS'),
          life: 3000
        });
        setTimeout(() => this.router.navigate(['/']), 1000);
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('TOAST.ERROR'),
          detail: err?.error?.message || this.translate.instant('TOAST.REGISTER_FAIL'),
          life: 4000
        });
      }
    });
  }
}
