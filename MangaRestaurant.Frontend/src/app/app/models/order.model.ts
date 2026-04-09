import { UserAddress } from './user-address.model';

export interface OrderItem {
  productId: number;
  productName: string;
  productNameAr?: string;
  pictureUrl: string;
  price: number;
  quantity: number;
}

export enum OrderStatus {
  Pending = 'Pending',
  PaymentReceived = 'PaymentReceived',
  PaymentFailed = 'PaymentFailed',
  Completed = 'Completed',
  Cancelled = 'Cancelled'
}

export interface Order {
  id: number;
  buyerEmail: string;
  orderDate: string;
  shipToAddress?: UserAddress;
  shippingAddress?: UserAddress;
  deliveryMethod: string;
  shippingPrice: number;
  orderStatus: OrderStatus;
  status?: OrderStatus;
  orderItems: OrderItem[];
  discount: number;
  total: number;
  deliveryPersonId?: number;
  deliveryPersonName?: string;
}

export interface DeliveryMethod {
  id: number;
  shortName: string;
  description: string;
  price: number;
}

export interface OrderCreateRequest {
  basketId: string;
  deliveryMethodId: number;
  shippingAddress: UserAddress;
}
