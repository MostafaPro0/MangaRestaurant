import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Product } from '../models/product.model';

@Injectable({
  providedIn: 'root'
})
export class ProductsService {
  constructor(private api: ApiService) {}

  getProducts(pageIndex = 1, pageSize = 12, search = '', category = '', brand = ''): Observable<any> {
    const params: any = {
      pageIndex: pageIndex.toString(),
      pageSize: pageSize.toString(),
    };

    if (search) params.search = search;
    if (category) params.category = category;
    if (brand) params.brand = brand;

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
}

