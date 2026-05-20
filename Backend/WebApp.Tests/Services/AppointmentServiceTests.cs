using System.Linq.Expressions;
using AutoMapper;
using BLL.Models.Appointment;
using BLL.Services;
using DAL.Entities;
using DAL.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace WebApp.Tests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IAppointmentRepository> _mockAppointmentRepo;
    private readonly Mock<IScheduleRepository> _mockScheduleRepo;
    private readonly Mock<IPatientRepository> _mockPatientRepo;
    private readonly Mock<IServiceRepository> _mockServiceRepo;
    private readonly AppointmentService _sut;

    public AppointmentServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockAppointmentRepo = new Mock<IAppointmentRepository>();
        _mockScheduleRepo = new Mock<IScheduleRepository>();
        _mockPatientRepo = new Mock<IPatientRepository>();
        _mockServiceRepo = new Mock<IServiceRepository>();

        _mockUow.Setup(u => u.AppointmentRepository).Returns(_mockAppointmentRepo.Object);
        _mockUow.Setup(u => u.ScheduleRepository).Returns(_mockScheduleRepo.Object);
        _mockUow.Setup(u => u.PatientRepository).Returns(_mockPatientRepo.Object);
        _mockUow.Setup(u => u.ServiceRepository).Returns(_mockServiceRepo.Object);

        _sut = new AppointmentService(_mockUow.Object, _mockMapper.Object);
    }

    // =========================================================
    // TEST 1: Happy-path — вільний слот, запис успішно створено
    // =========================================================
    [Fact]
    public async Task CreateAppointmentAsync_WhenSlotIsFree_ReturnsCreatedDto()
    {
        // Arrange
        const int userId = 1;
        var appointmentDate = GetFutureMonday().AddHours(10); // Monday 10:00

        var dto = new AddAppointmentDTO
        {
            PatientId = 1,
            UserId = userId,
            DoctorId = 1,
            ServiceId = 1,
            AppointmentDate = appointmentDate,
            DurationMinutes = 30
        };

        // Пацієнт належить поточному юзеру
        _mockPatientRepo
            .Setup(r => r.GetByIdAsync(dto.PatientId))
            .ReturnsAsync(new Patient { Id = 1, UserId = userId, Gender = "Чоловіча" });

        // Послуга існує й НЕ є "лікуванням"
        _mockServiceRepo
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                It.IsAny<Expression<Func<Service, object>>[]>()))
            .ReturnsAsync(new Service
            {
                Id = 1,
                Name = "Консультація",
                ServiceType = new ServiceType { Name = "Консультація" }
            });

        // Лікар працює в понеділок 09:00–17:00
        _mockScheduleRepo
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Schedule, bool>>>(),
                It.IsAny<Expression<Func<Schedule, object>>[]>()))
            .ReturnsAsync(new Schedule
            {
                DoctorId = 1,
                WeekDay = "Понеділок",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            });

        var savedEntity = new Appointment
        {
            Id = 99,
            DoctorId = 1,
            PatientId = 1,
            UserId = userId,
            AppointmentDate = appointmentDate,
            DurationMinutes = 30,
            Status = AppointmentStatus.CREATED,
            Patient = new Patient { Gender = "Чоловіча" },
            User = new User(),
            Doctor = new Doctor(),
            Service = new Service()
        };

        // Перший виклик — перевірка накладки (null = вільно)
        // Другий виклик — GetAppointmentByIdInternal після збереження
        _mockAppointmentRepo
            .SetupSequence(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Appointment, bool>>>(),
                It.IsAny<Expression<Func<Appointment, object>>[]>()))
            .ReturnsAsync((Appointment?)null)
            .ReturnsAsync(savedEntity);

        _mockAppointmentRepo
            .Setup(r => r.AddAsync(It.IsAny<Appointment>()))
            .Returns(Task.CompletedTask);

        _mockMapper.Setup(m => m.Map<Appointment>(dto)).Returns(new Appointment());
        _mockMapper.Setup(m => m.Map<GetAppointmentDTO>(savedEntity))
            .Returns(new GetAppointmentDTO { Id = 99 });

        // Act
        var result = await _sut.CreateAppointmentAsync(userId, dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(99);
        _mockAppointmentRepo.Verify(r => r.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    // =================================================================
    // TEST 2: Гранична умова — слот зайнятий, накладка 10:00–10:30
    // =================================================================
    [Fact]
    public async Task CreateAppointmentAsync_WhenSlotIsOccupied_ThrowsException()
    {
        // Arrange
        const int userId = 1;
        var appointmentDate = GetFutureMonday().AddHours(10); // Monday 10:00–10:30

        var dto = new AddAppointmentDTO
        {
            PatientId = 1,
            UserId = userId,
            DoctorId = 1,
            ServiceId = 1,
            AppointmentDate = appointmentDate,
            DurationMinutes = 30
        };

        _mockPatientRepo
            .Setup(r => r.GetByIdAsync(dto.PatientId))
            .ReturnsAsync(new Patient { Id = 1, UserId = userId, Gender = "Чоловіча" });

        _mockServiceRepo
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Service, bool>>>(),
                It.IsAny<Expression<Func<Service, object>>[]>()))
            .ReturnsAsync(new Service
            {
                Id = 1,
                ServiceType = new ServiceType { Name = "Консультація" }
            });

        _mockScheduleRepo
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Schedule, bool>>>(),
                It.IsAny<Expression<Func<Schedule, object>>[]>()))
            .ReturnsAsync(new Schedule
            {
                DoctorId = 1,
                WeekDay = "Понеділок",
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            });

        // BOUNDARY: вже існує запис на 10:00–10:30 → алгоритм overlap виявляє конфлікт
        var conflictingAppointment = new Appointment
        {
            DoctorId = 1,
            AppointmentDate = appointmentDate, // той самий час
            DurationMinutes = 30,
            Status = AppointmentStatus.CONFIRMED
        };

        _mockAppointmentRepo
            .Setup(r => r.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Appointment, bool>>>(),
                It.IsAny<Expression<Func<Appointment, object>>[]>()))
            .ReturnsAsync(conflictingAppointment); // overlap знайдено!

        _mockMapper.Setup(m => m.Map<Appointment>(dto)).Returns(new Appointment());

        // Act
        var act = async () => await _sut.CreateAppointmentAsync(userId, dto);

        // Assert — очікуємо Exception з повідомленням про зайнятий час
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*зайнятий*");
    }

    // Повертає найближчий понеділок як мінімум за 14 днів (гарантовано в майбутньому)
    private static DateTime GetFutureMonday()
    {
        var date = DateTime.Today.AddDays(14);
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(daysUntilMonday);
    }
}
