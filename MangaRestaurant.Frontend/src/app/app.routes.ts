import { Route } from '@angular/router';
import { adminGuard } from './app/guards/admin.guard';
import { authGuard } from './app/guards/auth.guard';
import { guestGuard } from './app/guards/guest.guard';

export const routes: Route[] = [
  { path: '', pathMatch: 'full', loadComponent: () => import('./app/pages/home/home.component').then(m => m.HomeComponent) },
  { path: 'products', loadComponent: () => import('./app/pages/products/products.component').then(m => m.ProductsComponent) },
  { path: 'products/:id', loadComponent: () => import('./app/pages/product-details/product-details.component').then(m => m.ProductDetailsComponent) },
  { path: 'basket', loadComponent: () => import('./app/pages/basket/basket.component').then(m => m.BasketComponent) },
  { path: 'checkout', canActivate: [authGuard], loadComponent: () => import('./app/pages/checkout/checkout.component').then(m => m.CheckoutComponent) },
  { path: 'orders', canActivate: [authGuard], loadComponent: () => import('./app/pages/orders/orders.component').then(m => m.OrdersComponent) },
  { path: 'admin', redirectTo: 'admin/reports', pathMatch: 'full' },
  { path: 'admin/:tab', canActivate: [adminGuard], loadComponent: () => import('./app/pages/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./app/pages/user-profile/user-profile.component').then(m => m.UserProfileComponent) },
  { path: 'account/forgot-password', canActivate: [guestGuard], loadComponent: () => import('./app/pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
  { path: 'account/reset-password', canActivate: [guestGuard], loadComponent: () => import('./app/pages/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) },
  { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./app/pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', canActivate: [guestGuard], loadComponent: () => import('./app/pages/register/register.component').then(m => m.RegisterComponent) },
  { path: '**', loadComponent: () => import('./app/pages/not-found/not-found.component').then(m => m.NotFoundComponent) }
];
