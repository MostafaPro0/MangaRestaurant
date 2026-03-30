import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TableModule } from 'primeng/table';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { BasketService } from '../../services/basket.service';

@Component({
  selector: 'app-basket',
  standalone: true,
  imports: [CommonModule, ButtonModule, CardModule, TableModule, RouterLink, TranslateModule],
  templateUrl: './basket.component.html',
  styleUrl: './basket.component.css'
})
export class BasketComponent implements OnInit {
  basket: any = { id: '', items: [] };

  constructor(private basketService: BasketService) {}

  ngOnInit(): void {
    this.basketService.basket$.subscribe((basket) => (this.basket = basket));
  }

  removeItem(productId: number) {
    this.basketService.removeItem(productId);
  }

  get total(): number {
    return this.basket.items.reduce((sum: number, item: any) => sum + item.price * item.quantity, 0);
  }
}
