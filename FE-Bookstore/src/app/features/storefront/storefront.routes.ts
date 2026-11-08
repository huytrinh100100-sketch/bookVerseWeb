import { Routes } from '@angular/router';
import { StorefrontLayoutComponent } from './storefront.layout';

export const STOREFRONT_ROUTES: Routes = [
  {
    path: '',
    component: StorefrontLayoutComponent,
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/storefront-home.page').then((m) => m.StorefrontHomePageComponent)
      },
      {
        path: 'catalog',
        loadComponent: () => import('./pages/storefront-catalog.page').then((m) => m.StorefrontCatalogPageComponent)
      },
      {
        path: 'books/:slug',
        loadComponent: () => import('./pages/storefront-book-details.page').then((m) => m.StorefrontBookDetailsPageComponent)
      },
      {
        path: 'cart',
        loadComponent: () => import('./pages/storefront-cart.page').then((m) => m.StorefrontCartPageComponent)
      },
      {
        path: 'checkout',
        loadComponent: () => import('./pages/storefront-checkout.page').then((m) => m.StorefrontCheckoutPageComponent)
      },
      {
        path: 'order-success/:code',
        loadComponent: () => import('./pages/storefront-order-success.page').then((m) => m.StorefrontOrderSuccessPageComponent)
      },
      {
        path: 'account',
        loadComponent: () => import('./pages/storefront-account.page').then((m) => m.StorefrontAccountPageComponent)
      },
      {
        path: 'wishlist',
        loadComponent: () => import('./pages/storefront-wishlist.page').then((m) => m.StorefrontWishlistPageComponent)
      },
      {
        path: 'addresses',
        loadComponent: () => import('./pages/storefront-addresses.page').then((m) => m.StorefrontAddressesPageComponent)
      },
      {
        path: 'rewards',
        loadComponent: () => import('./pages/storefront-rewards.page').then((m) => m.StorefrontRewardsPageComponent)
      },
      {
        path: 'settings',
        loadComponent: () => import('./pages/storefront-settings.page').then((m) => m.StorefrontSettingsPageComponent)
      },
      { path: '**', redirectTo: '' }
    ]
  }
];
