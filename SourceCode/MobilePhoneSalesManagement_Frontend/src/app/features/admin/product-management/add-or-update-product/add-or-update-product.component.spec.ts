import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddOrUpdateProductComponent } from './add-or-update-product.component';

describe('AddOrUpdateProductComponent', () => {
  let component: AddOrUpdateProductComponent;
  let fixture: ComponentFixture<AddOrUpdateProductComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddOrUpdateProductComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(AddOrUpdateProductComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  xit('should create', () => {
    expect(component).toBeTruthy();
  });
});
