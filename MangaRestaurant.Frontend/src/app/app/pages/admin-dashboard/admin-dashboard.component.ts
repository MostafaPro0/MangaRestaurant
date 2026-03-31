import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { Textarea } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { TabViewModule } from 'primeng/tabview';
import { AdminService } from '../../services/admin.service';
import { ProductsService } from '../../services/products.service';
import { OrdersService } from '../../services/orders.service';
import { Product } from '../../models/product.model';
import { Order, OrderStatus } from '../../models/order.model';
import { CurrencyPipe, DatePipe } from '@angular/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    CardModule,
    TableModule,
    ProgressSpinnerModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    Textarea,
    DropdownModule,
    TabViewModule,
    CurrencyPipe,
    DatePipe,
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.css'
})
export class AdminDashboardComponent implements OnInit {
  employees: any[] = [];

  products: Product[] = [];
  orders: Order[] = [];

  loadingEmployees = false;
  loadingProducts = false;
  loadingOrders = false;

  // Products dialog
  productDialogVisible = false;
  productDialogTitle = '';
  editingProductId: number | null = null;

  productDraft: Partial<Product> = {
    name: '',
    description: '',
    pictureUrl: '',
    brand: '',
    category: '',
    price: 0,
    quantityInStock: 0,
  };

  // Orders status editing
  orderStatusDraft: Record<number, string> = {};
  orderStatusOptions: { label: string; value: string }[] = [];

  constructor(
    private adminService: AdminService,
    private productsService: ProductsService,
    private ordersService: OrdersService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadEmployees();
    this.loadProducts();
    this.loadOrders();
  }

  loadEmployees(): void {
    this.loadingEmployees = true;
    this.adminService.getEmployees().subscribe({
      next: (data) => {
        this.employees = data ?? [];
        this.loadingEmployees = false;
      },
      error: () => (this.loadingEmployees = false),
    });
  }

  loadProducts(): void {
    this.loadingProducts = true;
    this.productsService.getProducts(1, 1000).subscribe({
      next: (result) => {
        this.products = result?.data ?? [];
        this.loadingProducts = false;
      },
      error: () => (this.loadingProducts = false),
    });
  }

  loadOrders(): void {
    this.loadingOrders = true;
    this.ordersService.getOrders().subscribe({
      next: (data) => {
        this.orders = data ?? [];
        this.orderStatusDraft = {};
        const statuses = Object.values(OrderStatus);
        for (const o of this.orders) {
          this.orderStatusDraft[o.id] = o.orderStatus || o.status || OrderStatus.Pending;
        }
        this.orderStatusOptions = statuses.map((s) => ({ label: this.translate.instant('ORDER_STATUS.' + s.toUpperCase()) || s, value: s }));
        this.loadingOrders = false;
      },
      error: () => (this.loadingOrders = false),
    });
  }

  openCreateProduct(): void {
    this.editingProductId = null;
    this.productDialogTitle = 'Add Product';
    this.productDraft = {
      name: '',
      description: '',
      pictureUrl: '',
      brand: '',
      category: '',
      price: 0,
      quantityInStock: 0,
    };
    this.productDialogVisible = true;
  }

  openEditProduct(p: Product): void {
    this.editingProductId = p.id;
    this.productDialogTitle = 'Edit Product';
    this.productDraft = { ...p };
    this.productDialogVisible = true;
  }

  saveProduct(): void {
    const payload = this.productDraft;
    if (!payload?.name) return;

    const isEdit = this.editingProductId !== null;
    const request$ = isEdit
      ? this.productsService.updateProduct(this.editingProductId as number, payload)
      : this.productsService.createProduct(payload);

    request$.subscribe({
      next: () => {
        this.productDialogVisible = false;
        this.loadProducts();
      },
      error: () => {
        // Let backend error show up in console; you can add p-message later if needed.
      },
    });
  }

  deleteProduct(productId: number): void {
    const ok = window.confirm('Delete this product?');
    if (!ok) return;
    this.productsService.deleteProduct(productId).subscribe({
      next: () => this.loadProducts(),
      error: () => {
        // backend error
      },
    });
  }

  updateOrderStatus(orderId: number): void {
    const status = this.orderStatusDraft[orderId];
    if (!status) return;

    this.ordersService.updateOrderStatus(orderId, status).subscribe({
      next: () => this.loadOrders(),
      error: () => {
        // backend error
      },
    });
  }
}
