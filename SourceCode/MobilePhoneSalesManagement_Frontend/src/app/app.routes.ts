import { Routes } from '@angular/router';
import { OrderManagementComponent } from './features/admin/order-management/order-management.component';
import { ProductManagementComponent } from './features/admin/product-management/product-management.component';
import { BrandManagementComponent } from './features/admin/brand-management/brand-management.component';
import { UserManagementComponent } from './features/admin/user-management/user-management.component';
import { AdminLayoutComponent } from './features/admin/admin-layout/admin-layout.component';
import { ClientLayoutComponent } from './features/client/client-layout/client-layout.component';
import { HomeComponent } from './features/client/home/home.component';
import { ProductListComponent } from './features/client/product/product-list/product-list.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ForgotPasswordComponent } from './features/auth/forgot-password/forgot-password.component';
import { VerifyEmailComponent } from './features/auth/verify-email/verify-email.component';
import { ResetPasswordComponent } from './features/auth/reset-password/reset-password.component';
import { AccountComponent } from './features/client/user/account/account.component';
import { authGuard } from './features/auth/guards/auth.guard';
import { AboutComponent } from './features/client/about/about.component';
import { ContactComponent } from './features/client/contact/contact.component';
import { BlogsComponent } from './features/client/blogs/blogs.component';
import { WishlistComponent } from './features/client/user/wishlist/wishlist.component';
import { CartComponent } from './features/client/cart/cart.component';
import { CheckoutComponent } from './features/client/checkout/checkout.component';
import { ProductDetailComponent } from './product-detail/product-detail.component';

export const routes: Routes = [
  // { path: 'admin', redirectTo: '/admin/user-management', pathMatch: 'full' },
  {
    path: 'admin',
    component: AdminLayoutComponent,
    // canActivate: [authGuard],
    data: { role: 'admin', title: 'Admin - MobliePhoneSale' },
    children: [
      { path: 'user-management', component: UserManagementComponent },
      { path: 'brand-management', component: BrandManagementComponent },
      { path: 'product-management', component: ProductManagementComponent },
      { path: 'order-management', component: OrderManagementComponent },
    ],
  },
  { path: '', redirectTo: '/home', pathMatch: 'full' },
  {
    path: '',
    component: ClientLayoutComponent,
    children: [
      { path: 'home', component: HomeComponent, data: { title: 'Trang chủ' } }, // Trang chủ của client
      { path: 'products', component: ProductListComponent, data: { title: 'Cửa hàng' } }, // Danh sách sản phẩm
      { path: 'about', component: AboutComponent, data: { title: 'Thông tin' } }, // Danh sách sản phẩm
      { path: 'blogs', component: BlogsComponent, data: { title: 'Bài viết' } }, // Danh sách sản phẩm
      { path: 'contact', component: ContactComponent, data: { title: 'Liên hệ, hỗ trợ' } }, // Danh sách sản phẩm
      { path: 'products/:id', component: ProductDetailComponent }, // Chi tiết sản phẩm
      { path: 'cart', component: CartComponent }, // Giỏ hàng
      { path: 'checkout', component: CheckoutComponent }, // Thanh toán

      // auth
      { path: 'login', component: LoginComponent, data: { title: 'Đăng nhập' } },
      { path: 'register', component: RegisterComponent, data: { title: 'Đăng ký' } },
      { path: 'forgot-password', component: ForgotPasswordComponent, data: { title: 'Quên mật khẩu' } },
      { path: 'reset-password', component: ResetPasswordComponent, data: { title: 'Khôi phục mật khẩu' } },
      // user
      { path: 'account', component: AccountComponent, data: { title: 'Tài khoản' } },
      { path: 'wishlist', component: WishlistComponent, data: { title: 'Sản phẩm yêu thích' } },
      { path: 'verify-email', component: VerifyEmailComponent },
    ]
  }
];
