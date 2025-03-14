import { TestBed } from '@angular/core/testing';

import { SpecificationTypesService } from './specification-types.service';

describe('SpecificationTypesService', () => {
  let service: SpecificationTypesService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SpecificationTypesService);
  });

  xit('should be created', () => {
    expect(service).toBeTruthy();
  });
});
