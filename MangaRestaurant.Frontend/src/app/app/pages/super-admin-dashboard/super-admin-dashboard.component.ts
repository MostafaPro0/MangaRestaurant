import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SuperAdminService, Tenant, CreateTenantDto } from '../../services/super-admin.service';
import { MessageService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { TabViewModule } from 'primeng/tabview';
import { DropdownModule } from 'primeng/dropdown';
import { TranslateService, TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-super-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    PasswordModule,
    ToastModule,
    TagModule,
    TabViewModule,
    DropdownModule,
    TranslateModule
  ],
  providers: [MessageService],
  templateUrl: './super-admin-dashboard.component.html',
  styleUrls: ['./super-admin-dashboard.component.css']
})
export class SuperAdminDashboardComponent implements OnInit {
  tenants: Tenant[] = [];
  loading: boolean = true;
  currentLang: string = 'en';

  displayCreateDialog: boolean = false;
  displayEditDialog: boolean = false;
  creating: boolean = false;
  updating: boolean = false;
  
  createForm: FormGroup;
  editForm: FormGroup;
  selectedTenant: Tenant | null = null;

  plans: any[] = [];

  constructor(
    private superAdminService: SuperAdminService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private translate: TranslateService
  ) {
    this.initPlans();
    this.currentLang = this.translate.currentLang || 'en';
    this.translate.onLangChange.subscribe(event => {
      this.currentLang = event.lang;
      this.initPlans();
    });

    this.createForm = this.fb.group({
      name: ['', Validators.required],
      nameAr: ['', Validators.required],
      slug: ['', [Validators.required, Validators.pattern('^[a-z0-9-]+$')]],
      adminEmail: ['', [Validators.required, Validators.email]],
      adminName: ['', Validators.required],
      adminPassword: ['', [Validators.required, Validators.minLength(6)]],
      planId: [1]
    });

    this.editForm = this.fb.group({
      name: ['', Validators.required],
      nameAr: ['', Validators.required],
      planId: [Validators.required],
      isActive: [true]
    });
  }

  initPlans() {
    this.plans = [
      { label: this.translate.instant('SUPER_ADMIN.PLAN_FREE'), value: 1 },
      { label: this.translate.instant('SUPER_ADMIN.PLAN_PRO'), value: 2 },
      { label: this.translate.instant('SUPER_ADMIN.PLAN_ENTERPRISE'), value: 3 }
    ];
  }

  ngOnInit() {
    this.loadTenants();
  }

  loadTenants() {
    this.loading = true;
    this.superAdminService.getAllTenants().subscribe({
      next: (data) => {
        this.tenants = data;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ 
          severity: 'error', 
          summary: this.translate.instant('TOAST.ERROR'), 
          detail: this.translate.instant('SUPER_ADMIN.ERROR_LOAD') 
        });
        this.loading = false;
      }
    });
  }

  showCreateDialog() {
    this.createForm.reset({ planId: 1 });
    this.displayCreateDialog = true;
  }

  hideCreateDialog() {
    this.displayCreateDialog = false;
  }

  onSubmit() {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    this.creating = true;
    const dto: CreateTenantDto = this.createForm.value;

    this.superAdminService.createTenant(dto).subscribe({
      next: (newTenant) => {
        this.messageService.add({ 
          severity: 'success', 
          summary: this.translate.instant('TOAST.SUCCESS'), 
          detail: this.translate.instant('SUPER_ADMIN.SUCCESS_CREATE') 
        });
        this.tenants.push(newTenant);
        this.hideCreateDialog();
        this.creating = false;
      },
      error: (err) => {
        this.messageService.add({ 
          severity: 'error', 
          summary: this.translate.instant('TOAST.ERROR'), 
          detail: err.error?.message || this.translate.instant('SUPER_ADMIN.ERROR_CREATE') 
        });
        this.creating = false;
      }
    });
  }

  onEdit(tenant: Tenant) {
    this.selectedTenant = tenant;
    this.editForm.patchValue({
      name: tenant.name,
      nameAr: tenant.nameAr,
      planId: tenant.planId,
      isActive: tenant.isActive
    });
    this.displayEditDialog = true;
  }

  onUpdateSubmit() {
    if (this.editForm.invalid || !this.selectedTenant) return;

    this.updating = true;
    this.superAdminService.updateTenant(this.selectedTenant.slug, this.editForm.value).subscribe({
      next: (updatedTenant) => {
        this.messageService.add({ 
          severity: 'success', 
          summary: this.translate.instant('TOAST.SUCCESS'), 
          detail: this.translate.instant('SUPER_ADMIN.SUCCESS_UPDATE') 
        });
        this.loadTenants();
        this.displayEditDialog = false;
        this.updating = false;
      },
      error: () => {
        this.messageService.add({ 
          severity: 'error', 
          summary: this.translate.instant('TOAST.ERROR'), 
          detail: this.translate.instant('SUPER_ADMIN.ERROR_UPDATE') 
        });
        this.updating = false;
      }
    });
  }

  deleteTenant(slug: string) {
    if (confirm(`${this.translate.instant('SUPER_ADMIN.DEACTIVATE_CONFIRM')} ${slug}?`)) {
      this.superAdminService.deleteTenant(slug).subscribe({
        next: () => {
          this.messageService.add({ 
            severity: 'success', 
            summary: this.translate.instant('TOAST.SUCCESS'), 
            detail: this.translate.instant('SUPER_ADMIN.SUCCESS_DEACTIVATE') 
          });
          this.loadTenants();
        },
        error: () => this.messageService.add({ 
          severity: 'error', 
          summary: this.translate.instant('TOAST.ERROR'), 
          detail: this.translate.instant('SUPER_ADMIN.ERROR_DEACTIVATE') 
        })
      });
    }
  }
}
