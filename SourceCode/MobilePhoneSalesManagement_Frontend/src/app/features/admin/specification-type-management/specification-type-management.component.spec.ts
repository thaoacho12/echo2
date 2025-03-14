import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SpecificationTypeManagementComponent } from './specification-type-management.component';

describe('SpecificationTypeManagementComponent', () => {
  let component: SpecificationTypeManagementComponent;
  let fixture: ComponentFixture<SpecificationTypeManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SpecificationTypeManagementComponent]
    })
      .compileComponents();

    fixture = TestBed.createComponent(SpecificationTypeManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  xit('should create', () => {
    expect(component).toBeTruthy();
  });
});
