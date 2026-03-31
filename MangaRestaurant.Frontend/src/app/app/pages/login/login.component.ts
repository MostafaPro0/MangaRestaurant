import { Component } from '@angular/core';
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

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, MessageModule, CardModule, TranslateModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';

  constructor(
    private authService: AuthService, 
    private router: Router, 
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

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
      error: (err) => (this.error = 'Failed to login: ' + (err?.message || err))
    });
  }
}
