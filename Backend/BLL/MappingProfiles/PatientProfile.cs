using AutoMapper;
using BLL.Models.Patient;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class PatientProfile : Profile
{
    public PatientProfile()
    {
        // 1. Створення пацієнта (DTO -> Entity)
        this.CreateMap<AddPatientDTO, Patient>();


        // 3. Отримання даних пацієнта (Entity -> DTO)
        this.CreateMap<Patient, GetPatientDTO>()
            // Дані профілю беремо з прив'язаного акаунту IdentityUser
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
            .ForMember(dest => dest.PhoneNumber,
                opt => opt.MapFrom(src => src.User != null ? src.User.PhoneNumber : string.Empty))

            // Рахуємо статистику (з перевіркою на null, щоб уникнути помилок)
            .ForMember(dest => dest.AppointmentsCount,
                opt => opt.MapFrom(src => src.Appointments != null ? src.Appointments.Count : 0))
            .ForMember(dest => dest.RecordsCount,
                opt => opt.MapFrom(src => src.MedicalRecords != null ? src.MedicalRecords.Count : 0));

    }
}
