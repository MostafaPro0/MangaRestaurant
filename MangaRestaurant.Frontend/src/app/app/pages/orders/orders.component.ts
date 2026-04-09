import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { Order, OrderStatus } from '../../models/order.model';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BadgeModule } from 'primeng/badge';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { SkeletonModule } from 'primeng/skeleton';
import { DividerModule } from 'primeng/divider';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OrdersService } from '../../services/orders.service';

import { SettingsService } from '../../services/settings.service';

import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, ProgressSpinnerModule, BadgeModule, DialogModule, ButtonModule, TranslateModule, SkeletonModule, DividerModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  selectedOrder: Order | null = null;
  orderDialogVisible = false;
  loading = false;
  settings$: any;

  constructor(
    private ordersService: OrdersService, 
    private messageService: MessageService, 
    public translate: TranslateService,
    private settingsService: SettingsService
  ) {}

  getImageUrl(url: string | null | undefined): string {
    if (!url) return 'assets/images/products/placeholder.png';
    if (url.startsWith('http')) return url;
    return `${environment.apiUrl}${url}`;
  }

  statusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    const s = (status || '').toLowerCase().replace(/\s+/g, '');
    if (s.includes('received') || s.includes('completed') || s.includes('delivered')) return 'success';
    if (s.includes('fail') || s.includes('cancel')) return 'danger';
    if (s.includes('prep') || s.includes('process') || s.includes('shipped')) return 'info';
    if (s.includes('pend') || s.includes('waiting')) return 'warn';
    return 'secondary';
  }

  getDisplayStatus(order: Order): string {
    return (order.orderStatus || order.status || OrderStatus.Pending);
  }

  getShippingAddress(order: Order) {
    return order.shippingAddress || order.shipToAddress;
  }

  getOrderShortId(order: Order): string {
    const id = (order as any).id;
    if (!id && id !== 0) return '-';
    const str = String(id);
    // UUID format: show first 8 chars
    if (str.includes('-') && str.length > 12) {
      return str.substring(0, 8).toUpperCase();
    }
    return str;
  }

  openOrderDetails(order: Order): void {
    this.selectedOrder = order;
    this.orderDialogVisible = true;

    // Fetch full order details with items from API
    this.ordersService.getOrder(order.id).subscribe({
      next: (fullOrder) => {
        this.selectedOrder = fullOrder;
      }
    });
  }

  closeOrderDetails(): void {
    this.selectedOrder = null;
    this.orderDialogVisible = false;
  }

  ngOnInit(): void {
    this.settings$ = this.settingsService.settings$;
    this.loading = true;
    this.ordersService.getOrders().subscribe({
      next: (data) => {
        this.orders = data || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load orders', err);
        this.messageService.add({ severity: 'error', summary: this.translate.instant('TOAST.ERROR') || 'Error', detail: this.translate.instant('TOAST.ORDERS_FAIL') || 'Could not load orders.' });
        this.loading = false;
      }
    });
  }
}
