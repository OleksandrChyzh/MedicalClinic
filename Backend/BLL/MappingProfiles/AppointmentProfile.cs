using AutoMapper;
using BLL.Models.Appointment;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class AppointmentProfile : Profile
{
    public AppointmentProfile()
    {
        // 1. Створення запису (DTO -> Entity)
        this.CreateMap<AddAppointmentDTO, Appointment>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AppointmentStatus.CREATED))
            // Використовуємо Unspecified замість UtcNow
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified)));

        // 2. Отримання запису (Entity -> DTO)
        this.CreateMap<Appointment, GetAppointmentDTO>()
            // ПІБ пацієнта беремо прямо з сутності Patient
            .ForMember(dest => dest.PatientFullName,
                opt => opt.MapFrom(src => $"{src.Patient.FirstName} {src.Patient.LastName}"))

            // ПІБ лікаря беремо прямо з сутності Doctor (бо там є ці поля)
            .ForMember(dest => dest.DoctorFullName,
                opt => opt.MapFrom(src => $"{src.Doctor.FirstName} {src.Doctor.LastName}"))

            // Назва послуги
            .ForMember(dest => dest.ServiceName,
                opt => opt.MapFrom(src => src.Service.Name))

            // Статус Enum -> String
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
