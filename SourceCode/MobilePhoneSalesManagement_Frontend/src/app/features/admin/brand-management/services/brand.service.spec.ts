import { TestBed } from '@angular/core/testing';

import { BrandService } from './brand.service';

describe('BrandService', () => {
  let service: BrandService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(BrandService);
  });

  xit('should be created', () => {
    expect(service).toBeTruthy();
  });
});
