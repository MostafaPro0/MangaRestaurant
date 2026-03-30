export interface BasketItem {
  productId: number;
  productName: string;
  price: number;
  quantity: number;
  pictureUrl: string;
}

export interface CustomerBasket {
  id: string;
  items: BasketItem[];
}
