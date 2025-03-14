import { Component, Renderer2 } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { ToastrModule } from 'ngx-toastr';
import { ToastService } from './core/services/toast-service/toast.service';
import { Title } from '@angular/platform-browser';
import { filter, map } from 'rxjs';
import { CartService } from './features/client/cart/service/cart.service';
import { AuthService } from './features/auth/services/auth.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  imports: [RouterOutlet],
  standalone: true,
  styleUrl: './app.component.css',
})
export class AppComponent {
  isClientLayout: boolean = false;
  isAdminLayout: boolean = false;
  isAuthenticated = false;
  title = 'MobilePhoneSalesManagement_Frontend';

  constructor(private renderer: Renderer2, private router: Router, private titleService: Title, private authService: AuthService) { }

  ngOnInit() {
    // check login
    this.authService.isAuthenticated.subscribe((status) => {
      this.isAuthenticated = status;
    });

    // Kiểm tra route hiện tại để xác định layout
    if (window.location.pathname.startsWith('/admin')) {
      this.isAdminLayout = true;
      this.isClientLayout = false;
      this.loadAdminScripts();
    } else {
      this.isClientLayout = true;
      this.isAdminLayout = false;
      this.loadClientScripts();
    }

    // set title
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        map(() => {
          const route = this.router.routerState.root;
          let title = '';
          let currentRoute = route;
          while (currentRoute.firstChild) {
            currentRoute = currentRoute.firstChild;
          }
          if (currentRoute.snapshot.data['title']) {
            title = currentRoute.snapshot.data['title'];
          }
          return title || 'Nhom 21 LT WEB';
        })
      )
      .subscribe((title: string) => {
        this.titleService.setTitle(title);
      });
  }
  loadClientScripts() {
    this.addStylesheet('/assets/css/bootstrap.min.css');
    this.addStylesheet('/assets/css/style.css');
    this.addStylesheet('/assets/css/owl.carousel.css');
    this.addStylesheet('/assets/css/owl.theme.default.css');
    this.addStylesheet('/assets/css/font-awesome.min.css');
    this.setFavicon('logo_icon.png');
    // this.addStylesheet(
    //   'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.7.2/css/all.min.css',
    //   'sha512-Evv84Mr4kqVGRNSgIGL/F/aIDqQb7xQ2vcrdIwxfjThSH8CSR7PBEakCr51Ck+w+/U6swU2Im1vVX0SVk9ABhg==',
    //   'anonymous',
    //   'no-referrer'
    // );


    this.addScript('/assets/js/jquery.min.js', () => {
      // Sau khi jQuery đã sẵn sàng, load Bootstrap và các tệp khác
      this.addScript('/assets/js/bootstrap.min.js', () => {
        this.addScript('/assets/js/menumaker.js');
        this.addScript('/assets/js/jquery.sticky.js');
        this.addScript('/assets/js/owl.carousel.min.js');
        this.addScript('/assets/js/multiple-carousel.js');
      });
    });
  }

  loadAdminScripts() {
    // Thêm các file CSS
    this.addStylesheet('/assets/vendor/bootstrap/css/bootstrap.min.css');
    this.addStylesheet('/assets/css/all.min.css');
    this.addStylesheet(
      '/assets/fonts/fontawesome-free-5.15.4-web/css/all.min.css'
    );
    this.addStylesheet('/assets/vendor/bootstrap/css/bootstrap.min.css');
    this.addStylesheet('/assets/css/css_admin/admin.css');

    // Thêm các file JS
    this.addScript('/assets/vendor/jquery/jquery.min.js');
    this.addScript('/assets/vendor/bootstrap/js/bootstrap.bundle.min.js');
    // this.addScript('node_modules/bootstrap/dist/js/bootstrap.bundle.min.js');
    this.addScript('/admin_assets/js/custome.js');
    this.addScript('/assets/js/menumaker.js');
    this.addScript('/assets/js/jquery.sticky.js');
    this.addScript('/assets/js/sticky-header.js');
    this.addScript('/assets/js/owl.carousel.min.js');
    this.addScript('/assets/js/multiple-carousel.js');
  }

  addStylesheet(href: string, integrity?: string, crossorigin?: string, referrerPolicy?: string): void {
    const link = this.renderer.createElement('link');

    this.renderer.setAttribute(link, 'rel', 'stylesheet');
    this.renderer.setAttribute(link, 'href', href);

    if (integrity) {
      this.renderer.setAttribute(link, 'integrity', integrity);
    }
    if (crossorigin) {
      this.renderer.setAttribute(link, 'crossorigin', crossorigin);
    }
    if (referrerPolicy) {
      this.renderer.setAttribute(link, 'referrerpolicy', referrerPolicy);
    }

    this.renderer.appendChild(document.head, link);
  }

  addScript(src: string, callback?: () => void) {
    const script = document.createElement('script');
    script.src = src;
    script.type = 'text/javascript';
    script.async = false;

    if (callback) {
      script.onload = callback;
    }

    document.body.appendChild(script);
  }
  setFavicon(image: string): void {
    // Tạo thẻ <link>
    const link: HTMLLinkElement = this.renderer.createElement('link');
    link.rel = 'icon';
    link.type = 'image/png';
    link.href = `assets/images/${image}`; // Đường dẫn tới file favicon

    // Thêm thẻ <link> vào <head>
    this.renderer.appendChild(document.head, link);
  }
}