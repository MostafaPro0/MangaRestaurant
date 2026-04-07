import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
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

  private notificationsSource = new BehaviorSubject<any[]>([]);
  notifications$ = this.notificationsSource.asObservable();

  constructor(
    private messageService: MessageService,
    private authService: AuthService,
    private translateService: TranslateService,
    private router: Router
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
      this.addNotification(notification);
      
      const title = this.translateService.currentLanguage === 'ar' ? notification.titleAr : notification.title;
      const msg = this.translateService.currentLanguage === 'ar' ? notification.messageAr : notification.message;
      
      this.messageService.add({
        severity: 'info',
        summary: title,
        detail: msg,
        sticky: true,
        styleClass: 'clickable-toast',
        icon: this.getNotificationIcon(notification.type),
        data: notification
      });
    });

    this.hubConnection.on('ReceiveAdminNotification', (notification: any) => {
      this.addNotification(notification); // Add to dropdown list
      
      this.messageService.add({
        severity: 'warn',
        summary: (this.translateService.currentLanguage === 'ar' ? 'تنبيه إداري: ' : 'Admin Alert: ') + notification.title,
        detail: notification.message,
        sticky: true,
        styleClass: 'clickable-toast',
        icon: this.getNotificationIcon(notification.type),
        data: notification
      });
    });

    // Join groups based on user role/identity
    this.authService.user.subscribe(user => {
      if (user && this.hubConnection) {
        this.hubConnection.invoke('JoinUserGroup', user.email);
        
        if (this.authService.isAdmin(user.token)) {
           this.hubConnection.invoke('JoinAdminGroup');
        }
      }
    });
  }

  clearUnreadCount() {
    this.unreadCountSource.next(0);
  }

  addNotification(notification: any) {
    const current = this.notificationsSource.value;
    // Prevent duplicates for specific local types
    if (notification.type === 'password_reminder' && current.some(n => n.type === 'password_reminder')) {
       return;
    }

    const newNotif = {
      ...notification,
      id: Date.now(),
      timestamp: new Date(),
      read: false
    };

    this.notificationsSource.next([newNotif, ...current]);
    this.unreadCountSource.next(this.unreadCountSource.value + 1);
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(error => console.log(error));
  }

  private getNotificationIcon(type: string): string {
    switch (type) {
      case 'OrderUpdate': return 'pi pi-shopping-bag';
      case 'NewProduct': return 'pi pi-bolt';
      case 'NewOrder': return 'pi pi-shopping-cart';
      case 'NewReview': return 'pi pi-star-fill';
      case 'password_reminder': return 'pi pi-shield';
      default: return 'pi pi-bell';
    }
  }

  checkPasswordReminder() {
    this.authService.user.subscribe(user => {
      if (user && user.hasPassword === false) {
        const titleAr = 'تنبيه أمني';
        const titleEn = 'Security Alert';
        const msgAr = 'يجب تعيين كلمة مرور لحسابك. اضغط هنا للتعيين الآن.';
        const msgEn = 'You must set a password for your account. Click here to set it now.';
        
        const notification = {
           type: 'password_reminder',
           title: titleEn,
           titleAr: titleAr,
           message: msgEn,
           messageAr: msgAr
        };

        this.addNotification(notification);

        this.messageService.add({
          severity: 'warn',
          summary: this.translateService.currentLanguage === 'ar' ? titleAr : titleEn,
          detail: this.translateService.currentLanguage === 'ar' ? msgAr : msgEn,
          sticky: true,
          styleClass: 'clickable-toast',
          data: { type: 'password_reminder' }
        });
      }
    });

    // Listen for toast clicks
    // Note: PrimeNG Toast onClick handles the event on the message component.
    // We can handle it globally if we use a specific key or data.
  }

  handleNotificationClick(event: any) {
    // Check if event is Message or has message property
    const msg = event?.message || event;
    if (msg?.data?.type === 'password_reminder') {
       this.router.navigate(['/profile'], { queryParams: { tab: 'password' } });
       this.messageService.clear();
    }
  }
}
