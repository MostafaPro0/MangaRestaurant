import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Product, Review } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
  constructor(private api: ApiService) {}

  getProducts(pageIndex = 1, pageSize = 12, search = '', categoryId: number | null = null, brandId: number | null = null, showHidden = false): Observable<any> {
    const params: any = {
      pageIndex: pageIndex.toString(),
      pageSize: pageSize.toString(),
    };

    if (search) params.search = search;
    if (categoryId) params.categoryId = categoryId.toString();
    if (brandId) params.brandId = brandId.toString();
    if (showHidden) params.showHidden = 'true';

    return this.api.get<{ data: Product[]; count: number; pageIndex: number; pageSize: number }>('Products', params);
  }

  getProduct(id: number): Observable<Product> {
    return this.api.get<Product>(`Products/${id}`);
  }

  createProduct(payload: Partial<Product>): Observable<Product> {
    return this.api.post<Product>('Products', payload);
  }

  updateProduct(id: number, payload: Partial<Product>): Observable<Product> {
    return this.api.put<Product>(`Products/${id}`, payload);
  }

  deleteProduct(id: number): Observable<boolean> {
    return this.api.delete<boolean>(`Products/${id}`);
  }

  getCategories(showHidden: boolean = false): Observable<any[]> {
    return this.api.get<any[]>('ProductCategory', { showHidden: showHidden.toString() });
  }

  createCategory(payload: any): Observable<any> {
    return this.api.post<any>('ProductCategory', payload);
  }

  updateCategory(id: number, payload: any): Observable<any> {
    return this.api.put<any>(`ProductCategory/${id}`, payload);
  }

  deleteCategory(id: number): Observable<any> {
    return this.api.delete<any>(`ProductCategory/${id}`);
  }

  toggleCategoryVisibility(id: number, hide: boolean): Observable<any> {
    return this.api.put<any>(`ProductCategory/${id}/hide?hide=${hide}`, {});
  }

  getBrands(showHidden: boolean = false): Observable<any[]> {
    return this.api.get<any[]>('ProductBrand', { showHidden: showHidden.toString() });
  }

  createBrand(payload: any): Observable<any> {
    return this.api.post<any>('ProductBrand', payload);
  }

  updateBrand(id: number, payload: any): Observable<any> {
    return this.api.put<any>(`ProductBrand/${id}`, payload);
  }

  deleteBrand(id: number): Observable<any> {
    return this.api.delete<any>(`ProductBrand/${id}`);
  }

  toggleBrandVisibility(id: number, hide: boolean): Observable<any> {
    return this.api.put<any>(`ProductBrand/${id}/hide?hide=${hide}`, {});
  }

  getDeals(): Observable<any[]> {
    return this.api.get<any[]>('Products/deals');
  }

  getLatestProducts(): Observable<any[]> {
    return this.api.get<any[]>('Products/latest');
  }

  addReview(review: Review): Observable<Review> {
    return this.api.post<Review>('Reviews', review);
  }
}

