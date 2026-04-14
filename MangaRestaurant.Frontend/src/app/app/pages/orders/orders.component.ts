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
import { RouterLink } from '@angular/router';

import { SettingsService } from '../../services/settings.service';

import { environment } from '../../../../environments/environment';

import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, ProgressSpinnerModule, BadgeModule, DialogModule, ButtonModule, TranslateModule, SkeletonModule, DividerModule, RouterLink, FormsModule, InputTextModule, TooltipModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  selectedOrder: Order | null = null;
  orderDialogVisible = false;
  loading = false;
  settings$: any;

  // Edit Address status
  editAddressVisible = false;
  editableAddress: any = null;
  updatingAddress = false;
  detectingLocation = false;

  constructor(
    private ordersService: OrdersService,
    private messageService: MessageService,
    public translate: TranslateService,
    private settingsService: SettingsService
  ) { }

  getImageUrl(url: string | null | undefined): string {
    if (!url) return 'assets/images/placeholder.png';
    if (url.startsWith('http')) return url;
    return `${environment.apiUrl}${url}`;
  }

  onImageError(event: any, item: any) {
    const img = event.target;
    const placeholder = 'assets/images/placeholder.png'; // Updated path
    
    // Prevent infinite loop: if the current image is already the placeholder or a transparent dot, stop.
    if (img.src.includes('data:image/gif;base64')) return;

    // 1. Try current product image if old version fails
    if (item.currentPictureUrl && img.src !== item.currentPictureUrl) {
      img.src = item.currentPictureUrl;
    } 
    // 2. Try local placeholder if current image also fails
    else if (!img.src.endsWith(placeholder)) {
      img.src = placeholder;
    } 
    // 3. Absolute last resort: transparent 1x1 pixel to stop all loops
    else {
      img.src = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7';
    }
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

  isOrderPending(order: Order | null): boolean {
    if (!order) return false;
    const status = this.getDisplayStatus(order);
    return status === 'Pending' || status === OrderStatus.Pending;
  }

  toggleEditAddress(): void {
    if (this.selectedOrder) {
      const addr = this.getShippingAddress(this.selectedOrder);
      this.editableAddress = { ...addr };
      this.editAddressVisible = true;
    }
  }

  saveAddress(): void {
    if (!this.selectedOrder || !this.editableAddress) return;

    this.updatingAddress = true;
    this.ordersService.updateOrderAddress(this.selectedOrder.id, this.editableAddress).subscribe({
      next: (updatedOrder) => {
        // Update local list and selected order
        const index = this.orders.findIndex(o => o.id === updatedOrder.id);
        if (index !== -1) {
          this.orders[index] = updatedOrder;
        }
        this.selectedOrder = updatedOrder;
        this.updatingAddress = false;
        this.editAddressVisible = false;
        this.messageService.add({ 
            severity: 'success', 
            summary: this.translate.instant('TOAST.SUCCESS') || 'Success', 
            detail: this.translate.instant('ORDERS.ADDRESS_UPDATED') || 'Address updated successfully.' 
        });
      },
      error: (err) => {
        console.error('Update address failed', err);
        this.updatingAddress = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Could not update address.' });
      }
    });
  }

  detectLocation(): void {
    if (!navigator.geolocation) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Geolocation not supported' });
      return;
    }

    this.detectingLocation = true;
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        this.editableAddress.latitude = pos.coords.latitude;
        this.editableAddress.longitude = pos.coords.longitude;
        this.editableAddress.locationUrl = `https://www.google.com/maps?q=${pos.coords.latitude},${pos.coords.longitude}`;
        this.detectingLocation = false;
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Location detected!' });
      },
      (err) => {
        this.detectingLocation = false;
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Location access denied' });
      }
    );
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
