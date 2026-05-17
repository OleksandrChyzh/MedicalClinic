import pandas as pd
import os
import joblib
from sklearn.model_selection import train_test_split
from sklearn.feature_extraction.text import TfidfVectorizer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import classification_report, accuracy_score

def train_medical_ai():
    dataset_path = "triage_dataset.csv"
    
    # 1. Перевірка наявності датасету
    if not os.path.exists(dataset_path):
        print(f"❌ Помилка: Файл {dataset_path} не знайдено! Спочатку запусти генератор даних.")
        return

    print("📖 Завантаження датасету...")
    df = pd.read_csv(dataset_path)
    
    # Заповнимо пусті значення, якщо вони випадково з'явилися
    df['cleaned_text'] = df['cleaned_text'].fillna('')

    X = df['cleaned_text'] # Вхідні дані (очищений текст скарги)
    y_specialty = df['specialty'] # Цільова мітка 1: Лікар
    y_urgency = df['urgency'] # Цільова мітка 2: Терміновість

    print(f"📊 Загальна кількість записів для навчання: {len(df)}")

    # 2. Векторизація тексту за допомогою TF-IDF
    # Використовуємо n-grams (1, 2), щоб модель бачила не тільки окремі слова, а й стійкі словосполучення 
    # (наприклад, "висока температура", "пече в грудях")
    print("🔤 Векторизація тексту (TF-IDF)...")
    vectorizer = TfidfVectorizer(ngram_range=(1, 2), max_features=5000)
    X_tfidf = vectorizer.fit_transform(X)

    # ====================================================================
    # МОДЕЛЬ 1: МЕДИЧНЕ СОРТУВАННЯ (КЛАСИФІКАЦІЯ НАПРЯМКІВ ЛІКАРІВ)
    # ====================================================================
    print("\n--- 🧠 Навчання Моделі №1: Медичне сортування (Спеціальність) ---")
    
    # Розділяємо дані на навчальну (80%) та тестову (20%) вибірки
    X_train_sp, X_test_sp, y_train_sp, y_test_sp = train_test_split(
        X_tfidf, y_specialty, test_size=0.2, random_state=42, stratify=y_specialty
    )

    # Логістична регресія чудово працює з TF-IDF на великій кількості класів
    specialty_model = LogisticRegression(C=1.0, max_iter=1000, random_state=42)
    specialty_model.fit(X_train_sp, y_train_sp)

    # Валідація моделі сортування
    y_pred_sp = specialty_model.predict(X_test_sp)
    print(f"✅ Точність моделі сортування (Accuracy): {accuracy_score(y_test_sp, y_pred_sp):.4f}")
    print("\nДетальний звіт по класах (Спеціальності):")
    print(classification_report(y_test_sp, y_pred_sp))

    # ====================================================================
    # МОДЕЛЬ 2: ВИЗНАЧЕННЯ ТЕРМІНОВОСТІ (URGENCY DETECTION)
    # ====================================================================
    print("\n--- 🧠 Навчання Моделі №2: Визначення терміновості (Критичність) ---")
    
    X_train_ur, X_test_ur, y_train_ur, y_test_ur = train_test_split(
        X_tfidf, y_urgency, test_size=0.2, random_state=42, stratify=y_urgency
    )

    urgency_model = LogisticRegression(C=1.0, max_iter=1000, random_state=42)
    urgency_model.fit(X_train_ur, y_train_ur)

    # Валідація моделі терміновості
    y_pred_ur = urgency_model.predict(X_test_ur)
    print(f"✅ Точність моделі терміновості (Accuracy): {accuracy_score(y_test_ur, y_pred_ur):.4f}")
    print("\nДетальний звіт по класах (Терміновість):")
    print(classification_report(y_test_ur, y_pred_ur))

    # ====================================================================
    # ЕТАП 3: СЕРІАЛІЗАЦІЯ (ЗБЕРЕЖЕННЯ МОДЕЛЕЙ)
    # ====================================================================
    print("\n💾 Збереження навчених моделей та векторизатора у файли .pkl...")
    
    # Створюємо папку під артефакти моделі, якщо її немає
    models_dir = "models"
    os.makedirs(models_dir, exist_ok=True)

    # Дуже важливо зберегти ТОЙ САМИЙ векторизатор, на якому вчилися моделі!
    joblib.dump(vectorizer, os.path.join(models_dir, "tfidf_vectorizer.pkl"))
    joblib.dump(specialty_model, os.path.join(models_dir, "specialty_model.pkl"))
    joblib.dump(urgency_model, os.path.join(models_dir, "urgency_model.pkl"))

    print("🎉 Все успішно збережено в папку 'models/'!")
    print("   - models/tfidf_vectorizer.pkl")
    print("   - models/specialty_model.pkl")
    print("   - models/urgency_model.pkl")

if __name__ == "__main__":
    train_medical_ai()