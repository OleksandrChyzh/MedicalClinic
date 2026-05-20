import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;

  describe('when token exists', () => {
    beforeEach(() => {
      const mockAuthService = { getToken: vi.fn().mockReturnValue('mock-token') };

      TestBed.configureTestingModule({
        providers: [
          provideHttpClient(withInterceptors([authInterceptor])),
          provideHttpClientTesting(),
          { provide: AuthService, useValue: mockAuthService }
        ]
      });

      http     = TestBed.inject(HttpClient);
      httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpMock.verify());

    it('should add Authorization: Bearer <token> header to every request', () => {
      // Act — виконуємо довільний GET-запит
      http.get('/api/test').subscribe();

      // Перехоплюємо запит через HttpTestingController
      const req = httpMock.expectOne('/api/test');

      // Assert — заголовок Authorization присутній і містить правильний токен
      expect(req.request.headers.get('Authorization')).toBe('Bearer mock-token');

      req.flush({});
    });
  });

  describe('when token is absent', () => {
    beforeEach(() => {
      const mockAuthService = { getToken: vi.fn().mockReturnValue(null) };

      TestBed.configureTestingModule({
        providers: [
          provideHttpClient(withInterceptors([authInterceptor])),
          provideHttpClientTesting(),
          { provide: AuthService, useValue: mockAuthService }
        ]
      });

      http     = TestBed.inject(HttpClient);
      httpMock = TestBed.inject(HttpTestingController);
    });

    afterEach(() => httpMock.verify());

    it('should NOT add Authorization header when token is null', () => {
      http.get('/api/public').subscribe();

      const req = httpMock.expectOne('/api/public');

      // Assert — заголовку Authorization немає
      expect(req.request.headers.has('Authorization')).toBe(false);

      req.flush({});
    });
  });
});
