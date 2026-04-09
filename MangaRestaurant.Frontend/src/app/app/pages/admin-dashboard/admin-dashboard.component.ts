import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TableModule } from 'primeng/table';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextarea } from 'primeng/inputtextarea';
import { TagModule } from 'primeng/tag';
import { SkeletonModule } from 'primeng/skeleton';
import { SelectButtonModule } from 'primeng/selectbutton';
import { CheckboxModule } from 'primeng/checkbox';
import { NgApexchartsModule, ChartComponent, ApexAxisChartSeries, ApexChart, ApexXAxis, ApexTitleSubtitle, ApexStroke, ApexDataLabels, ApexFill, ApexGrid, ApexYAxis, ApexTooltip, ApexLegend, ApexPlotOptions, ApexResponsive, ApexTheme } from "ng-apexcharts";
import { environment } from '../../../../environments/environment';
import { AdminService } from '../../services/admin.service';
import { ProductsService } from '../../services/products.service';
import { OrdersService } from '../../services/orders.service';
import { SettingsService } from '../../services/settings.service';
import { MessageService } from 'primeng/api';
import { SiteSettings } from '../../models/site-settings.model';

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
  theme: ApexTheme;
};

import { TooltipModule } from 'primeng/tooltip';

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
    InputTextarea,
    TagModule,
    SkeletonModule,
    NgApexchartsModule,
    TooltipModule,
    SelectButtonModule,
    CheckboxModule
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
  users: any[] = [];
  loadingOrders: boolean = false;
  loadingReports: boolean = false;
  loadingProducts: boolean = false;
  loadingUsers: boolean = false;
  userDialog: boolean = false;
  newUser: any = { displayName: '', email: '', password: '', phoneNumber: '' };
  selectedUserRole: string = 'User';
  roleOptions: any[] = [
    { label: 'Admin', value: 'Admin' },
    { label: 'Cashier', value: 'Cashier' },
    { label: 'Delivery', value: 'Delivery' },
    { label: 'Waiter', value: 'Waiter' },
    { label: 'User', value: 'User' }
  ];
  categories: any[] = [];
  brands: any[] = [];
  uploadingImage: boolean = false;
  
  // Category & Brand management
  categoriesList: any[] = [];
  brandsList: any[] = [];
  loadingCategories = false;
  loadingBrands = false;
  itemDialog = false;
  isCategoryMode = true; 
  editingItem: any = null;
  selectedItem: any = { name: '', nameAr: '', isHidden: false };

  // Settings
  siteSettings: SiteSettings = {} as SiteSettings;
  savingSettings: boolean = false;
  settings$: any;
  productDialogVisible: boolean = false;
  editingProductId: number | null = null;
  selectedProduct: any = {};

  // Charts Options
  public salesChartOptions!: Partial<ChartOptions>;
  public statusChartOptions!: Partial<ChartOptions>;
  public peakHoursChartOptions!: Partial<ChartOptions>;
  public categoryChartOptions!: Partial<ChartOptions>;
  public topProductsChartOptions!: Partial<ChartOptions>;
  public deliveryChartOptions!: Partial<ChartOptions>;

  get orderStatusOptions() {
    return [
      { label: this.translateService.instant('ORDER_STATUS.PENDING'), value: 'Pending' },
      { label: this.translateService.instant('ORDER_STATUS.PAYMENTRECEIVED'), value: 'PaymentReceived' },
      { label: this.translateService.instant('ORDER_STATUS.PAYMENTFAILED'), value: 'PaymentFailed' },
      { label: this.translateService.instant('ORDER_STATUS.CONFIRMED'), value: 'Confirmed' },
      { label: this.translateService.instant('ORDER_STATUS.PROCESSING'), value: 'Processing' },
      { label: this.translateService.instant('ORDER_STATUS.SHIPPED'), value: 'Shipped' },
      { label: this.translateService.instant('ORDER_STATUS.DELIVERED'), value: 'Delivered' },
      { label: this.translateService.instant('ORDER_STATUS.COMPLETED'), value: 'Completed' },
      { label: this.translateService.instant('ORDER_STATUS.CANCELLED'), value: 'Cancelled' },
      { label: this.translateService.instant('ORDER_STATUS.REFUNDED'), value: 'Refunded' }
    ];
  }
  
  orderStatusDraft: { [key: number]: string } = {};
  deliveryEmployees: any[] = [];
  orderDeliveryAssignee: { [key: number]: string } = {};

  selectedOrderType: string = 'All';
  
  get orderTypeOptions() {
    return [
      { label: this.translateService.currentLang === 'ar' ? 'الكل' : 'All', value: 'All' },
      { label: this.translateService.currentLang === 'ar' ? 'داخل المطعم' : 'Dine-In', value: 'DineIn' },
      { label: this.translateService.currentLang === 'ar' ? 'توصيل/سفري' : 'Delivery/TakeAway', value: 'Delivery' }
    ];
  }

  get filteredOrders() {
    if (this.selectedOrderType === 'All') return this.orders;
    
    return this.orders.filter(o => o.orderType === this.selectedOrderType);
  }

  constructor(
    private productsService: ProductsService,
    private ordersService: OrdersService,
    private adminService: AdminService,
    private settingsService: SettingsService,
    private messageService: MessageService,
    public translateService: TranslateService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.settings$ = this.settingsService.settings$;
    
    // Listen to route params for tab sync
    this.route.paramMap.subscribe(params => {
      const tab = params.get('tab');
      if (tab) {
        this.activeTab = tab;
        if (tab === 'reports' && this.reportData) {
            setTimeout(() => this.updateCharts(this.reportData), 100);
        }
      }
    });

    this.loadAllData();
    this.loadSettings();
    
    // Auto-refresh charts when language changes
    this.translateService.onLangChange.subscribe(() => {
      if (this.reportData) {
        this.updateCharts(this.reportData);
      }
    });
  }

  loadAllData() {
    this.loadingReports = true;
    this.adminService.getAdminReport().subscribe(report => {
      this.reportData = report;
      this.updateCharts(report);
      this.loadingReports = false;
    });

    this.loadingOrders = true;
    this.ordersService.getAllOrdersAdmin().subscribe(orders => {
      this.orders = orders;
      orders.forEach(o => {
          this.orderStatusDraft[o.id] = o.orderStatus || o.status || 'Pending';
          if (o.deliveryPersonId) {
              this.orderDeliveryAssignee[o.id] = o.deliveryPersonId;
          }
      });
      this.loadingOrders = false;
    });

    this.loadingProducts = true;
    this.productsService.getProducts(1, 10, '', null, null, true).subscribe(p => {
      this.products = p.data;
      this.loadingProducts = false;
    });

    this.loadUsers();
    this.loadCategories();
    this.loadBrands();
  }

  loadUsers() {
    this.loadingUsers = true;
    this.adminService.getUsers().subscribe(u => {
      this.users = u;
      this.deliveryEmployees = u.filter((user: any) => user.role?.toLowerCase() === 'delivery');
      this.loadingUsers = false;
    });
  }

  loadCategories() {
    this.loadingCategories = true;
    this.productsService.getCategories(true).subscribe({
      next: (res) => {
        this.categoriesList = res;
        this.loadingCategories = false;
      },
      error: () => (this.loadingCategories = false)
    });
  }

  loadBrands() {
    this.loadingBrands = true;
    this.productsService.getBrands(true).subscribe({
      next: (res) => {
        this.brandsList = res;
        this.loadingBrands = false;
      },
      error: () => (this.loadingBrands = false)
    });
  }

  openCreateItem(mode: 'categories' | 'brands') {
    this.isCategoryMode = mode === 'categories';
    this.editingItem = null;
    this.selectedItem = { name: '', nameAr: '', isHidden: false };
    this.itemDialog = true;
  }

  openEditItem(item: any, mode: 'categories' | 'brands') {
    this.isCategoryMode = mode === 'categories';
    this.editingItem = item;
    this.selectedItem = { ...item };
    this.itemDialog = true;
  }

  saveItem() {
    if (!this.selectedItem.name || !this.selectedItem.nameAr) {
      this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Please fill all required fields' });
      return;
    }

    if (this.isCategoryMode) {
      const obs = this.editingItem 
        ? this.productsService.updateCategory(this.editingItem.id, this.selectedItem)
        : this.productsService.createCategory(this.selectedItem);
      
      obs.subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Category saved' });
          this.itemDialog = false;
          this.loadCategories();
        }
      });
    } else {
      const obs = this.editingItem 
        ? this.productsService.updateBrand(this.editingItem.id, this.selectedItem)
        : this.productsService.createBrand(this.selectedItem);
      
      obs.subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Brand saved' });
          this.itemDialog = false;
          this.loadBrands();
        }
      });
    }
  }

  deleteItem(id: number, mode: 'categories' | 'brands') {
    if (confirm('Are you sure you want to delete this item?')) {
      const obs = mode === 'categories' 
        ? this.productsService.deleteCategory(id)
        : this.productsService.deleteBrand(id);
      
      obs.subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Deleted successfully' });
          mode === 'categories' ? this.loadCategories() : this.loadBrands();
        }
      });
    }
  }

  toggleItemVisibility(id: number, hide: boolean, mode: 'categories' | 'brands') {
    const obs = mode === 'categories'
      ? this.productsService.toggleCategoryVisibility(id, hide)
      : this.productsService.toggleBrandVisibility(id, hide);
    
    obs.subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: hide ? 'Hidden' : 'Shown' });
        mode === 'categories' ? this.loadCategories() : this.loadBrands();
      }
    });
  }

  saveUser() {
    this.adminService.createUser(this.newUser, this.selectedUserRole).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User created' });
        this.userDialog = false;
        this.loadUsers();
      },
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to create user' })
    });
  }

  onRoleChange(user: any, newRole: string) {
    this.adminService.updateUserRole(user.id, newRole).subscribe({
      next: () => this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Role updated' }),
      error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to update role' })
    });
  }

  onDeleteUser(userId: string) {
    if (confirm('Are you sure you want to delete this user?')) {
      this.adminService.deleteUser(userId).subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'User deleted' });
          this.loadAllData();
        },
        error: () => this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Failed to delete user' })
      });
    }
  }

  updateOrderDelivery(orderId: number) {
      const employeeId = this.orderDeliveryAssignee[orderId];
      if (!employeeId) return;

      this.ordersService.assignDelivery(orderId, employeeId).subscribe({
          next: () => {
              this.messageService.add({ 
                  severity: 'success', 
                  summary: 'Success', 
                  detail: this.translateService.currentLang === 'ar' ? 'تم تعيين عامل التوصيل بنجاح' : 'Delivery person assigned' 
              });
          },
          error: () => {
            this.messageService.add({ 
                severity: 'error', 
                summary: 'Error', 
                detail: this.translateService.currentLang === 'ar' ? 'فشل التعيين' : 'Assignment failed' 
            });
          }
      });
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
    if (tab === 'reports' && this.reportData) {
      setTimeout(() => this.updateCharts(this.reportData), 100);
    }
  }

  getSummaryStats() {
    if (!this.reportData) return [];
    const settings = this.siteSettings;
    return [
      { label: 'ADMIN.TOTAL_ORDERS', value: this.reportData.totalOrders, icon: 'pi pi-shopping-bag', colorClass: 'sales' },
      { label: 'ADMIN.TOTAL_REVENUE', value: this.formatCurrency(this.reportData.revenue, settings), icon: 'pi pi-money-bill', colorClass: 'revenue' },
      { label: 'ADMIN.AVG_ORDER_VALUE', value: this.formatCurrency(this.reportData.averageOrderValue, settings), icon: 'pi pi-chart-bar', colorClass: 'avg' },
      { label: 'ADMIN.TOTAL_USERS', value: this.users.length, icon: 'pi pi-users', colorClass: 'users' }
    ];
  }

  formatCurrency(value: number, settings?: SiteSettings) {
    const s = settings || this.siteSettings;
    const isAr = this.translateService.currentLang === 'ar';
    if (s && s.currencyCode) {
        if (isAr) {
            const formattedNum = new Intl.NumberFormat('en-US', { 
                minimumFractionDigits: 2, 
                maximumFractionDigits: 2 
            }).format(value);
            return `${formattedNum} ${s.currencySymbol || 'ج.م'}`;
        }
        return new Intl.NumberFormat('en-US', { 
            style: 'currency', 
            currency: s.currencyCode 
        }).format(value);
    }
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
  }

  updateCharts(report: any) {
    const isAr = this.translateService.currentLang === 'ar';
    const isDark = document.body.classList.contains('theme-dark');
    const themeMode = isDark ? 'dark' : 'light';
    const chartBaseConfig = { background: 'transparent', toolbar: { show: false } };

    // 1. Sales Trend
    this.salesChartOptions = {
      series: [{ name: this.translateService.instant('ADMIN.TOTAL_REVENUE'), data: report.salesLast7Days.map((d: any) => d.revenue) }],
      chart: { type: 'area', height: 350, ...chartBaseConfig },
      theme: { mode: themeMode as 'light' | 'dark' },
      xaxis: { categories: report.salesLast7Days.map((d: any) => d.date) },
      yaxis: {
        labels: {
          formatter: (val: number) => this.formatCurrency(val)
        }
      },
      tooltip: {
        y: {
          formatter: (val: number) => this.formatCurrency(val)
        }
      },
      colors: ['#ff3e3e'],
      stroke: { curve: 'smooth', width: 3 },
      fill: { type: 'gradient', gradient: { opacityFrom: 0.6, opacityTo: 0.1 } }
    };

    // 2. Status Distribution
    this.statusChartOptions = {
      series: [report.pendingOrders, report.paymentReceivedOrders, report.paymentFailedOrders],
      labels: [
        this.translateService.instant('ORDER_STATUS.PENDING'),
        this.translateService.instant('ORDER_STATUS.PAYMENTRECEIVED'),
        this.translateService.instant('ORDER_STATUS.PAYMENTFAILED')
      ],
      chart: { type: 'donut', height: 350, background: 'transparent' },
      theme: { mode: themeMode as 'light' | 'dark' },
      colors: ['#f39c12', '#2ecc71', '#e74c3c'],
      legend: { position: 'bottom' }
    };

    // 3. Peak Hours
    this.peakHoursChartOptions = {
      series: [{ name: this.translateService.instant('ORDERS.TITLE'), data: report.peakHours.map((h: any) => h.count) }],
      chart: { type: 'bar', height: 350, ...chartBaseConfig },
      theme: { mode: themeMode as 'light' | 'dark' },
      xaxis: { categories: report.peakHours.map((h: any) => `${h.hour}:00`) },
      colors: ['#e6b980'],
      plotOptions: { bar: { borderRadius: 4 } }
    };

    // 4. Categories
    this.categoryChartOptions = {
      series: report.topCategories.map((c: any) => c.count),
      labels: report.topCategories.map((c: any) => isAr ? (c.nameAr || c.name) : c.name),
      chart: { type: 'pie', height: 350, background: 'transparent' },
      theme: { mode: themeMode as 'light' | 'dark' },
      colors: ['#ff3e3e', '#e6b980', '#2c3e50', '#8e44ad', '#2980b9']
    };

    // 5. Top Products
    this.topProductsChartOptions = {
      series: [{ name: this.translateService.instant('BASKET.QUANTITY'), data: report.topProducts.map((p: any) => p.quantity) }],
      chart: { type: 'bar', height: 350, ...chartBaseConfig },
      theme: { mode: themeMode as 'light' | 'dark' },
      xaxis: { categories: report.topProducts.map((p: any) => isAr ? (p.nameAr || p.name) : p.name) },
      colors: ['#ff4d4d'],
      plotOptions: { bar: { horizontal: true, borderRadius: 4 } }
    };

    // 6. Delivery
    this.deliveryChartOptions = {
      series: report.topDeliveryMethods.map((d: any) => d.count),
      labels: report.topDeliveryMethods.map((d: any) => isAr ? (d.nameAr || d.name) : d.name),
      chart: { type: 'donut', height: 300, background: 'transparent' },
      theme: { mode: themeMode as 'light' | 'dark' },
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
    this.selectedProduct = { 
      name: '', nameAr: '', 
      description: '', descriptionAr: '', 
      price: 0, oldPrice: 0,
      pictureUrl: '',
      categoryId: this.categories[0]?.id || 1,
      brandId: this.brands[0]?.id || 1
    };
    this.productDialogVisible = true;
  }

  openEditProduct(product: any) {
    this.editingProductId = product.id;
    this.selectedProduct = { ...product };
    this.productDialogVisible = true;
  }

  deleteProduct(id: number) {
    if (confirm(this.translateService.instant('ADMIN.DELETE_CONFIRM'))) {
      this.productsService.deleteProduct(id).subscribe(() => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Product deleted' });
        this.loadAllData();
      });
    }
  }

  saveProduct() {
    // Validation
    if (!this.selectedProduct.name || !this.selectedProduct.nameAr || 
        !this.selectedProduct.price || !this.selectedProduct.categoryId || 
        !this.selectedProduct.brandId || !this.selectedProduct.description || 
        !this.selectedProduct.descriptionAr || !this.selectedProduct.pictureUrl) {
      this.messageService.add({ 
        severity: 'error', 
        summary: this.translateService.instant('TOAST.ERROR'), 
        detail: this.translateService.currentLang === 'ar' ? 'يرجى ملء جميع الحقول المطلوبة' : 'Please fill all required fields' 
      });
      return;
    }

    const operation = this.editingProductId 
      ? this.productsService.updateProduct(this.editingProductId, this.selectedProduct)
      : this.productsService.createProduct(this.selectedProduct);

    operation.subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Product saved' });
        this.productDialogVisible = false;
        this.loadAllData();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Save failed' });
      }
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.uploadingImage = true;
      this.adminService.uploadProductImage(file).subscribe({
        next: (path: string) => {
          this.selectedProduct.pictureUrl = path;
          this.uploadingImage = false;
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Image uploaded successfully' });
        },
        error: () => {
          this.uploadingImage = false;
          this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Image upload failed' });
        }
      });
    }
  }

  getImageUrl(path: string): string {
    if (!path) return 'assets/images/products/placeholder.jpg';
    if (path.startsWith('http')) return path;
    const baseUrl = environment.apiUrl.replace('/api', '');
    return `${baseUrl}/${path}`;
  }

  loadSettings() {
    this.settingsService.settings$.subscribe(settings => {
      if (settings) {
        this.siteSettings = { ...settings };
      }
    });
  }

  saveSettings() {
    this.savingSettings = true;
    this.settingsService.updateSettings(this.siteSettings).subscribe({
      next: () => {
        this.messageService.add({ 
            severity: 'success', 
            summary: this.translateService.currentLang === 'ar' ? 'نجاح' : 'Success', 
            detail: this.translateService.currentLang === 'ar' ? 'تم تحديث الإعدادات' : 'Settings updated successfully' 
        });
        this.savingSettings = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'Error', detail: 'Update failed' });
        this.savingSettings = false;
      }
    });
  }
}
