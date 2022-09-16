import { TestBed, inject } from '@angular/core/testing';
import {TokenInterceptor} from './token.interceptor';



describe('Http.InterceptorService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [TokenInterceptor]
    });
  });

  it('should be created', inject([TokenInterceptor], (service: TokenInterceptor) => {
    expect(service).toBeTruthy();
  }));
});
