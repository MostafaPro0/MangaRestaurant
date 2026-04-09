import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Order, OrderStatus, OrderCreateRequest, OrderItem, DeliveryMethod } from '../models/order.model';
import { map } from 'rxjs/operators';

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

  private normalizeStatus(rawStatus: string | OrderStatus | undefined): OrderStatus {
    if (!rawStatus) return OrderStatus.Pending;

    const normalized = String(rawStatus).replace(/\s+/g, '').toLowerCase();

    switch (normalized) {
      case 'paymentreceived':
        return OrderStatus.PaymentReceived;
      case 'paymentfailed':
        return OrderStatus.PaymentFailed;
      case 'completed':
        return OrderStatus.Completed;
      case 'cancelled':
      case 'canceled':
        return OrderStatus.Cancelled;
      case 'pending':
      default:
        return OrderStatus.Pending;
    }
  }

  getOrders(): Observable<Order[]> {
    return this.api.get<Order[]>('Orders').pipe(
      map((orders) =>
        orders.map((o) => {
          const resolvedStatus = this.normalizeStatus(o.orderStatus ?? o.status);
          return {
            ...o,
            status: resolvedStatus,
            orderStatus: resolvedStatus,
            orderItems: (o as any).items ?? o.orderItems ?? []
          } as Order;
        })
      )
    );
  }

  getOrder(id: number): Observable<Order> {
    return this.api.get<Order>(`Orders/${id}`).pipe(
      map((o) => {
        const resolvedStatus = this.normalizeStatus(o.orderStatus ?? o.status);
        return {
          ...o,
          status: resolvedStatus,
          orderStatus: resolvedStatus,
          orderItems: (o as any).items ?? o.orderItems ?? []
        } as Order;
      })
    );
  }

  getDeliveryMethods(): Observable<DeliveryMethod[]> {
    return this.api.get<DeliveryMethod[]>('Orders/DeliveryMethods');
  }

  getAllOrdersAdmin(): Observable<Order[]> {
    return this.api.get<Order[]>('Orders/Admin/All').pipe(
      map((orders) =>
        orders.map((o) => {
          const resolvedStatus = this.normalizeStatus(o.orderStatus ?? o.status);
          return {
            ...o,
            status: resolvedStatus,
            orderStatus: resolvedStatus,
            orderItems: (o as any).items ?? o.orderItems ?? []
          } as Order;
        })
      )
    );
  }

  updateOrderStatus(orderId: number, status: string): Observable<Order> {
    // API expects a DTO with PascalCase 'Status' property
    return this.api.put<Order>(`Orders/${orderId}/status`, { Status: status });
  }

  assignDelivery(orderId: number, employeeId: number): Observable<any> {
    return this.api.put(`Orders/${orderId}/assign-delivery`, { EmployeeId: employeeId });
  }
}

