import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AiTriageService, AiTriageResult } from './ai-triage.service';

describe('AiTriageService', () => {
  let service: AiTriageService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AiTriageService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service  = TestBed.inject(AiTriageService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify()); // перевіряє, що немає незавершених запитів

  // ================================================================
  // TEST 5: Успішне отримання AI-відповіді від бекенду
  // ================================================================
  it('should POST to /api/AiTriage/analyze and return mapped result', () => {
    // Arrange — очікувана відповідь від .NET API
    const mockResponse: AiTriageResult = {
      isRecognized:       true,
      predictedSpecialty: 'neurologist',
      predictedUrgency:   'medium',
      confidence:         0.87,
      humanMessage:       'Вам треба звернутися до Невролога. Критичність: Середня.'
    };

    let actualResult: AiTriageResult | undefined;

    // Act — викликаємо метод сервісу
    service.analyze({ symptomText: 'болить голова' }).subscribe(res => {
      actualResult = res;
    });

    // Перехоплюємо HTTP-запит
    const req = httpMock.expectOne('https://localhost:7272/api/AiTriage/analyze');

    // Assert — метод та тіло запиту коректні
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ symptomText: 'болить голова' });

    // Емулюємо відповідь сервера
    req.flush(mockResponse);

    // Assert — Observable повернув правильний результат
    expect(actualResult).toEqual(mockResponse);
    expect(actualResult?.isRecognized).toBe(true);
    expect(actualResult?.predictedSpecialty).toBe('neurologist');
    expect(actualResult?.humanMessage).toContain('Невролога');
  });

  // ================================================================
  // Сценарій: сервер повертає is_recognized = false
  // ================================================================
  it('should correctly handle unrecognized symptom response', () => {
    const unrecognizedResponse: AiTriageResult = {
      isRecognized:       false,
      predictedSpecialty: null,
      predictedUrgency:   null,
      confidence:         0.1,
      humanMessage:       'Не вдалося знайти медичних симптомів у запиті.'
    };

    let actualResult: AiTriageResult | undefined;

    service.analyze({ symptomText: 'хочу піцу' }).subscribe(res => {
      actualResult = res;
    });

    const req = httpMock.expectOne('https://localhost:7272/api/AiTriage/analyze');
    req.flush(unrecognizedResponse);

    expect(actualResult?.isRecognized).toBe(false);
    expect(actualResult?.predictedSpecialty).toBeNull();
  });
});
