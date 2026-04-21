import { Route } from '@angular/router';
import { adminGuard }    from './app/guards/admin.guard';
import { authGuard }     from './app/guards/auth.guard';
import { guestGuard }    from './app/guards/guest.guard';
import { deliveryGuard } from './app/guards/delivery.guard';

import { luckyRewardsGuard } from './app/guards/lucky-rewards.guard';

import { superAdminGuard } from './app/guards/super-admin.guard';

export const routes: Route[] = [
  { path: '', pathMatch: 'full', loadComponent: () => import('./app/pages/home/home.component').then(m => m.HomeComponent) },
  { path: 'products', loadComponent: () => import('./app/pages/products/products.component').then(m => m.ProductsComponent) },
  { path: 'products/:id', loadComponent: () => import('./app/pages/product-details/product-details.component').then(m => m.ProductDetailsComponent) },
  { path: 'basket', loadComponent: () => import('./app/pages/basket/basket.component').then(m => m.BasketComponent) },
  { path: 'checkout', canActivate: [authGuard], loadComponent: () => import('./app/pages/checkout/checkout.component').then(m => m.CheckoutComponent) },
  { path: 'wishlist', canActivate: [authGuard], loadComponent: () => import('./app/pages/wishlist/wishlist.component').then(m => m.WishlistComponent) },
  { path: 'orders', canActivate: [authGuard], loadComponent: () => import('./app/pages/orders/orders.component').then(m => m.OrdersComponent) },
  { path: 'orders/:id/track', canActivate: [authGuard], loadComponent: () => import('./app/pages/order-tracking/order-tracking.component').then(m => m.OrderTrackingComponent) },
  { path: 'delivery-agent', canActivate: [deliveryGuard], loadComponent: () => import('./app/pages/delivery-agent/delivery-agent.component').then(m => m.DeliveryAgentComponent) },
  { path: 'admin', redirectTo: 'admin/reports', pathMatch: 'full' },
  { path: 'admin/:tab', canActivate: [adminGuard], loadComponent: () => import('./app/pages/admin-dashboard/admin-dashboard.component').then(m => m.AdminDashboardComponent) },
  { path: 'profile', canActivate: [authGuard], loadComponent: () => import('./app/pages/user-profile/user-profile.component').then(m => m.UserProfileComponent) },
  { path: 'account/forgot-password', canActivate: [guestGuard], loadComponent: () => import('./app/pages/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent) },
  { path: 'account/reset-password', canActivate: [guestGuard], loadComponent: () => import('./app/pages/reset-password/reset-password.component').then(m => m.ResetPasswordComponent) },
  { path: 'login', canActivate: [guestGuard], loadComponent: () => import('./app/pages/login/login.component').then(m => m.LoginComponent) },
  { path: 'register', canActivate: [guestGuard], loadComponent: () => import('./app/pages/register/register.component').then(m => m.RegisterComponent) },
  { path: 'pos', loadComponent: () => import('./app/pages/pos/pos.component').then(m => m.PosComponent) },
  { path: 'lucky-rewards', canActivate: [authGuard, luckyRewardsGuard], loadComponent: () => import('./app/pages/lucky-rewards/lucky.component').then(m => m.LuckyRewardsPageComponent) },
  { path: 'super-admin', canActivate: [superAdminGuard], loadComponent: () => import('./app/pages/super-admin-dashboard/super-admin-dashboard.component').then(m => m.SuperAdminDashboardComponent) },
  { path: '**', loadComponent: () => import('./app/pages/not-found/not-found.component').then(m => m.NotFoundComponent) }
];
