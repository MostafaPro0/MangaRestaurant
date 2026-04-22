import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormsModule, NgForm } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { AvatarModule } from 'primeng/avatar';
import { environment } from '../../../../environments/environment';
import { DialogModule } from 'primeng/dialog';
import { DividerModule } from 'primeng/divider';
import { SkeletonModule } from 'primeng/skeleton';
import { PasswordModule } from 'primeng/password';
import { TabViewModule } from 'primeng/tabview';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, InputTextModule, TranslateModule, AvatarModule, DialogModule, DividerModule, SkeletonModule, PasswordModule, TabViewModule, TooltipModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {
  user: User | null = null;
  displayName = '';
  phoneNumber = '';
  phoneNumber2 = '';
  profilePictureUrl: string | null = null;
  
  addresses: any[] = [];
  addressDialogVisible = false;
  editingAddress: any = this.getEmptyAddress();
  
  changePasswordDialogVisible = false;
  passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };

  loadingProfile = false;
  loadingAddresses = false;
  loadingPicture = false;
  savingAddress = false;
  activeTabIndex = 0;

  // GPS Location for address dialog
  detectingLocation = false;
  locationDetected = false;
  locationError = '';

  @ViewChild('fileInput') fileInput!: ElementRef;
  @ViewChild('addressForm') addressForm!: NgForm;

  constructor(
    private authService: AuthService,
    private messageService: MessageService,
    private translate: TranslateService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.refreshUser();
    this.loadAddresses();

    this.route.queryParams.subscribe(params => {
      if (params['tab'] === 'password') {
        this.activeTabIndex = 1;
      } else if (params['tab'] === 'address') {
        this.activeTabIndex = 0;
      }
    });
  }

  refreshUser(): void {
    this.authService.getCurrentUser().subscribe({
      next: (user) => {
        this.user = user;
        if (this.user) {
          this.displayName = this.user.displayName;
          this.phoneNumber = this.user.phoneNumber || '';
          this.phoneNumber2 = this.user.phoneNumber2 || '';
          this.profilePictureUrl = this.user.profilePictureUrl || null;
        }
      }
    });
  }

  loadAddresses(): void {
    this.loadingAddresses = true;
    this.authService.getUserAddresses().subscribe({
      next: (data) => {
        this.addresses = data || [];
        this.loadingAddresses = false;
      },
      error: () => this.loadingAddresses = false
    });
  }

  getEmptyAddress() {
    return {
      firstName: '',
      lastName: '',
      street: '',
      city: '',
      state: '',
      zipCode: '',
      country: ''
    };
  }

  openNewAddress(): void {
    this.editingAddress = this.getEmptyAddress();
    this.locationDetected = false;
    this.locationError = '';
    this.addressDialogVisible = true;
  }

  editAddress(address: any): void {
    this.editingAddress = { ...address };
    this.locationDetected = !!(address.locationUrl);
    this.locationError = '';
    this.addressDialogVisible = true;
  }

  detectLocation(): void {
    if (!navigator.geolocation) {
      this.locationError = this.translate.instant('LOCATION.NOT_SUPPORTED');
      return;
    }
    this.detectingLocation = true;
    this.locationError = '';
    this.locationDetected = false;
    navigator.geolocation.getCurrentPosition(
      (position) => {
        const lat = position.coords.latitude;
        const lng = position.coords.longitude;
        this.editingAddress.locationUrl = `https://www.google.com/maps?q=${lat},${lng}`;
        this.editingAddress.latitude = lat;
        this.editingAddress.longitude = lng;
        this.detectingLocation = false;
        this.locationDetected = true;
      },
      (error) => {
        this.detectingLocation = false;
        switch (error.code) {
          case error.PERMISSION_DENIED:
            this.locationError = this.translate.instant('LOCATION.PERMISSION_DENIED');
            break;
          case error.POSITION_UNAVAILABLE:
            this.locationError = this.translate.instant('LOCATION.UNAVAILABLE');
            break;
          default:
            this.locationError = this.translate.instant('LOCATION.ERROR');
        }
      },
      { enableHighAccuracy: true, timeout: 10000 }
    );
  }

  clearLocation(): void {
    this.editingAddress.locationUrl = null;
    this.editingAddress.latitude = null;
    this.editingAddress.longitude = null;
    this.locationDetected = false;
    this.locationError = '';
  }

  saveAddress(): void {
    if (this.addressForm.invalid) {
        this.addressForm.control.markAllAsTouched();
        
        // Find missing fields
        const missingFields = [];
        const controls = this.addressForm.controls;
        if (controls['firstName']?.invalid) missingFields.push(this.translate.instant('PROFILE.FIRST_NAME'));
        if (controls['lastName']?.invalid) missingFields.push(this.translate.instant('PROFILE.LAST_NAME'));
        if (controls['street']?.invalid) missingFields.push(this.translate.instant('PROFILE.STREET'));
        if (controls['city']?.invalid) missingFields.push(this.translate.instant('PROFILE.CITY'));
        if (controls['state']?.invalid) missingFields.push(this.translate.instant('PROFILE.STATE'));
        if (controls['zipCode']?.invalid) missingFields.push(this.translate.instant('PROFILE.ZIP'));
        if (controls['country']?.invalid) missingFields.push(this.translate.instant('PROFILE.COUNTRY'));

        this.messageService.add({ 
            severity: 'warn', 
            summary: this.translate.instant('TOAST.WARN') || 'Warning', 
            detail: (this.translate.instant('COMMON.REQUIRED') || 'Required') + ': ' + missingFields.join(', ')
        });
        return;
    }
    
    this.savingAddress = true;
    const obs = this.editingAddress.id 
      ? this.authService.updateAddress(this.editingAddress)
      : this.authService.addAddress(this.editingAddress);

    obs.subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: this.translate.instant('TOAST.SUCCESS'), detail: this.translate.instant('PROFILE.ADDRESS_SAVED') || 'Address saved successfully' });
        this.loadAddresses();
        this.addressDialogVisible = false;
        this.savingAddress = false;
      },
      error: (err) => {
        console.error('Save address error:', err);
        let detail = this.translate.instant('PROFILE.ADDRESS_SAVE_FAILED') || 'Failed to save address';
        
        // Handle validation errors from backend
        if (err.errors && Array.isArray(err.errors) && err.errors.length > 0) {
            detail = err.errors.join(' | ');
            
            // If Arabic, try to translate common field names in the error messages
            if (this.translate.currentLang === 'ar') {
                detail = this.translateValidationError(err.errors);
            }
        }
        
        this.messageService.add({ 
            severity: 'error', 
            summary: this.translate.instant('TOAST.ERROR'), 
            detail: detail,
            life: 6000
        });
        this.savingAddress = false;
      }
    });
  }

  private translateValidationError(errors: string[]): string {
    return errors.map(err => {
      let translated = err;
      // Pattern: The {FieldName} field is required.
      if (err.includes('field is required')) {
        const field = err.split(' ')[1];
        const fieldMap: any = {
          'FirstName': 'الاسم الأول',
          'LastName': 'الاسم الأخير',
          'Street': 'الشارع',
          'City': 'المدينة',
          'State': 'المحافظة',
          'ZipCode': 'الرمز البريدي',
          'Country': 'البلد',
          'PhoneNumber': 'رقم الهاتف'
        };
        if (fieldMap[field]) {
          translated = `حقل ${fieldMap[field]} مطلوب.`;
        }
      }
      return translated;
    }).join(' \n ');
  }

  deleteAddress(id: number): void {
    if (confirm(this.translate.instant('PROFILE.DELETE_ADDRESS_CONFIRM') || 'Are you sure?')) {
      this.authService.deleteAddress(id).subscribe({
        next: () => {
          this.messageService.add({ 
            severity: 'success', 
            summary: this.translate.instant('TOAST.SUCCESS'), 
            detail: this.translate.instant('PROFILE.ADDRESS_DELETED') || 'Address deleted' 
          });
          this.loadAddresses();
        },
        error: () => {
          this.messageService.add({ 
            severity: 'error', 
            summary: this.translate.instant('TOAST.ERROR'), 
            detail: this.translate.instant('PROFILE.DELETE_FAILED') || 'Failed' 
          });
        }
      });
    }
  }

  openChangePassword(): void {
    this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
    this.changePasswordDialogVisible = true;
  }

  changePassword(): void {
    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: this.translate.instant('AUTH.PASSWORDS_DONT_MATCH') });
      return;
    }

    const obs = (this.user?.hasPassword ?? true) 
      ? this.authService.changePassword(this.passwordData)
      : this.authService.addPassword({ newPassword: this.passwordData.newPassword, confirmPassword: this.passwordData.confirmPassword });

    obs.subscribe({
      next: (res: any) => {
        this.messageService.add({ severity: 'success', summary: this.translate.instant('TOAST.SUCCESS'), detail: res.message || 'Updated' });
        this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
        if (!this.user?.hasPassword) {
           this.user = this.authService.currentUser;
        }
      },
      error: (err) => {
        const detail = err.error?.message || err.error?.errors?.[0] || 'Failed to update password';
        this.messageService.add({ severity: 'error', summary: this.translate.instant('TOAST.ERROR'), detail });
      }
    });
  }

  saveProfile(): void {
    if (this.phoneNumber && this.phoneNumber2 && this.phoneNumber === this.phoneNumber2) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: this.translate.instant('PROFILE.PHONE_MATCH_ERROR') || 'Phone numbers cannot be identical' });
      return;
    }

    this.loadingProfile = true;
    this.authService.updateProfile({ 
       displayName: this.displayName, 
       phoneNumber: this.phoneNumber, 
       phoneNumber2: this.phoneNumber2 
    }).subscribe({
      next: (updatedUser) => {
        this.messageService.add({ 
          severity: 'success', 
          summary: this.translate.instant('TOAST.SUCCESS'), 
          detail: this.translate.instant('PROFILE.UPDATED') || 'Profile updated successfully' 
        });
        this.loadingProfile = false;
      },
      error: () => {
        this.messageService.add({ 
          severity: 'error', 
          summary: this.translate.instant('TOAST.ERROR'), 
          detail: this.translate.instant('PROFILE.UPDATE_FAILED') || 'Failed to update profile' 
        });
        this.loadingProfile = false;
      }
    });
  }

  triggerFileInput(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: any): void {
    const file: File = event.target.files[0];
    if (file) {
      this.loadingPicture = true;
      this.authService.uploadProfileImage(file).subscribe({
        next: (res) => {
          this.authService.updateProfile({ 
            displayName: this.displayName, 
            phoneNumber: this.phoneNumber, 
            phoneNumber2: this.phoneNumber2,
            profilePictureUrl: res.url 
          }).subscribe({
            next: (updatedUser) => {
              this.user = updatedUser;
              this.profilePictureUrl = res.url;
              this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Profile picture updated' });
              this.loadingPicture = false;
            }
          });
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to upload image' });
          this.loadingPicture = false;
        }
      });
    }
  }

  getInitials(): string {
    if (!this.displayName) return 'U';
    return this.displayName.charAt(0).toUpperCase();
  }
}
