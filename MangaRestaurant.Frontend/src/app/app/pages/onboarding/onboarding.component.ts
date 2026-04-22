import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { CardModule } from 'primeng/card';
import { DropdownModule } from 'primeng/dropdown';
import { SaasInfoService } from '../../services/saas-info.service';
import { SaasPlan } from '../../models/saas.models';

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [
    CommonModule, 
    FormsModule, 
    RouterModule, 
    TranslateModule,
    InputTextModule, 
    PasswordModule, 
    ButtonModule, 
    ToastModule,
    CardModule,
    DropdownModule
  ],
  providers: [MessageService],
  templateUrl: './onboarding.component.html',
  styleUrl: './onboarding.component.css'
})
export class OnboardingComponent implements OnInit {
  restaurantName = '';
  restaurantNameAr = '';
  slug = '';
  adminName = '';
  adminEmail = '';
  adminPassword = '';
  selectedPlanId = 1;
  plans: SaasPlan[] = [];
  loading = false;
  loadingPlans = true;

  constructor(
    private saasService: SaasInfoService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    // Check if a plan was pre-selected from landing page
    this.route.queryParams.subscribe(params => {
      if (params['planId']) {
        this.selectedPlanId = +params['planId'];
      }
    });

    this.loadPlans();
  }

  loadPlans() {
    this.saasService.getAvailablePlans().subscribe({
      next: (data) => {
        this.plans = data;
        this.loadingPlans = false;
      },
      error: () => {
        this.loadingPlans = false;
      }
    });
  }

  get currentLang() {
    return this.translate.currentLang || localStorage.getItem('lang') || 'en';
  }

  generateSlug() {
    if (!this.slug && this.restaurantName) {
      this.slug = this.restaurantName
        .toLowerCase()
        .replace(/[^a-z0-9]/g, '-')
        .replace(/-+/g, '-')
        .replace(/^-|-$/g, '');
    }
  }

  onSubmit() {
    if (this.loading) return;
    
    // Basic validation
    if (!this.restaurantName || !this.restaurantNameAr || !this.slug || !this.adminEmail || !this.adminPassword) {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('TOAST.WARN'),
        detail: this.translate.instant('ADMIN.REQUIRED_FIELDS_ERROR')
      });
      return;
    }

    this.loading = true;

    const dto = {
      name: this.restaurantName,
      nameAr: this.restaurantNameAr,
      slug: this.slug,
      adminName: this.adminName,
      adminEmail: this.adminEmail,
      adminPassword: this.adminPassword,
      planId: this.selectedPlanId
    };

    this.saasService.registerRestaurant(dto).subscribe({
      next: (res: any) => {
        this.loading = false;
        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('TOAST.SUCCESS'),
          detail: res.message,
          life: 5000
        });

        // Successful onboarding! Redirect after toast
        setTimeout(() => {
          const protocol = window.location.protocol;
          const host = window.location.host;
          let redirectUrl = '';
          
          if (host.includes('localhost')) {
            const port = host.split(':')[1] || '4200';
            redirectUrl = `${protocol}//${res.subdomain}.localhost:${port}`;
          } else {
            redirectUrl = `${protocol}//${res.subdomain}.${host.replace('www.', '')}`;
          }

          this.router.navigate(['/onboarding-success'], { queryParams: { url: redirectUrl, slug: res.subdomain } });
        }, 2000);
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('TOAST.ERROR'),
          detail: err.error?.message || this.translate.instant('ONBOARDING.PROVISIONING_FAILED')
        });
      }
    });
  }
}
