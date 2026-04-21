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
  creating: boolean = false;
  createForm: FormGroup;

  constructor(
    private superAdminService: SuperAdminService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private translate: TranslateService
  ) {
    this.currentLang = this.translate.currentLang || 'en';
    this.translate.onLangChange.subscribe(event => {
      this.currentLang = event.lang;
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
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to load tenants' });
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
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Tenant Database Provisioned Successfully' });
        this.tenants.push(newTenant);
        this.hideCreateDialog();
        this.creating = false;
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: err.error?.message || 'Failed to provision tenant' });
        this.creating = false;
      }
    });
  }

  deleteTenant(slug: string) {
    if (confirm(`Are you sure you want to deactivate tenant: ${slug}?`)) {
      this.superAdminService.deleteTenant(slug).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Deactivated', detail: 'Tenant deactivated successfully' });
          this.loadTenants();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to deactivate tenant' })
      });
    }
  }
}
