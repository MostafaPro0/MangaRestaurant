export interface Product {
  id: number;
  name: string;
  nameAr?: string;
  description: string;
  descriptionAr?: string;
  price: number;
  oldPrice?: number;
  discountPercentage?: number;
  pictureUrl: string;
  brand: string;
  brandAr?: string;
  category: string;
  categoryAr?: string;
  quantityInStock: number;
  views: number;
  averageRating: number;
  reviews: Review[];
}

export interface Review {
  id?: number;
  productId: number;
  userName?: string;
  email?: string;
  rating: number;
  comment: string;
  createdAt?: string;
}
