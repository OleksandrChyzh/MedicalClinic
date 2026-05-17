import os
import re
import joblib
from contextlib import asynccontextmanager
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

# Шляхи до збережених моделей
MODELS_DIR = "models"
VECTORIZER_PATH = os.path.join(MODELS_DIR, "tfidf_vectorizer.pkl")
SPECIALTY_MODEL_PATH = os.path.join(MODELS_DIR, "specialty_model.pkl")
URGENCY_MODEL_PATH = os.path.join(MODELS_DIR, "urgency_model.pkl")

vectorizer = None
specialty_model = None
urgency_model = None

# Словники для красивого виводу українською
SPECIALTIES_UA = {
    "therapist": "Терапевта",
    "cardiologist": "Кардіолога",
    "dermatologist": "Дерматолога",
    "neurologist": "Невролога",
    "surgeon": "Хірурга",
    "pediatrician": "Педіатра",
    "gastroenterologist": "Гастроентеролога",
    "traumatologist": "Травматолога",
    "otolaryngologist": "Отоларинголога (ЛОРа)",
    "ophthalmologist": "Офтальмолога",
    "endocrinologist": "Ендокринолога",
    "urologist": "Уролога"
}

URGENCIES_UA = {
    "low": "Низька (плановий огляд або консультація)",
    "medium": "Середня (потрібно звернутися найближчим часом)",
    "high": "Висока (негайно зверніться до лікаря або викличте швидку!)"
}

@asynccontextmanager
async def lifespan(app: FastAPI):
    global vectorizer, specialty_model, urgency_model
    try:
        vectorizer = joblib.load(VECTORIZER_PATH)
        specialty_model = joblib.load(SPECIALTY_MODEL_PATH)
        urgency_model = joblib.load(URGENCY_MODEL_PATH)
        print("🚀 [AI-Module] Усі ML-моделі завантажено. Сервер готовий!")
    except Exception as e:
        print(f"❌ Помилка завантаження моделей: {str(e)}")
        raise e
    yield

app = FastAPI(title="Медичний AI-Модуль Сортування", lifespan=lifespan)

def clean_text(text: str) -> str:
    text = text.lower()
    text = re.sub(r'[^а-яіїєґ0-9\s]', ' ', text)
    text = re.sub(r'\s+', ' ', text).strip()
    return text

class TriageRequest(BaseModel):
    symptom_text: str = Field(..., description="Текст скарги")

class TriageResponse(BaseModel):
    is_recognized: bool = Field(..., description="Чи змогла модель розпізнати проблему")
    predicted_specialty: str = Field(default="", description="Ключ напрямку")
    predicted_urgency: str = Field(default="", description="Ключ терміновості")
    confidence: float = Field(default=0.0, description="Впевненість моделі (0-1)")
    human_message: str = Field(..., description="Текст для виводу користувачу")

@app.post("/api/ai/analyze", response_model=TriageResponse)
def analyze_symptoms(request: TriageRequest):
    if not vectorizer:
        raise HTTPException(status_code=503, detail="Моделі не завантажені.")
    
    cleaned = clean_text(request.symptom_text)
    if not cleaned:
        return TriageResponse(
            is_recognized=False,
            human_message="Будь ласка, введіть коректний текст скарги українською мовою."
        )
    
    text_vector = vectorizer.transform([cleaned])
    
    # Перевірка 1: Якщо векторизатор не знайшов ЖОДНОГО знайомого слова (наприклад, "хочу чіпси")
    # text_vector.nnz повертає кількість ненульових елементів
    if text_vector.nnz == 0:
         return TriageResponse(
            is_recognized=False,
            human_message="Вибачте, але я не можу знайти медичних симптомів у вашому запиті. Будь ласка, опишіть вашу проблему детальніше або зверніться до реєстратури."
        )

    # Отримуємо ймовірності для всіх класів
    probabilities = specialty_model.predict_proba(text_vector)[0]
    max_confidence = float(max(probabilities))
    
    # Перевірка 2: Поріг впевненості. Якщо модель не впевнена хоча б на 25% (0.25)
    CONFIDENCE_THRESHOLD = 0.25
    if max_confidence < CONFIDENCE_THRESHOLD:
        return TriageResponse(
            is_recognized=False,
            confidence=round(max_confidence, 2),
            human_message="Ваш запит містить нестандартні симптоми або потребує лікаря, якого немає в нашій базі. Рекомендуємо зателефонувати до клініки для уточнення."
        )

    # Якщо все добре, формуємо відповідь
    predicted_specialty = specialty_model.classes_[probabilities.argmax()]
    predicted_urgency = urgency_model.predict(text_vector)[0]
    
    specialty_name = SPECIALTIES_UA.get(predicted_specialty, "Лікаря")
    urgency_desc = URGENCIES_UA.get(predicted_urgency, "Не визначено")
    
    final_message = f"Вам треба звернутися до {specialty_name}. Критичність: {urgency_desc}"

    return TriageResponse(
        is_recognized=True,
        predicted_specialty=predicted_specialty,
        predicted_urgency=predicted_urgency,
        confidence=round(max_confidence, 2),
        human_message=final_message
    )