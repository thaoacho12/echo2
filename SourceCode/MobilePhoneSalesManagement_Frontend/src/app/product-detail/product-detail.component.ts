import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ProductService } from '../features/admin/product-management/services/product.service';
import { Product } from '../features/admin/product-management/models/product';
@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.css'


})

export class ProductDetailComponent implements OnInit {
  pagination: any;
  reviews: any;
  changePage(arg0: number) {
    throw new Error('Method not implemented.');
  }
  filterRequest: any;
  toggleWishList(arg0: any) {
    throw new Error('Method not implemented.');
  }
  addToCart(arg0: any) {
    throw new Error('Method not implemented.');
  }
  onSortChange($event: Event) {
    throw new Error('Method not implemented.');
  }
  onInternalMemoryFilterChange($event: Event) {
    throw new Error('Method not implemented.');
  }
  screenSizes: any;
  internalMemories: any;
  onPriceFilterChange($event: Event) {
    throw new Error('Method not implemented.');
  }
  brands: any;
  priceRanges: any;
  onBrandFilterChange($event: Event) {
    throw new Error('Method not implemented.');
  }
  product?: Product;

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    console.log(id);
    if (id) {
      this.productService.getProductById(id).subscribe((data: any) => {
        this.product = data.product;
        this.reviews = data.reviews;
        console.log(data)
      });
    }
  }
}
