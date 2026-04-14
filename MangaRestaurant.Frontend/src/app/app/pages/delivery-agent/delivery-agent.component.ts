import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule }         from '@angular/common';
import { FormsModule }          from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ButtonModule }         from 'primeng/button';
import { DropdownModule }       from 'primeng/dropdown';
import { CardModule }           from 'primeng/card';
import { TagModule }            from 'primeng/tag';
import { ToastModule }          from 'primeng/toast';
import { BadgeModule }          from 'primeng/badge';
import { MessageService }       from 'primeng/api';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { DeliveryTrackingService } from '../../services/delivery-tracking.service';
import { environment }             from '../../../../environments/environment';

interface AssignedOrder {
  id: number;
  buyerEmail:   string;
  buyerName?:   string;
  buyerPhone?:  string;
  buyerPhone2?: string;
  orderStatus: string;
  orderDate:   string;
  shippingAddress: {
    firstName:  string;
    lastName:   string;
    street:     string;
    city:       string;
    country:    string;
    locationUrl?: string;
    latitude?:  number;
    longitude?: number;
  };
}

@Component({
  selector: 'app-delivery-agent',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, ButtonModule,
            DropdownModule, CardModule, TagModule, ToastModule, BadgeModule],
  providers: [MessageService],
  templateUrl: './delivery-agent.component.html',
  styleUrl:    './delivery-agent.component.css',
})
export class DeliveryAgentComponent implements OnInit, OnDestroy {

  assignedOrders: AssignedOrder[] = [];
  loadingOrders  = false;
  selectedOrder?: AssignedOrder;

  status = 'on_the_way';
  statusOptions = [
    { label: '🛵 On the way', value: 'on_the_way' },
    { label: '📍 Arrived',    value: 'arrived'    },
    { label: '✅ Delivered',   value: 'delivered'  },
  ];

  tracking  = false;
  watchId?: number;
  currentLat?: number;
  currentLng?: number;

  private get token(): string {
    return JSON.parse(localStorage.getItem('currentUser') ?? '{}')?.token ?? '';
  }

  constructor(
    private tracker:   DeliveryTrackingService,
    private http:      HttpClient,
    private toast:     MessageService,
    private translate: TranslateService,
  ) {}

  ngOnInit(): void { this.loadAssignedOrders(); }

  loadAssignedOrders(): void {
    this.loadingOrders = true;
    this.http.get<AssignedOrder[]>(`${environment.apiUrl}/orders/my-deliveries`, {
      headers: new HttpHeaders({ Authorization: `Bearer ${this.token}` })
    }).subscribe({
      next: orders => {
        this.assignedOrders = orders;
        this.loadingOrders  = false;
      },
      error: () => {
        this.toast.add({
          severity: 'error',
          summary: this.translate.instant('DELIVERY.TOAST_LOAD_ERROR'),
          detail:  this.translate.instant('DELIVERY.TOAST_LOAD_ERROR')
        });
        this.loadingOrders = false;
      }
    });
  }

  selectOrder(order: AssignedOrder): void {
    if (this.tracking) this.stopTracking();
    this.selectedOrder = order;
  }

  startTracking(): void {
    if (!this.selectedOrder) return;
    if (!navigator.geolocation) {
      this.toast.add({
        severity: 'error',
        summary: this.translate.instant('DELIVERY.TOAST_NO_GEO'),
        detail:  this.translate.instant('DELIVERY.TOAST_NO_GEO')
      });
      return;
    }

    this.tracking = true;
    this.watchId  = navigator.geolocation.watchPosition(
      pos => this.broadcast(pos.coords.latitude, pos.coords.longitude),
      ()  => this.toast.add({
        severity: 'warn',
        summary: this.translate.instant('DELIVERY.TOAST_LOC_ERROR'),
        detail:  this.translate.instant('DELIVERY.TOAST_LOC_ERROR')
      }),
      { enableHighAccuracy: true, maximumAge: 5000 }
    );
    this.toast.add({
      severity: 'success',
      summary: this.translate.instant('DELIVERY.TOAST_STARTED_SUMMARY'),
      detail:  this.translate.instant('DELIVERY.TOAST_STARTED_DETAIL')
    });
  }

  private broadcast(lat: number, lng: number): void {
    this.currentLat = lat;
    this.currentLng = lng;
    this.tracker.sendLocation(
      String(this.selectedOrder!.id), lat, lng, this.status, this.token
    ).catch(console.error);
  }

  stopTracking(): void {
    if (this.watchId !== undefined) navigator.geolocation.clearWatch(this.watchId);
    if (this.selectedOrder) this.tracker.stopTracking(String(this.selectedOrder.id));
    this.tracking = false;
    this.toast.add({
      severity: 'info',
      summary: this.translate.instant('DELIVERY.TOAST_STOPPED_SUMMARY'),
      detail:  this.translate.instant('DELIVERY.TOAST_STOPPED_DETAIL')
    });
  }

  openCurrentLocation(): void {
    if (this.currentLat && this.currentLng)
      window.open(`https://www.google.com/maps?q=${this.currentLat},${this.currentLng}`, '_blank');
  }

  statusSeverity(status: string): 'info' | 'success' | 'warning' | 'danger' {
    const map: Record<string, any> = {
      Pending: 'warning', Shipped: 'info', Processing: 'info',
      Confirmed: 'info', PaymentReceived: 'info',
      Delivered: 'success', Cancelled: 'danger'
    };
    return map[status] ?? 'info';
  }

  /** Builds the best Google Maps link for an order's shipping address */
  getMapUrl(order: AssignedOrder): string {
    const addr = order.shippingAddress;
    if (addr?.latitude && addr?.longitude)
      return `https://www.google.com/maps?q=${addr.latitude},${addr.longitude}`;
    if (addr?.locationUrl)
      return addr.locationUrl;
    const text = [addr?.street, addr?.city, addr?.country].filter(Boolean).join(', ');
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(text)}`;
  }

  ngOnDestroy(): void {
    if (this.tracking) this.stopTracking();
  }
}
