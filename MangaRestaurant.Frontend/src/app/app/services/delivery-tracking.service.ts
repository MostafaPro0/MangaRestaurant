import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface DeliveryLocation {
  orderId:   string;
  latitude:  number;
  longitude: number;
  status:    'on_the_way' | 'arrived' | 'delivered';
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class DeliveryTrackingService {
  private hubConnection?: signalR.HubConnection;
  private readonly hubUrl = `${environment.apiUrl.replace('/api', '')}/hub/delivery`;

  /** Emits every location update for the tracked order */
  location$ = new BehaviorSubject<DeliveryLocation | null>(null);
  /** Connection state */
  connected$ = new BehaviorSubject<boolean>(false);

  // ── Customer: watch an order ────────────────────────────────────────────────
  async startTracking(orderId: string, token: string): Promise<void> {
    if (this.hubConnection) await this.stopTracking(orderId);

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token,
        transport: signalR.HttpTransportType.WebSockets |
                   signalR.HttpTransportType.ServerSentEvents |
                   signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveLocation', (data: DeliveryLocation) => {
      this.location$.next(data);
    });

    this.hubConnection.onclose(() => this.connected$.next(false));
    this.hubConnection.onreconnected(() => this.connected$.next(true));

    await this.hubConnection.start();
    this.connected$.next(true);
    await this.hubConnection.invoke('JoinOrderGroup', orderId);
  }

  async stopTracking(orderId: string): Promise<void> {
    if (!this.hubConnection) return;
    try {
      await this.hubConnection.invoke('LeaveOrderGroup', orderId);
      await this.hubConnection.stop();
    } catch { /* ignore */ }
    this.hubConnection = undefined;
    this.connected$.next(false);
    this.location$.next(null);
  }

  // ── Delivery Agent: broadcast location ─────────────────────────────────────
  async sendLocation(orderId: string, lat: number, lng: number, status: string, token: string): Promise<void> {
    if (!this.hubConnection || this.hubConnection.state !== signalR.HubConnectionState.Connected) {
      await this.startTracking(orderId, token);   // re-use same connection
    }
    await this.hubConnection!.invoke('SendLocation', orderId, lat, lng, status);
  }
}
