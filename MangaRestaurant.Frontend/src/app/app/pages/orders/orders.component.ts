import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BadgeModule } from 'primeng/badge';
import { TranslateModule } from '@ngx-translate/core';
import { OrdersService } from '../../services/orders.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, CardModule, TableModule, ProgressSpinnerModule, BadgeModule, TranslateModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit {
  orders: any[] = [];
  loading = false;

  constructor(private ordersService: OrdersService) {}

  statusSeverity(status: string): 'success' | 'info' | 'warn' | 'danger' | 'secondary' {
    const s = (status || '').toLowerCase();
    if (s.includes('deliv') || s.includes('done') || s.includes('completed')) return 'success';
    if (s.includes('cancel')) return 'danger';
    if (s.includes('prep') || s.includes('process') || s.includes('progress')) return 'info';
    if (s.includes('pend') || s.includes('waiting')) return 'warn';
    return 'secondary';
  }

  ngOnInit(): void {
    this.loading = true;
    this.ordersService.getOrders().subscribe({ next: (data) => { this.orders = data; this.loading = false; }, error: () => (this.loading = false) });
  }
}
