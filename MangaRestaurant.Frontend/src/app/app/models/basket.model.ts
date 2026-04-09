export interface BasketItem {
  productId: number;
  productName: string;
  productNameAr?: string;
  price: number;
  quantity: number;
  pictureUrl: string;
  brand?: string;
  category?: string;
}

export interface CustomerBasket {
  id: string;
  items: BasketItem[];
  paymentIntentId?: string;
  clientSecret?: string;
  deliveryMethodId?: number;
}
