import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { NgApexchartsModule, ChartComponent, ApexAxisChartSeries, ApexChart, ApexXAxis, ApexTitleSubtitle, ApexStroke, ApexDataLabels, ApexFill, ApexGrid, ApexYAxis, ApexTooltip, ApexLegend, ApexPlotOptions, ApexResponsive } from "ng-apexcharts";
import { AdminService } from '../../services/admin.service';
import { ProductsService } from '../../services/products.service';
import { OrdersService } from '../../services/orders.service';
import { MessageService } from 'primeng/api';

export type ChartOptions = {
  series: ApexAxisChartSeries | any;
  chart: ApexChart;
  xaxis: ApexXAxis;
  title: ApexTitleSubtitle;
  stroke: ApexStroke;
  dataLabels: ApexDataLabels;
  fill: ApexFill;
  grid: ApexGrid;
  yaxis: ApexYAxis;
  colors: string[];
  tooltip: ApexTooltip;
  labels: string[];
  legend: ApexLegend;
  plotOptions: ApexPlotOptions;
  responsive: ApexResponsive[];
};

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    TableModule,
    CardModule,
    ButtonModule,
    DropdownModule,
    DialogModule,
    InputTextModule,
    TagModule,
    NgApexchartsModule
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css'],
  providers: [MessageService]
})
export class AdminDashboardComponent implements OnInit {
  activeTab: string = 'reports';
  reportData: any = null;
  orders: any[] = [];
  products: any[] = [];
  employees: any[] = [];
  loadingOrders: boolean = false;

  // Charts Options
  public salesChartOptions!: Partial<ChartOptions>;
  public statusChartOptions!: Partial<ChartOptions>;
  public peakHoursChartOptions!: Partial<ChartOptions>;
  public categoryChartOptions!: Partial<ChartOptions>;
  public topProductsChartOptions!: Partial<ChartOptions>;
  public deliveryChartOptions!: Partial<ChartOptions>;

  // Dropdown Options
  orderStatusOptions = [
    { label: 'Pending', value: 'Pending' },
    { label: 'Confirmed', value: 'Confirmed' },
    { label: 'Processing', value: 'Processing' },
    { label: 'Shipped', value: 'Shipped' },
    { label: 'Delivered', value: 'Delivered' },
    { label: 'Cancelled', value: 'Cancelled' }
  ];
  orderStatusDraft: { [key: number]: string } = {};

  // Dialog Control
  productDialogVisible: boolean = false;
  editingProductId: number | null = null;
  selectedProduct: any = {};

  constructor(
    private adminService: AdminService,
    private productsService: ProductsService,
    private ordersService: OrdersService,
    private messageService: MessageService,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.loadAllData();
    
    // Auto-refresh charts when language changes
    this.translate.onLangChange.subscribe(() => {
      if (this.reportData) {
        this.updateCharts(this.reportData);
      }
    });
  }

  loadAllData() {
    this.adminService.getAdminReport().subscribe(report => {
      this.reportData = report;
      this.updateCharts(report);
    });

    this.ordersService.getAllOrdersAdmin().subscribe(orders => {
      this.orders = orders;
      orders.forEach(o => this.orderStatusDraft[o.id] = o.status);
    });

    // ProductsService.getProducts expects categoryId or pageIndex (using 1 to avoid negative offset)
    this.productsService.getProducts(1).subscribe(p => this.products = p.data);

    this.adminService.getEmployees().subscribe(e => this.employees = e);
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    if (tab === 'reports' && this.reportData) {
      setTimeout(() => this.updateCharts(this.reportData), 100);
    }
  }

  getSummaryStats() {
    if (!this.reportData) return [];
    return [
      { label: 'ADMIN.TOTAL_ORDERS', value: this.reportData.totalOrders, icon: 'pi pi-shopping-bag', colorClass: 'sales' },
      { label: 'ADMIN.TOTAL_REVENUE', value: this.formatCurrency(this.reportData.revenue), icon: 'pi pi-money-bill', colorClass: 'revenue' },
      { label: 'ADMIN.AVG_ORDER_VALUE', value: this.formatCurrency(this.reportData.averageOrderValue), icon: 'pi pi-chart-bar', colorClass: 'avg' },
      { label: 'ADMIN.TOTAL_EMPLOYEES', value: this.employees.length, icon: 'pi pi-users', colorClass: 'users' }
    ];
  }

  formatCurrency(value: number) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  updateCharts(report: any) {
    const isAr = this.translate.currentLang === 'ar';

    // 1. Sales Trend
    this.salesChartOptions = {
      series: [{ name: this.translate.instant('ADMIN.TOTAL_REVENUE'), data: report.salesLast7Days.map((d: any) => d.revenue) }],
      chart: { type: 'area', height: 350, toolbar: { show: false } },
      xaxis: { categories: report.salesLast7Days.map((d: any) => d.date) },
      colors: ['#f9d423'],
      stroke: { curve: 'smooth', width: 3 },
      fill: { type: 'gradient', gradient: { opacityFrom: 0.6, opacityTo: 0.1 } }
    };

    // 2. Status Distribution
    this.statusChartOptions = {
      series: [report.pendingOrders, report.paymentReceivedOrders, report.paymentFailedOrders],
      labels: [
        this.translate.instant('ORDER_STATUS.PENDING'),
        this.translate.instant('ORDER_STATUS.PAYMENTRECEIVED'),
        this.translate.instant('ORDER_STATUS.PAYMENTFAILED')
      ],
      chart: { type: 'donut', height: 350 },
      colors: ['#f39c12', '#2ecc71', '#e74c3c'],
      legend: { position: 'bottom' }
    };

    // 3. Peak Hours
    this.peakHoursChartOptions = {
      series: [{ name: this.translate.instant('ORDERS.TITLE'), data: report.peakHours.map((h: any) => h.count) }],
      chart: { type: 'bar', height: 350, toolbar: { show: false } },
      xaxis: { categories: report.peakHours.map((h: any) => `${h.hour}:00`) },
      colors: ['#3498db'],
      plotOptions: { bar: { borderRadius: 4 } }
    };

    // 4. Categories
    this.categoryChartOptions = {
      series: report.topCategories.map((c: any) => c.count),
      labels: report.topCategories.map((c: any) => isAr ? (c.nameAr || c.name) : c.name),
      chart: { type: 'pie', height: 350 },
      colors: ['#8e44ad', '#2c3e50', '#d35400', '#16a085', '#2980b9']
    };

    // 5. Top Products
    this.topProductsChartOptions = {
      series: [{ name: this.translate.instant('BASKET.QUANTITY'), data: report.topProducts.map((p: any) => p.quantity) }],
      chart: { type: 'bar', height: 350, toolbar: { show: false } },
      xaxis: { categories: report.topProducts.map((p: any) => isAr ? (p.nameAr || p.name) : p.name) },
      colors: ['#e67e22'],
      plotOptions: { bar: { horizontal: true, borderRadius: 4 } }
    };

    // 6. Delivery
    this.deliveryChartOptions = {
      series: report.topDeliveryMethods.map((d: any) => d.count),
      labels: report.topDeliveryMethods.map((d: any) => isAr ? (d.nameAr || d.name) : d.name),
      chart: { type: 'donut', height: 300 },
      colors: ['#27ae60', '#f1c40f', '#e74c3c']
    };
  }

  // --- ACTIONS ---
  updateOrderStatus(orderId: number) {
    const status = this.orderStatusDraft[orderId];
    this.ordersService.updateOrderStatus(orderId, status).subscribe(() => {
      this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Order updated' });
      this.loadAllData();
    });
  }

  openCreateProduct() {
    this.editingProductId = null;
    this.selectedProduct = {};
    this.productDialogVisible = true;
  }

  openEditProduct(product: any) {
    this.editingProductId = product.id;
    this.selectedProduct = { ...product };
    this.productDialogVisible = true;
  }

  deleteProduct(id: number) {
    if (confirm(this.translate.instant('ADMIN.DELETE_CONFIRM'))) {
      this.productsService.deleteProduct(id).subscribe(() => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Product deleted' });
        this.loadAllData();
      });
    }
  }

  saveProduct() {
    // Logic to save/update product would go here
    this.productDialogVisible = false;
  }
}
