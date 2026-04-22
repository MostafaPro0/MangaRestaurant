import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, ButtonModule, TranslateModule, RouterModule],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  email: string = '';
  loading: boolean = false;

  constructor(private auth: AuthService, private messageService: MessageService, private translate: TranslateService) {}

  onSubmit() {
    this.loading = true;
    this.auth.forgotPassword(this.email).subscribe({
      next: (res) => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('TOAST.SUCCESS'),
          detail: res.message || this.translate.instant('AUTH.FORGOT_SUCCESS')
        });
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('TOAST.ERROR'),
          detail: this.translate.instant('AUTH.FORGOT_ERROR')
        });
      }
    });
  }
}
