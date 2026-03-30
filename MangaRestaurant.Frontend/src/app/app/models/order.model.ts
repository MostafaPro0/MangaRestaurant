import { UserAddress } from './user-address.model';

export interface OrderItem {
  productId: number;
  productName: string;
  pictureUrl: string;
  price: number;
  quantity: number;
}

export interface Order {
  id: number;
  buyerEmail: string;
  orderDate: string;
  shipToAddress: UserAddress;
  deliveryMethod: string;
  shippingPrice: number;
  status: string;
  orderItems: OrderItem[];
  subtotal: number;
  total: number;
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
