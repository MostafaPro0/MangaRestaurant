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
import { SettingsService } from './settings.service';
import { TenantService } from './tenant.service';

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
    private basketService: BasketService,
    private settingsService: SettingsService,
    private tenantService: TenantService
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
      .withUrl(`${environment.hubUrl}notifications?tenant=${this.tenantService.getTenantSlug()}`)
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
    this.hubConnection.on('PriceUpdated', (data: any) => {
      const productId = data.productId || data.ProductId;
      const newPrice = data.newPrice || data.NewPrice;
      
      const current = this.basketService.getCurrentBasket();
      const affectedItem = current.items.find(item => item.productId === productId);

      if (affectedItem) {
        // Force refresh the basket from the API to get the latest synced prices
        // Our API GetCustomerBasket now has logic to sync prices from DB
        this.basketService.getBasket(current.id).subscribe({
          next: (updatedBasket) => {
            console.log('Basket refreshed after price update');
            
            // Notify the user with a Toast
            const isAr = this.translateService.currentLanguage === 'ar';
            const productNameAr = data.productNameAr || affectedItem.productNameAr;
            const productLabel = isAr ? (productNameAr || data.productName) : data.productName;
            
            this.messageService.add({
              severity: 'info',
              summary: isAr ? '📢 تحديث تلقائي' : '📢 Auto Update',
              detail: isAr 
                ? `تم تحديث سعر "${productLabel}" في سلتك` 
                : `Price updated for "${productLabel}" in your cart`,
              life: 5000,
              icon: 'pi pi-sync'
            });
          }
        });
      }
    });

    this.hubConnection.on('SettingsUpdated', () => {
      console.log('[SignalR] Site settings updated. Refreshing...');
      this.settingsService.loadSettings();
      
      const isAr = this.translateService.currentLanguage === 'ar';
      this.messageService.add({
        severity: 'info',
        summary: isAr ? '⚙️ تحديث الإعدادات' : '⚙️ Settings Updated',
        detail: isAr 
          ? 'تم تحديث إعدادات الموقع بنجاح' 
          : 'Site settings have been updated in real-time',
        life: 3000
      });
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
