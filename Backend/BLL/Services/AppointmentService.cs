using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Appointment;
using DAL.Entities;
using DAL.Exceptions;
using DAL.Interfaces;

namespace BLL.Services;
public class AppointmentService(IUnitOfWork unitOfWork, IMapper mapper) : IAppointmentService
{
    public async Task<IEnumerable<GetAppointmentDTO>> GetAllAppointmentsAsync()
    {
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            includes: [a => a.Patient, a => a.User, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<IEnumerable<GetAppointmentDTO>>(appointments);
    }

    public async Task<IEnumerable<GetAppointmentDTO>> GetAppointmentsByUserIdAsync(int userId)
    {
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            filter: a => a.UserId == userId,
            includes: [a => a.Patient, a => a.User, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<IEnumerable<GetAppointmentDTO>>(appointments);
    }

    public async Task<IEnumerable<GetAppointmentDTO>> GetAppointmentsByDoctorIdAsync(int doctorId)
    {
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            filter: a => a.DoctorId == doctorId,
            includes: [a => a.Patient, a => a.User, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<IEnumerable<GetAppointmentDTO>>(appointments);
    }

    // 1. Метод для ЗВИЧАЙНОГО ЮЗЕРА
    public async Task<GetAppointmentDTO> CreateAppointmentAsync(int userId, AddAppointmentDTO dto)
    {
        // Перевірка власності: чи пацієнт належить юзеру
        var patient = await unitOfWork.PatientRepository.GetByIdAsync(dto.PatientId);
        if (patient == null || patient.UserId != userId)
        {
            throw new UnauthorizedAccessException("Ви не можете записувати цього пацієнта.");
        }

        // Перевірка існування послуги
        var service = await unitOfWork.ServiceRepository.GetFirstOrDefaultAsync(
            filter: s => s.Id == dto.ServiceId,
            includes: s => s.ServiceType);
        if (service == null)
        {
            throw new KeyNotFoundException("Послугу не знайдено.");
        }

        if (IsTreatmentServiceType(service.ServiceType.Name))
        {
            throw new InvalidOperationException("Запис на лікування може створити тільки лікар після консультації.");
        }

        // Перевірка розкладу
        await ValidateSlotAvailability(dto);

        var appointment = mapper.Map<Appointment>(dto);

        // ФІКС ПОМИЛКИ NPGSQL: 
        // Жорстко прибираємо часовий пояс (UTC), щоб БД прийняла дату в колонку "timestamp without time zone"
        appointment.AppointmentDate = DateTime.SpecifyKind(appointment.AppointmentDate, DateTimeKind.Unspecified);

        appointment.UserId = userId; // Прив'язуємо до того, хто створив запис
        appointment.Status = AppointmentStatus.CREATED;

        await unitOfWork.AppointmentRepository.AddAsync(appointment);

        return await GetAppointmentByIdInternal(appointment.Id);
    }

    private static bool IsTreatmentServiceType(string? serviceTypeName)
    {
        if (string.IsNullOrWhiteSpace(serviceTypeName))
        {
            return false;
        }

        var normalized = serviceTypeName.Trim().ToLowerInvariant();
        return normalized.Contains("лікуван") || normalized.Contains("лiкуван") || normalized.Contains("treatment");
    }

    public async Task ChangeStatusAsync(int appointmentId, AppointmentStatus status)
    {
        var appointment = await unitOfWork.AppointmentRepository.GetByIdAsync(appointmentId);
        if (appointment == null)
        {
            throw new KeyNotFoundException("Запис не знайдено.");
        }

        appointment.Status = status;
        await unitOfWork.AppointmentRepository.UpdateStatusAsync(appointment);
    }

    // Приватний метод для отримання повного DTO після створення
    private async Task<GetAppointmentDTO> GetAppointmentByIdInternal(int id)
    {
        var entity = await unitOfWork.AppointmentRepository.GetFirstOrDefaultAsync(
            filter: a => a.Id == id,
            includes: [a => a.Patient, a => a.User, a => a.Doctor, a => a.Service]
        );
        return mapper.Map<GetAppointmentDTO>(entity);
    }

    public async Task<GetAppointmentDTO> CreateAppointmentByDoctorAsync(int doctorUserId, AddAppointmentDTO dto)
    {
        // 1. Знаходимо самого лікаря по його UserId
        var doctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(d => d.UserId == doctorUserId);
        if (doctor == null)
        {
            throw new UnauthorizedAccessException("Ви не зареєстровані як лікар.");
        }

        var knownPatientAppointment = await unitOfWork.AppointmentRepository.GetFirstOrDefaultAsync(a =>
            a.DoctorId == doctor.Id &&
            a.PatientId == dto.PatientId &&
            a.UserId == dto.UserId);

        if (knownPatientAppointment == null)
        {
            throw new UnauthorizedAccessException("Ви можете створювати записи тільки для пацієнтів зі своїх наявних записів.");
        }

        // 2. Валідація часу та розкладу
        await ValidateSlotAvailability(dto);

        // 3. Мапінг та збереження
        var appointment = mapper.Map<Appointment>(dto);
        // UserId у записі — це ID власника акаунта пацієнта (беремо з DTO)
        appointment.UserId = dto.UserId;
        appointment.Status = AppointmentStatus.CONFIRMED; // Лікар створює — значить уже підтверджено

        await unitOfWork.AppointmentRepository.AddAsync(appointment);

        return await GetAppointmentByIdInternal(appointment.Id);
    }

    // Приватний метод для перевірки накладок (Overlapping)
    private async Task ValidateSlotAvailability(AddAppointmentDTO dto)
    {
        var start = dto.AppointmentDate;
        var end = start.AddMinutes(dto.DurationMinutes);

        if (start <= DateTime.Now)
        {
            throw new InvalidOperationException("Неможливо створити запис на дату або час, що вже минули.");
        }

        if (start > DateTime.Now.AddMonths(1))
        {
            throw new InvalidParameterException("Неможливо створити запис більш ніж на місяць вперед.");
        }

        // 1. Створюємо словник для перекладу днів тижня на українську
        var dayOfWeekDict = new Dictionary<DayOfWeek, string>
    {
        { DayOfWeek.Monday, "Понеділок" },
        { DayOfWeek.Tuesday, "Вівторок" },
        { DayOfWeek.Wednesday, "Середа" },
        { DayOfWeek.Thursday, "Четвер" },
        { DayOfWeek.Friday, "П'ятниця" },
        { DayOfWeek.Saturday, "Субота" },
        { DayOfWeek.Sunday, "Неділя" }
    };

        // 2. Отримуємо правильний український день тижня
        var day = dayOfWeekDict[start.DayOfWeek];

        // Перевірка робочого графіку (Schedule)
        var schedule = await unitOfWork.ScheduleRepository.GetFirstOrDefaultAsync(
            s => s.DoctorId == dto.DoctorId && s.WeekDay == day);

        if (schedule == null)
        {
            throw new Exception($"Лікар не працює в цей день ({day})."); // Додав вивід дня для зручності
        }

        var workStart = TimeOnly.FromDateTime(start);
        var workEnd = TimeOnly.FromDateTime(end);

        if (workStart < schedule.StartTime || workEnd > schedule.EndTime)
        {
            throw new Exception("Час виходить за межі робочої зміни лікаря.");
        }

        // ПЕРЕВІРКА НАКЛАДКИ: (StartA < EndB) AND (EndA > StartB)
        var overlap = await unitOfWork.AppointmentRepository.GetFirstOrDefaultAsync(a =>
            a.DoctorId == dto.DoctorId &&
            a.Status != AppointmentStatus.CANCELLED &&
            start < a.AppointmentDate.AddMinutes(a.DurationMinutes) &&
            end > a.AppointmentDate);

        if (overlap != null)
        {
            throw new Exception($"Цей час уже зайнятий: {overlap.AppointmentDate:HH:mm} - {overlap.AppointmentDate.AddMinutes(overlap.DurationMinutes):HH:mm}");
        }
    }

    public async Task<IEnumerable<GetAppointmentDTO>> GetDoctorAppointmentsByDateAsync(int doctorId, DateTime targetDate)
    {
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            // Відфільтровуємо по DoctorId та збігу самої дати (без урахування часу)
            filter: a => a.DoctorId == doctorId && a.AppointmentDate.Date == targetDate.Date,
            // Обов'язково підтягуємо пов'язані сутності для DTO
            includes: [a => a.Patient, a => a.User, a => a.Doctor, a => a.Service]
        );

        // Якщо потрібно, щоб записи йшли по порядку (від ранку до вечора), додаємо сортування:
        var sortedAppointments = appointments.OrderBy(a => a.AppointmentDate);

        return mapper.Map<IEnumerable<GetAppointmentDTO>>(sortedAppointments);
    }

    public async Task<AvailableSlotsResponseDTO> GetAvailableSlotsAsync(int doctorId, DateTime targetDate)
    {
        var response = new AvailableSlotsResponseDTO { DoctorId = doctorId, Date = targetDate.Date };

        // 1. Отримуємо день тижня в потрібному форматі (якщо в БД українська)
        var dayOfWeek = GetUkrainianDayOfWeek(targetDate);
        // Якщо ж в БД зберігається англійською, заміни на: var dayOfWeek = targetDate.DayOfWeek.ToString();

        // 2. Беремо розклад лікаря на цей день
        var schedule = await unitOfWork.ScheduleRepository.GetFirstOrDefaultAsync(
            s => s.DoctorId == doctorId && s.WeekDay == dayOfWeek);

        if (schedule == null)
        {
            return response; // Лікар не працює в цей день, повертаємо порожній список
        }

        // 3. Беремо всі АКТИВНІ записи лікаря на цю дату
        var appointments = await unitOfWork.AppointmentRepository.GetAllAsync(
            a => a.DoctorId == doctorId &&
                 a.AppointmentDate.Date == targetDate.Date &&
                 a.Status != AppointmentStatus.CANCELLED
        );

        // 4. Генеруємо слоти (з кроком у 30 хвилин)
        int slotDurationMinutes = 30;
        var currentTime = schedule.StartTime;

        // Перевіряємо поточний час (для фільтрації минулих слотів, якщо дата - сьогодні)
        // Зверни увагу: якщо твій сервер в іншому часовому поясі, краще використовувати DateTime.Now або передавати таймзону
        var now = DateTime.UtcNow.AddHours(2); // Приклад для Києва, адаптуй під свої потреби

        while (currentTime.AddMinutes(slotDurationMinutes) <= schedule.EndTime)
        {
            var slotStartDateTime = targetDate.Date.Add(currentTime.ToTimeSpan());
            var slotEndDateTime = slotStartDateTime.AddMinutes(slotDurationMinutes);

            // Перевірка 1: Чи слот не в минулому?
            bool isPast = slotStartDateTime <= now;

            // Перевірка 2: Чи є накладка з існуючими Appointments?
            bool isOverlapping = appointments.Any(a =>
            {
                var apptStart = a.AppointmentDate;
                var apptEnd = apptStart.AddMinutes(a.DurationMinutes);
                // Формула перетину: (StartA < EndB) AND (EndA > StartB)
                return apptStart < slotEndDateTime && apptEnd > slotStartDateTime;
            });

            // Якщо час ще не минув і немає накладок — додаємо слот
            if (!isPast && !isOverlapping)
            {
                response.FreeSlots.Add(new FreeSlotDTO
                {
                    StartTime = currentTime.ToString("HH:mm"),
                    EndTime = currentTime.AddMinutes(slotDurationMinutes).ToString("HH:mm")
                });
            }

            // Переходимо до наступного слота
            currentTime = currentTime.AddMinutes(slotDurationMinutes);
        }

        return response;
    }

    // Допоміжний метод для перекладу днів тижня (додай його вниз файлу)
    private string GetUkrainianDayOfWeek(DateTime date)
    {
        return date.DayOfWeek switch
        {
            DayOfWeek.Monday => "Понеділок",
            DayOfWeek.Tuesday => "Вівторок",
            DayOfWeek.Wednesday => "Середа",
            DayOfWeek.Thursday => "Четвер",
            DayOfWeek.Friday => "П'ятниця",
            DayOfWeek.Saturday => "Субота",
            DayOfWeek.Sunday => "Неділя",
            // Використовуємо nameof(date), щоб уникнути помилок у назві змінної
            _ => throw new ArgumentOutOfRangeException(nameof(date), $"Невідомий день тижня: {date.DayOfWeek}")
        };
    }
}
