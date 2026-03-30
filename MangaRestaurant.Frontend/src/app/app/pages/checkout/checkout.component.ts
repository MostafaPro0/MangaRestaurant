import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { TranslateModule } from '@ngx-translate/core';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { BasketService } from '../../services/basket.service';
import { OrdersService } from '../../services/orders.service';
import { UserAddress } from '../../models/user-address.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, PasswordModule, ButtonModule, DropdownModule, ProgressSpinnerModule, TranslateModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  basket: any = { id: '', items: [] };
  address: UserAddress = { firstName: '', lastName: '', street: '', city: '', state: '', zipcode: '', country: '' };
  deliveryMethodId = 1;
  deliveryMethods: any[] = [];
  loading = false;

  constructor(private basketService: BasketService, private ordersService: OrdersService, private router: Router) {}

  ngOnInit(): void {
    this.basketService.basket$.subscribe((basket) => (this.basket = basket));
    this.ordersService.getDeliveryMethods().subscribe((methods) => (this.deliveryMethods = methods));
  }

  get subtotal(): number {
    return this.basket.items.reduce((sum: number, item: any) => sum + item.price * item.quantity, 0);
  }

  createOrder(): void {
    if (!this.basket.id || !this.address.street) return;
    this.loading = true;
    this.ordersService.createOrder({ basketId: this.basket.id, deliveryMethodId: this.deliveryMethodId, shippingAddress: this.address }).subscribe({
      next: () => {
        this.loading = false;
        this.basketService.deleteBasket(this.basket.id).subscribe();
        this.router.navigate(['/orders']);
      },
      error: () => (this.loading = false)
    });
  }
}
