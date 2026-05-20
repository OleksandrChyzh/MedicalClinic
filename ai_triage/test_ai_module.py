"""
Тести для AI-мікросервісу Curaionyx.
Запуск: pytest test_ai_module.py -v
(виконувати з директорії ai_triage/, де знаходиться models/)
"""

import pytest
from fastapi.testclient import TestClient
from main import app, clean_text


# =============================================================
# TEST 6: Юніт-тести функції очищення тексту clean_text()
# =============================================================

class TestCleanText:

    def test_removes_special_characters(self):
        """Спецсимволи (!@#$%^&*) мають бути видалені."""
        result = clean_text("Болить голова!!! @#$%")
        assert "!" not in result
        assert "@" not in result
        assert "#" not in result
        assert "$" not in result

    def test_converts_to_lowercase(self):
        """Весь текст має стати нижнім регістром."""
        result = clean_text("БОЛИТЬ ГОЛОВА")
        assert result == "болить голова"

    def test_keeps_cyrillic_letters(self):
        """Кириличні символи мають зберігатися."""
        result = clean_text("болить голова")
        assert "болить" in result
        assert "голова" in result

    def test_removes_latin_letters(self):
        """Латинські літери мають бути видалені (модель навчена лише на кирилиці)."""
        result = clean_text("head pain abc")
        assert "a" not in result
        assert "b" not in result
        assert "c" not in result

    def test_strips_extra_whitespace(self):
        """Зайві пробіли мають бути скорочені до одного."""
        result = clean_text("болить   дуже   голова")
        assert "  " not in result  # немає подвійних пробілів

    def test_empty_string_returns_empty(self):
        """Порожній рядок на вході → порожній рядок на виході."""
        result = clean_text("")
        assert result == ""

    def test_only_special_chars_returns_empty(self):
        """Якщо весь текст — спецсимволи, результат — порожній рядок."""
        result = clean_text("!!! @@@ ###")
        assert result.strip() == ""


# =============================================================
# TEST 7: Інтеграційні тести ендпоінту POST /api/ai/analyze
# =============================================================

@pytest.fixture(scope="module")
def client():
    """
    Запускає FastAPI-додаток разом із lifespan (завантаження ML-моделей).
    scope="module" — моделі завантажуються один раз для всього модуля тестів.
    """
    with TestClient(app) as c:
        yield c


class TestAnalyzeEndpoint:

    def test_returns_http_200(self, client):
        """Ендпоінт має повертати HTTP 200 для будь-якого валідного запиту."""
        response = client.post(
            "/api/ai/analyze",
            json={"symptom_text": "болить голова"}
        )
        assert response.status_code == 200

    def test_response_has_is_recognized_field(self, client):
        """Відповідь обов'язково містить поле is_recognized."""
        response = client.post(
            "/api/ai/analyze",
            json={"symptom_text": "болить голова"}
        )
        data = response.json()
        assert "is_recognized" in data

    def test_response_has_human_message_field(self, client):
        """Відповідь обов'язково містить текстове повідомлення для користувача."""
        response = client.post(
            "/api/ai/analyze",
            json={"symptom_text": "болить голова"}
        )
        data = response.json()
        assert "human_message" in data
        assert isinstance(data["human_message"], str)
        assert len(data["human_message"]) > 0

    def test_recognized_response_has_specialty(self, client):
        """Якщо симптом розпізнано, поле predicted_specialty не порожнє."""
        response = client.post(
            "/api/ai/analyze",
            json={"symptom_text": "болить голова шум у вухах нудота запаморочення"}
        )
        data = response.json()
        if data["is_recognized"]:
            assert data["predicted_specialty"] != ""
            assert data["confidence"] > 0.0

    def test_empty_text_returns_not_recognized(self, client):
        """Порожній текст → is_recognized = False (без виключення)."""
        response = client.post(
            "/api/ai/analyze",
            json={"symptom_text": ""}
        )
        assert response.status_code == 200
        assert response.json()["is_recognized"] is False

    def test_non_medical_text_returns_not_recognized(self, client):
        """Текст лише латиницею → clean_text() повертає '', вектор порожній → is_recognized = False."""
        response = client.post(
            "/api/ai/analyze",
            json={"symptom_text": "I want pizza and coffee please"}
        )
        assert response.status_code == 200
        assert response.json()["is_recognized"] is False

    def test_confidence_is_between_0_and_1(self, client):
        """Поле confidence завжди має бути в діапазоні [0.0, 1.0]."""
        response = client.post(
            "/api/ai/analyze",
            json={"symptom_text": "болить серце задишка"}
        )
        data = response.json()
        assert 0.0 <= data["confidence"] <= 1.0
