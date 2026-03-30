import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Order, OrderCreateRequest, OrderItem, DeliveryMethod } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrdersService {
  constructor(private api: ApiService) {}

  createOrder(payload: OrderCreateRequest): Observable<Order> {
    return this.api.post<Order>('Orders', {
      basketId: payload.basketId,
      deliveryMethodId: payload.deliveryMethodId,
      shippingAddress: payload.shippingAddress
    });
  }

  getOrders(): Observable<Order[]> {
    return this.api.get<Order[]>('Orders');
  }

  getOrder(id: number): Observable<Order> {
    return this.api.get<Order>(`Orders/${id}`);
  }

  getDeliveryMethods(): Observable<DeliveryMethod[]> {
    return this.api.get<DeliveryMethod[]>('Orders/DeliveryMethods');
  }

  updateOrderStatus(orderId: number, status: string): Observable<Order> {
    return this.api.put<Order>(`Orders/${orderId}`, { status });
  }
}

