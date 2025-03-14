import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddOrUpdateBrandComponent } from './add-or-update-brand.component';

describe('AddOrUpdateBrandComponent', () => {
  let component: AddOrUpdateBrandComponent;
  let fixture: ComponentFixture<AddOrUpdateBrandComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddOrUpdateBrandComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(AddOrUpdateBrandComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  xit('should create', () => {
    expect(component).toBeTruthy();
  });
});
