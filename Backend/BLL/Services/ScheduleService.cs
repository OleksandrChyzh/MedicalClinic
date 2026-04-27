using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Schedule;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class ScheduleService(IUnitOfWork unitOfWork, IMapper mapper) : IScheduleService
{
    public async Task<IEnumerable<GetScheduleDTO>> GetDoctorScheduleAsync(int doctorId)
    {
        var schedules = await unitOfWork.ScheduleRepository.GetAllAsync(
            filter: s => s.DoctorId == doctorId
        );
        return mapper.Map<IEnumerable<GetScheduleDTO>>(schedules);
    }

    public async Task<GetScheduleDTO> GetScheduleByIdAsync(int id)
    {
        var schedule = await unitOfWork.ScheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Розклад не знайдено.");
        }

        return mapper.Map<GetScheduleDTO>(schedule);
    }

    public async Task<GetScheduleDTO> CreateScheduleAsync(AddScheduleDTO dto)
    {
        // 1. Перевірка чи існує лікар
        var doctor = await unitOfWork.DoctorRepository.GetByIdAsync(dto.DoctorId);
        if (doctor == null)
        {
            throw new KeyNotFoundException("Лікаря не знайдено.");
        }

        // 2. Валідація часу
        var startTime = TimeOnly.Parse(dto.StartTime);
        var endTime = TimeOnly.Parse(dto.EndTime);
        if (startTime >= endTime)
        {
            throw new ArgumentException("Час початку має бути раніше за час завершення.");
        }

        // 3. Перевірка на дублікат дня (щоб не було двох понеділків для одного лікаря)
        var existingSchedule = await unitOfWork.ScheduleRepository.GetFirstOrDefaultAsync(
            s => s.DoctorId == dto.DoctorId && s.WeekDay == dto.WeekDay);

        if (existingSchedule != null)
        {
            throw new InvalidOperationException($"Розклад на {dto.WeekDay} для цього лікаря вже існує.");
        }

        // 4. Створення
        var schedule = mapper.Map<Schedule>(dto);
        await unitOfWork.ScheduleRepository.AddAsync(schedule);

        return mapper.Map<GetScheduleDTO>(schedule);
    }

    public async Task UpdateScheduleAsync(UpdateScheduleDTO dto)
    {
        var schedule = await unitOfWork.ScheduleRepository.GetByIdAsync(dto.Id);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Розклад не знайдено.");
        }

        // Валідація часу
        var startTime = TimeOnly.Parse(dto.StartTime);
        var endTime = TimeOnly.Parse(dto.EndTime);
        if (startTime >= endTime)
        {
            throw new ArgumentException("Час початку має бути раніше за час завершення.");
        }

        // Перевірка на дублікат дня (якщо день змінили, перевіряємо чи не зайнятий новий день)
        if (schedule.WeekDay != dto.WeekDay)
        {
            var existingSchedule = await unitOfWork.ScheduleRepository.GetFirstOrDefaultAsync(
                s => s.DoctorId == schedule.DoctorId && s.WeekDay == dto.WeekDay);

            if (existingSchedule != null)
            {
                throw new InvalidOperationException($"Розклад на {dto.WeekDay} для цього лікаря вже існує.");
            }
        }

        mapper.Map(dto, schedule);
        await unitOfWork.ScheduleRepository.UpdateAsync(schedule);
    }

    public async Task DeleteScheduleAsync(int id)
    {
        var schedule = await unitOfWork.ScheduleRepository.GetByIdAsync(id);
        if (schedule == null)
        {
            throw new KeyNotFoundException("Розклад не знайдено.");
        }

        await unitOfWork.ScheduleRepository.DeleteAsync(schedule);
    }
}
