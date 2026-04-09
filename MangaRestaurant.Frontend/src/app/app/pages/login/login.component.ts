import { Component, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageModule } from 'primeng/message';
import { CardModule } from 'primeng/card';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../services/auth.service';

declare var google: any;

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, MessageModule, CardModule, TranslateModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent implements AfterViewInit {
  email = '';
  password = '';
  error = '';

  constructor(
    private authService: AuthService, 
    private router: Router, 
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngAfterViewInit(): void {
    this.initGoogle();

    // Re-render Google button when language changes to apply 'ar' or 'en' locale
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
    const googleBtnElement = document.getElementById('google-btn');
    if (googleBtnElement && typeof google !== 'undefined' && google.accounts?.id) {
      googleBtnElement.innerHTML = ''; // prevent duplicates on re-render
      
      // Determine language safely using the exact local storage key
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
      this.authService.googleLogin(response.credential).subscribe({
        next: (user) => {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('TOAST.SUCCESS') || 'Success',
            detail: this.translate.instant('TOAST.LOGIN_SUCCESS') || 'Logged in successfully via Google',
            life: 3000
          });
          this.router.navigate([this.authService.isAdmin(user?.token ?? null) ? '/admin' : '/']);
        },
        error: (err) => {
          const errorMsg = err?.error?.message || 'Failed to login via Google';
          this.error = errorMsg;
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('TOAST.ERROR') || 'Error',
            detail: errorMsg,
            life: 3000
          });
        }
      });
    }
  }

  login(): void {
    this.error = '';
    this.authService.login(this.email, this.password).subscribe({
      next: (user) => {
        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('TOAST.SUCCESS') || 'Success',
          detail: this.translate.instant('TOAST.LOGIN_SUCCESS') || 'Logged in successfully',
          life: 3000
        });
        this.router.navigate([this.authService.isAdmin(user?.token ?? null) ? '/admin' : '/']);
      },
      error: (err) => {
        let errorMsg = err?.error?.message || 'Failed to login: ' + (err?.message || err);
        
        // Translate known backend error texts or 401 status to correct language
        if (err?.status === 401 || typeof errorMsg === 'string' && (errorMsg.includes('Unauthorized') || errorMsg.includes('Invalid email or password'))) {
          errorMsg = this.translate.instant('LOGIN.INVALID_CREDENTIALS');
        }

        this.error = errorMsg;
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('TOAST.ERROR') || 'Error',
          detail: errorMsg,
          life: 3000
        });
      }
    });
  }
}
