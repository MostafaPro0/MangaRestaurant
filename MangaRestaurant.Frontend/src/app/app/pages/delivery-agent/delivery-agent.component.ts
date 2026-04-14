import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule }         from '@angular/common';
import { FormsModule }          from '@angular/forms';
import { TranslateModule }      from '@ngx-translate/core';
import { ButtonModule }         from 'primeng/button';
import { DropdownModule }       from 'primeng/dropdown';
import { CardModule }           from 'primeng/card';
import { TagModule }            from 'primeng/tag';
import { ToastModule }          from 'primeng/toast';
import { MessageService }       from 'primeng/api';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { DeliveryTrackingService } from '../../services/delivery-tracking.service';
import { environment }             from '../../../../environments/environment';

interface OrderOption { label: string; value: string; }

@Component({
  selector: 'app-delivery-agent',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule, ButtonModule,
            DropdownModule, CardModule, TagModule, ToastModule],
  providers: [MessageService],
  templateUrl: './delivery-agent.component.html',
  styleUrl:    './delivery-agent.component.css',
})
export class DeliveryAgentComponent implements OnInit, OnDestroy {

  orders: OrderOption[] = [];
  selectedOrderId = '';
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

  constructor(
    private tracker:  DeliveryTrackingService,
    private http:     HttpClient,
    private toast:    MessageService,
  ) {}

  ngOnInit(): void { this.loadOrders(); }

  private loadOrders(): void {
    const token = JSON.parse(localStorage.getItem('currentUser') ?? '{}')?.token ?? '';
    this.http.get<any[]>(`${environment.apiUrl}/orders/all`, {
      headers: new HttpHeaders({ Authorization: `Bearer ${token}` })
    }).subscribe({
      next: orders => {
        this.orders = orders
          .filter(o => o.status !== 'delivered')
          .map(o => ({ label: `#${o.id} — ${o.buyerName ?? ''}`, value: String(o.id) }));
      },
      error: () => this.toast.add({ severity: 'error', summary: 'Error', detail: 'Could not load orders' })
    });
  }

  startTracking(): void {
    if (!this.selectedOrderId) {
      this.toast.add({ severity: 'warn', summary: 'Select order', detail: 'Please select an order first' });
      return;
    }
    if (!navigator.geolocation) {
      this.toast.add({ severity: 'error', summary: 'Error', detail: 'Geolocation not supported' });
      return;
    }

    this.tracking = true;
    this.watchId  = navigator.geolocation.watchPosition(
      pos => this.broadcast(pos.coords.latitude, pos.coords.longitude),
      ()  => this.toast.add({ severity: 'warn', summary: 'Location', detail: 'Could not get location' }),
      { enableHighAccuracy: true, maximumAge: 5000 }
    );
    this.toast.add({ severity: 'success', summary: 'Started', detail: 'Broadcasting location...' });
  }

  private broadcast(lat: number, lng: number): void {
    this.currentLat = lat;
    this.currentLng = lng;
    const token = JSON.parse(localStorage.getItem('currentUser') ?? '{}')?.token ?? '';
    this.tracker.sendLocation(this.selectedOrderId, lat, lng, this.status, token).catch(console.error);
  }

  stopTracking(): void {
    if (this.watchId !== undefined) navigator.geolocation.clearWatch(this.watchId);
    this.tracker.stopTracking(this.selectedOrderId);
    this.tracking = false;
    this.toast.add({ severity: 'info', summary: 'Stopped', detail: 'Location broadcast stopped' });
  }

  openCurrentLocation(): void {
    if (this.currentLat && this.currentLng)
      window.open(`https://www.google.com/maps?q=${this.currentLat},${this.currentLng}`, '_blank');
  }

  ngOnDestroy(): void {
    if (this.tracking) this.stopTracking();
  }
}
