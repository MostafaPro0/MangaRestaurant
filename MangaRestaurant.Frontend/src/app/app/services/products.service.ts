import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Product } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
  constructor(private api: ApiService) {}

  getProducts(pageIndex = 1, pageSize = 12, search = '', categoryId: number | null = null, brandId: number | null = null): Observable<any> {
    const params: any = {
      pageIndex: pageIndex.toString(),
      pageSize: pageSize.toString(),
    };

    if (search) params.search = search;
    if (categoryId) params.categoryId = categoryId.toString();
    if (brandId) params.brandId = brandId.toString();

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

  getCategories(): Observable<any[]> {
    return this.api.get<any[]>('ProductCategory');
  }

  getBrands(): Observable<any[]> {
    return this.api.get<any[]>('ProductBrand');
  }
}

