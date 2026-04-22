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
  template: `
    <div class="container section-pad flex justify-content-center">
      <div class="mg-card p-6 w-full md:w-30rem">
        <div class="text-center mb-5">
            <h2 class="display-font text-3xl font-bold text-primary mb-2">{{ 'AUTH.FORGOT_TITLE' | translate }}</h2>
            <p class="text-dim">{{ 'AUTH.FORGOT_DESC' | translate }}</p>
        </div>

        <form (ngSubmit)="onSubmit()" #forgotForm="ngForm" class="flex flex-column gap-4">
          <div class="flex flex-column gap-2">
            <label class="font-bold opacity-70">{{ 'AUTH.EMAIL' | translate }}</label>
            <input pInputText type="email" [(ngModel)]="email" name="email" 
                   required email class="p-input-lg" placeholder="email@example.com" />
          </div>

          <button pButton type="submit" [label]="'AUTH.SEND_LINK' | translate" 
                  class="p-button-primary p-button-lg w-full py-4 text-xl mt-2" 
                  [loading]="loading" [disabled]="!forgotForm.valid"></button>
          
          <div class="text-center mt-4">
            <a routerLink="/login" class="text-primary no-underline font-bold hover:underline">
                <i class="pi pi-arrow-left mr-2"></i> {{ 'AUTH.BACK_TO_LOGIN' | translate }}
            </a>
          </div>
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
