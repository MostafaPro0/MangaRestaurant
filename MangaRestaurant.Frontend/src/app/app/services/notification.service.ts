import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { MessageService } from 'primeng/api';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { TranslateService } from './translate.service';

import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private hubConnection?: HubConnection;
  private unreadCountSource = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSource.asObservable();

  constructor(
    private messageService: MessageService,
    private authService: AuthService,
    private translateService: TranslateService,
    private router: import('@angular/router').Router
  ) {}

  createHubConnection() {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(environment.hubUrl + 'notifications')
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        this.checkPasswordReminder();
      })
      .catch(error => console.log(error));

    this.hubConnection.on('ReceiveNotification', (notification: any) => {
      this.unreadCountSource.next(this.unreadCountSource.value + 1);
      const title = this.translateService.currentLanguage === 'ar' ? notification.titleAr : notification.title;
      const msg = this.translateService.currentLanguage === 'ar' ? notification.messageAr : notification.message;
      
      this.messageService.add({
        severity: 'info',
        summary: title,
        detail: msg,
        sticky: true,
        icon: this.getNotificationIcon(notification.type)
      });
    });

    this.hubConnection.on('ReceiveAdminNotification', (notification: any) => {
      if (this.authService.isAdmin()) {
        this.unreadCountSource.next(this.unreadCountSource.value + 1);
        this.messageService.add({
          severity: 'warn',
          summary: 'Admin Alert: ' + notification.title,
          detail: notification.message,
          life: 10000
        });
      }
    });

    // Join user-specific group for order updates
    this.authService.user.subscribe(user => {
      if (user && this.hubConnection) {
        this.hubConnection.invoke('JoinUserGroup', user.email);
      }
    });
  }

  clearUnreadCount() {
    this.unreadCountSource.next(0);
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(error => console.log(error));
  }

  private getNotificationIcon(type: string): string {
    switch (type) {
      case 'OrderUpdate': return 'pi pi-shopping-bag';
      case 'NewProduct': return 'pi pi-bolt';
      default: return 'pi pi-bell';
    }
  }

  checkPasswordReminder() {
    this.authService.user.subscribe(user => {
      if (user && user.hasPassword === false) {
        const title = this.translateService.currentLanguage === 'ar' ? 'تنبيه أمني' : 'Security Alert';
        const msg = this.translateService.currentLanguage === 'ar' 
          ? 'يجب تعيين كلمة مرور لحسابك. اضغط هنا للتعيين الآن.' 
          : 'You must set a password for your account. Click here to set it now.';
        
        this.unreadCountSource.next(this.unreadCountSource.value + 1);
        
        this.messageService.add({
          severity: 'warn',
          summary: title,
          detail: msg,
          sticky: true,
          data: { type: 'password_reminder' }
        });
      }
    });

    // Listen for toast clicks
    // Note: PrimeNG Toast onClick handles the event on the message component.
    // We can handle it globally if we use a specific key or data.
  }

  handleNotificationClick(event: any) {
    if (event?.message?.data?.type === 'password_reminder') {
       this.router.navigate(['/profile'], { queryParams: { tab: 'password' } });
       this.messageService.clear();
    }
  }
}
