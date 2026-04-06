import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/user.model';
import { AvatarModule } from 'primeng/avatar';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, InputTextModule, TranslateModule, AvatarModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.css'
})
export class UserProfileComponent implements OnInit {
  user: User | null = null;
  displayName = '';
  phoneNumber = '';
  phoneNumber2 = '';
  profilePictureUrl: string | null = null;
  
  address = {
    firstName: '',
    lastName: '',
    street: '',
    city: '',
    state: '',
    zipcode: '',
    country: ''
  };

  loadingProfile = false;
  loadingAddress = false;
  loadingPicture = false;

  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(
    private authService: AuthService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.user = this.authService.currentUser;
    if (this.user) {
      this.displayName = this.user.displayName;
      this.phoneNumber = this.user.phoneNumber || '';
      this.phoneNumber2 = this.user.phoneNumber2 || '';
      this.profilePictureUrl = this.user.profilePictureUrl ? environment.apiUrl.replace('/api/', '') + this.user.profilePictureUrl : null;
      if (this.user.profilePictureUrl?.startsWith('http')) {
         this.profilePictureUrl = this.user.profilePictureUrl;
      }
    }

    this.loadingAddress = true;
    this.authService.getUserAddress().subscribe({
      next: (addr) => {
        if (addr) {
          this.address = addr;
        }
        this.loadingAddress = false;
      },
      error: () => this.loadingAddress = false
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
        this.user = updatedUser;
        this.messageService.add({ severity: 'success', summary: this.translate.instant('TOAST.SUCCESS') || 'Success', detail: 'Profile updated successfully' });
        this.loadingProfile = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update profile' });
        this.loadingProfile = false;
      }
    });
  }

  saveAddress(): void {
    this.loadingAddress = true;
    this.authService.updateUserAddress(this.address).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Address updated successfully' });
        this.loadingAddress = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update address' });
        this.loadingAddress = false;
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
              this.profilePictureUrl = environment.apiUrl.replace('/api/', '') + res.url;
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
