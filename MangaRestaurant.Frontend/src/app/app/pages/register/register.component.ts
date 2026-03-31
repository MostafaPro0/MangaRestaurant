import { Component } from '@angular/core';
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

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, MessageModule, CardModule, TranslateModule],
  providers: [MessageService],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  displayName = '';
  email = '';
  password = '';
  phoneNumber = '';
  error = '';

  constructor(
    private authService: AuthService, 
    private router: Router,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  register(): void {
    this.error = '';
    this.authService.register(this.displayName, this.email, this.password, this.phoneNumber).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('TOAST.SUCCESS'),
          detail: this.translate.instant('TOAST.REGISTER_SUCCESS'),
          life: 3000
        });
        setTimeout(() => this.router.navigate(['/']), 1000);
      },
      error: (err) => {
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
