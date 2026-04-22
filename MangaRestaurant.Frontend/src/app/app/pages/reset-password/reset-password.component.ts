import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { InputTextModule } from 'primeng/inputtext';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PasswordModule } from 'primeng/password';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, ButtonModule, TranslateModule, RouterModule, PasswordModule],
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent implements OnInit {
  loading: boolean = false;
  data = { email: '', token: '', newPassword: '', confirmPassword: '' };

  constructor(
    private route: ActivatedRoute, 
    public auth: AuthService, 
    private messageService: MessageService, 
    private router: Router,
    private translate: TranslateService
  ) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.data.token = params['token'] || '';
      this.data.email = params['email'] || '';
    });

    if (!this.data.token || !this.data.email) {
      this.messageService.add({ severity: 'error', summary: this.translate.instant('AUTH.INVALID_LINK'), detail: this.translate.instant('AUTH.INVALID_LINK_DESC') });
      this.router.navigate(['/login']);
    }
  }

  onSubmit() {
    this.loading = true;
    this.auth.resetPassword(this.data).subscribe({
      next: (res) => {
        this.loading = false;
        this.messageService.add({ severity: 'success', summary: this.translate.instant('TOAST.SUCCESS'), detail: res.message || this.translate.instant('AUTH.RESET_SUCCESS') });
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.loading = false;
        console.error('Reset error', err);
        const errorMsg = err.error?.message || err.error?.errors?.[0] || this.translate.instant('TOAST.UPDATE_FAILED');
        this.messageService.add({ severity: 'error', summary: this.translate.instant('TOAST.ERROR'), detail: errorMsg });
      }
    });
  }
}
