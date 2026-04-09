import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { MessageService } from 'primeng/api';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { TranslateService } from './translate.service';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { BasketService } from './basket.service';

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
    private router: Router,
    private http: HttpClient,
    private basketService: BasketService
  ) {}

  loadNotifications() {
    this.http.get<any[]>(environment.apiUrl + '/notifications').subscribe({
      next: (notifications) => {
        this.notificationsSource.next(notifications);
        const unread = notifications.filter(n => !n.isRead).length;
        this.unreadCountSource.next(unread);
      },
      error: (e) => console.log(e)
    });
  }

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
      this.addNotification(notification); 
      
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

    // Listen for real-time price updates and sync basket items accordingly
    this.hubConnection.on('PriceUpdated', (data: { productId: number; productName: string; newPrice: number }) => {
      const current = this.basketService.getCurrentBasket();
      const affectedItem = current.items.find(item => item.productId === data.productId);

      if (affectedItem) {
        // Update the price for the affected item
        const updatedItems = current.items.map(item =>
          item.productId === data.productId ? { ...item, price: data.newPrice } : item
        );
        const updatedBasket = { ...current, items: updatedItems };

        // Persist the updated price to Redis
        this.basketService.updateBasket(updatedBasket).subscribe();

        // Notify the user
        const isAr = this.translateService.currentLanguage === 'ar';
        const productLabel = isAr ? (affectedItem.productNameAr || data.productName) : data.productName;
        this.messageService.add({
          severity: 'warn',
          summary: isAr ? '⚠️ تحديث سعر' : '⚠️ Price Updated',
          detail: isAr
            ? `تم تحديث سعر "${productLabel}" في سلتك إلى $${data.newPrice.toFixed(2)}`
            : `Price of "${productLabel}" in your cart updated to $${data.newPrice.toFixed(2)}`,
          life: 6000,
          icon: 'pi pi-tag'
        });
      }
    });

    this.authService.user.subscribe(user => {
      if (user) {
        if (this.hubConnection) {
          this.hubConnection.invoke('JoinUserGroup', user.email);
          if (this.authService.isAdmin(user.token)) {
             this.hubConnection.invoke('JoinAdminGroup');
          }
        }
        this.loadNotifications();
      } else {
        this.notificationsSource.next([]);
        this.unreadCountSource.next(0);
      }
    });
  }

  clearUnreadCount() {
    this.http.post(environment.apiUrl + '/notifications/read-all', {}).subscribe(() => {
      this.unreadCountSource.next(0);
      
      const updated = this.notificationsSource.value.map(n => ({...n, isRead: true}));
      this.notificationsSource.next(updated);
    });
  }
  
  markAsRead(id: number) {
    this.http.post(environment.apiUrl + `/notifications/${id}/read`, {}).subscribe(() => {
      const current = this.notificationsSource.value;
      const index = current.findIndex(n => n.id === id);
      if (index !== -1) {
         current[index].isRead = true;
         this.notificationsSource.next([...current]);
         
         const unread = Math.max(0, this.unreadCountSource.value - 1);
         this.unreadCountSource.next(unread);
      }
    });
  }

  addNotification(notification: any) {
    const current = this.notificationsSource.value;
    if (notification.type === 'password_reminder' && current.some(n => n.type === 'password_reminder')) {
       return;
    }

    const newNotif = {
      ...notification,
      id: notification.id || Date.now(), 
      createdAt: new Date(),
      isRead: false
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
      case 'PasswordChange':
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
  }

  handleNotificationClick(event: any) {
    const msg = event?.message || event;
    const data = msg?.data || msg;
    
    if (data.id && !data.isRead) {
      this.markAsRead(data.id);
    }
    
    switch(data.type) {
        case 'password_reminder':
        case 'PasswordChange':
            this.router.navigate(['/profile'], { queryParams: { tab: 'password' } });
            break;
        case 'OrderUpdate':
            // Redirect to orders
            this.router.navigate(['/orders']); 
            break;
        case 'NewOrder':
            // Logic admin order
            this.router.navigate(['/admin'], { queryParams: { tab: 'orders', orderId: data.relatedId }});
            break;
        case 'NewProduct':
            if (data.relatedId) {
               this.router.navigate(['/product', data.relatedId]);
            } else {
               this.router.navigate(['/products']);
            }
            break;
    }
    this.messageService.clear();
  }
}
