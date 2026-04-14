import {
  Component, OnInit, OnDestroy,
  Input, ElementRef, ViewChild, AfterViewInit
} from '@angular/core';
import { CommonModule }          from '@angular/common';
import { ActivatedRoute }        from '@angular/router';
import { TranslateModule }       from '@ngx-translate/core';
import { ButtonModule }          from 'primeng/button';
import { CardModule }            from 'primeng/card';
import { TagModule }             from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import * as L from 'leaflet';

import { DeliveryTrackingService, DeliveryLocation } from '../../services/delivery-tracking.service';
import { Subscription }          from 'rxjs';

// Fix default marker icons for Leaflet when bundled
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl:       'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl:     'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

@Component({
  selector:    'app-order-tracking',
  standalone:  true,
  imports: [CommonModule, TranslateModule, ButtonModule, CardModule, TagModule, ProgressSpinnerModule],
  templateUrl: './order-tracking.component.html',
  styleUrl:    './order-tracking.component.css',
})
export class OrderTrackingComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('mapContainer') mapContainer!: ElementRef<HTMLDivElement>;

  orderId    = '';
  connected  = false;
  location: DeliveryLocation | null = null;
  statusLabel: Record<string, string> = {
    on_the_way: 'TRACKING.ON_THE_WAY',
    arrived:    'TRACKING.ARRIVED',
    delivered:  'TRACKING.DELIVERED',
  };
  statusSeverity: Record<string, string> = {
    on_the_way: 'warning',
    arrived:    'info',
    delivered:  'success',
  };

  private map?: L.Map;
  private marker?: L.Marker;
  private subs = new Subscription();

  constructor(
    private route:    ActivatedRoute,
    private tracker:  DeliveryTrackingService,
  ) {}

  ngOnInit(): void {
    this.orderId = this.route.snapshot.paramMap.get('id') ?? '';
    const token  = JSON.parse(localStorage.getItem('currentUser') ?? '{}')?.token ?? '';
    this.tracker.startTracking(this.orderId, token);

    this.subs.add(
      this.tracker.connected$.subscribe(v => this.connected = v)
    );
    this.subs.add(
      this.tracker.location$.subscribe(loc => {
        this.location = loc;
        if (loc) this.updateMap(loc.latitude, loc.longitude);
      })
    );
  }

  ngAfterViewInit(): void {
    this.initMap();
  }

  private initMap(): void {
    this.map = L.map(this.mapContainer.nativeElement).setView([30.0444, 31.2357], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors',
      maxZoom: 19,
    }).addTo(this.map);
  }

  private updateMap(lat: number, lng: number): void {
    if (!this.map) return;
    const latlng = L.latLng(lat, lng);

    if (!this.marker) {
      this.marker = L.marker(latlng, {
        icon: L.divIcon({
          className: '',
          html: `<div class="delivery-marker">🛵</div>`,
          iconSize: [40, 40],
          iconAnchor: [20, 20],
        })
      }).addTo(this.map);
    } else {
      this.marker.setLatLng(latlng);
    }

    this.map.setView(latlng, 16, { animate: true });
  }

  openInGoogleMaps(): void {
    if (!this.location) return;
    window.open(
      `https://www.google.com/maps?q=${this.location.latitude},${this.location.longitude}`,
      '_blank'
    );
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.tracker.stopTracking(this.orderId);
    this.map?.remove();
  }
}
