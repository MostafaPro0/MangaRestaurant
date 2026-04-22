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
  template: `
    <div class="container section-pad flex justify-content-center">
      <div class="mg-card p-6 w-full md:w-35rem">
        <div class="text-center mb-5">
            <h2 class="display-font text-3xl font-bold text-primary mb-2">{{ 'AUTH.RESET_TITLE' | translate }}</h2>
            <p class="text-dim">{{ 'AUTH.RESET_DESC' | translate }}</p>
        </div>

        <form (ngSubmit)="onSubmit()" #resetForm="ngForm" class="flex flex-column gap-4">
          <div class="flex flex-column gap-2 mb-3">
            <label class="font-bold opacity-70">{{ 'AUTH.NEW_PASSWORD' | translate }}</label>
            <p-password [(ngModel)]="data.newPassword" name="newPassword" 
                        [toggleMask]="true" required class="w-full" styleClass="w-full" inputStyleClass="w-full p-input-lg"></p-password>
          </div>

          <div class="flex flex-column gap-2">
            <label class="font-bold opacity-70">{{ 'AUTH.CONFIRM_PASSWORD' | translate }}</label>
            <p-password [(ngModel)]="data.confirmPassword" name="confirmPassword" 
                       [toggleMask]="true" required [feedback]="false" class="w-full" styleClass="w-full" inputStyleClass="w-full p-input-lg"></p-password>
          </div>

          <div class="bg-red-900/20 p-3 rounded-lg border-1 border-red-500/30 text-red-500 text-sm mt-2" 
               *ngIf="data.newPassword && data.confirmPassword && data.newPassword !== data.confirmPassword">
            <i class="pi pi-exclamation-circle mr-2"></i> {{ 'AUTH.PASSWORDS_DONT_MATCH' | translate }}
          </div>

          <button pButton type="submit" [label]="'AUTH.UPDATE_PASSWORD' | translate" 
                  class="p-button-primary p-button-lg w-full py-4 text-xl mt-4" 
                  [loading]="loading" [disabled]="!resetForm.valid || data.newPassword !== data.confirmPassword"></button>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .mg-card {
        background: var(--surface);
        border: 1px solid var(--glass-border);
        border-radius: var(--radius-lg);
    }
  `]
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
