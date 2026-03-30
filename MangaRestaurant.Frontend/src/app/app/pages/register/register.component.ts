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

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule, MessageModule, CardModule, TranslateModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  displayName = '';
  email = '';
  password = '';
  phoneNumber = '';
  error = '';

  constructor(private authService: AuthService, private router: Router) {}

  register(): void {
    this.error = '';
    this.authService.register(this.displayName, this.email, this.password, this.phoneNumber).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => (this.error = 'Failed to register: ' + (err?.message || err))
    });
  }
}
