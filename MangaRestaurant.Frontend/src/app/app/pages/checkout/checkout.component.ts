import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { CheckboxModule } from 'primeng/checkbox';
import { RadioButtonModule } from 'primeng/radiobutton';
import { BasketService } from '../../services/basket.service';
import { OrdersService } from '../../services/orders.service';
import { UserAddress } from '../../models/user-address.model';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, InputTextModule, PasswordModule, ButtonModule, DropdownModule, ProgressSpinnerModule, TranslateModule, CheckboxModule, RadioButtonModule],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css'
})
export class CheckoutComponent implements OnInit {
  basket: any = { id: '', items: [] };
  savedAddresses: any[] = [];
  selectedAddress: any = null;
  address: UserAddress = { firstName: '', lastName: '', street: '', city: '', state: '', zipCode: '', country: '' };
  deliveryMethodId = 1;
  deliveryMethods: any[] = [];
  loading = false;
  saveAddressToProfile = false;
  addressMode: 'saved' | 'new' = 'new';

  constructor(
    private basketService: BasketService,
    private ordersService: OrdersService,
    private authService: AuthService,
    private router: Router,
    private messageService: MessageService,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.basketService.basket$.subscribe((basket) => (this.basket = basket));
    this.ordersService.getDeliveryMethods().subscribe((methods) => (this.deliveryMethods = methods));
    this.loadSavedAddresses();
  }

  loadSavedAddresses(): void {
    this.authService.getUserAddresses().subscribe({
      next: (data) => {
        this.savedAddresses = data || [];
        if (this.savedAddresses.length > 0) {
            this.addressMode = 'saved';
        }
      }
    });
  }

  onAddressSelect(event: any): void {
    if (event.value) {
      this.address = { ...event.value };
    }
  }

  get subtotal(): number {
    return this.basket.items.reduce((sum: number, item: any) => sum + item.price * item.quantity, 0);
  }

  createOrder(): void {
    const currentBasket = this.basketService.getCurrentBasket();
    const addressValid = [this.address.firstName, this.address.lastName, this.address.street, this.address.city, this.address.state, this.address.zipCode, this.address.country]
      .every((value) => value && value.trim().length > 0);

    if (!currentBasket.id || currentBasket.items.length === 0 || !addressValid) {
      console.warn('Order not created: basket or address missing', {
        basketId: currentBasket.id,
        address: this.address,
        items: currentBasket.items.length,
        addressValid
      });
      alert('يرجى إضافة منتج واحد على الأقل وتعبئة جميع حقول العنوان الإجبارية (بما في ذلك الدولة)');
      return;
    }

    this.loading = true;

    // Save address to profile if requested and not already selected from saved addresses
    if (this.saveAddressToProfile && this.addressMode === 'new') {
      this.authService.addAddress(this.address).subscribe({
        next: () => console.log('Address saved to profile'),
        error: (err) => console.error('Failed to save address to profile', err)
      });
    }

    this.ordersService.createOrder({ basketId: currentBasket.id, deliveryMethodId: this.deliveryMethodId, shippingAddress: this.address }).subscribe({
      next: (order) => {
        this.loading = false;
        console.log('Order created successfully:', order);

        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('TOAST.SUCCESS') || 'Success',
          detail: this.translate.instant('TOAST.ORDER_SUCCESS') || 'Order has been placed successfully!',
          life: 4000
        });

        this.basketService.deleteBasket(currentBasket.id).subscribe({
          next: () => console.log('Basket deleted'),
          error: (err) => console.error('Basket delete error', err)
        });
        this.router.navigate(['/orders']);
      },
      error: (err) => {
        this.loading = false;
        console.error('Order create error', err);

        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('TOAST.ERROR') || 'Error',
          detail: this.translate.instant('TOAST.ORDER_FAIL') || 'Failed to create order. Please try again.',
          life: 4000
        });
      }
    });
  }
}
