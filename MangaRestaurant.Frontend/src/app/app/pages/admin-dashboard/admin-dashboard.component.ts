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
import { NgApexchartsModule } from "ng-apexcharts";
import {
  ChartComponent,
  ApexAxisChartSeries,
  ApexNonAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexDataLabels,
  ApexTitleSubtitle,
  ApexStroke,
  ApexGrid,
  ApexFill,
  ApexMarkers,
  ApexYAxis,
  ApexTheme,
  ApexLegend,
  ApexPlotOptions,
  ApexTooltip
} from "ng-apexcharts";

export type ChartOptions = {
  series: ApexAxisChartSeries | ApexNonAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  dataLabels: ApexDataLabels;
  grid: ApexGrid;
  stroke: ApexStroke;
  title: ApexTitleSubtitle;
  fill: ApexFill;
  markers: ApexMarkers;
  yaxis: ApexYAxis;
  theme: ApexTheme;
  legend: ApexLegend;
  plotOptions: ApexPlotOptions;
  tooltip: ApexTooltip;
  labels: string[];
  colors: string[];
};
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
    NgApexchartsModule
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

  // Chart Options
  public salesChartOptions!: Partial<ChartOptions>;
  public categoryChartOptions!: Partial<ChartOptions>;
  public statusChartOptions!: Partial<ChartOptions>;

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
    this.initCharts();
  }

  initCharts(): void {
    const isDark = localStorage.getItem('theme') === 'dark';
    const textColor = isDark ? '#ffffff' : '#333333';
    
    // Sales Trend Chart (Area)
    this.salesChartOptions = {
      series: [
        {
          name: "Sales",
          data: [450, 520, 380, 670, 480, 920, 850]
        }
      ],
      chart: {
        height: 350,
        type: "area",
        toolbar: { show: false },
        background: 'transparent',
        animations: { enabled: true }
      },
      colors: ["#ff4d4d"],
      dataLabels: { enabled: false },
      stroke: { curve: "smooth", width: 3 },
      fill: {
        type: "gradient",
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.7,
          opacityTo: 0.2,
          stops: [0, 90, 100]
        }
      },
      xaxis: {
        categories: ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
        labels: { style: { colors: textColor } }
      },
      yaxis: {
        labels: { style: { colors: textColor } }
      },
      grid: {
        borderColor: isDark ? "rgba(255,255,255,0.1)" : "rgba(0,0,0,0.1)"
      }
    };

    // Top Categories (Bar)
    this.categoryChartOptions = {
      series: [
        {
          name: "Orders",
          data: [44, 55, 41, 67, 22]
        }
      ],
      chart: {
        type: "bar",
        height: 350,
        toolbar: { show: false },
        background: 'transparent'
      },
      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "55%",
          borderRadius: 8
        }
      },
      colors: ["#f9d423"],
      xaxis: {
        categories: ["Ramen", "Sushi", "Drinks", "Desserts", "Bento"],
        labels: { style: { colors: textColor } }
      },
      yaxis: {
        labels: { style: { colors: textColor } }
      },
      grid: {
        borderColor: isDark ? "rgba(255,255,255,0.1)" : "rgba(0,0,0,0.1)"
      }
    };

    // Order Status Distribution (Donut)
    this.statusChartOptions = {
      series: [44, 55, 13, 33],
      chart: {
        type: "donut",
        height: 350,
        background: 'transparent'
      },
      labels: ["Pending", "Completed", "Cancelled", "Shipped"],
      colors: ["#f39c12", "#2ecc71", "#e74c3c", "#3498db"],
      legend: {
        position: "bottom",
        labels: { colors: textColor }
      },
      stroke: { show: false }
    };
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
        this.orderStatusOptions = statuses.map((s) => ({ label: s, value: s }));
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
