using AutoMapper;
using BLL.Models.MedicalCard;
using BLL.Models.MedicalRecord;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class MedicalRecordProfile : Profile
{
    public MedicalRecordProfile()
    {
        // 1. Створення медичного запису (DTO -> Entity)
        this.CreateMap<AddMedicalRecordDTO, MedicalRecord>()
            // Гарантуємо, що час створення встановлюється коректно при мапінгу
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)));

        // 2. Отримання медичного запису (Entity -> DTO)
        this.CreateMap<MedicalRecord, GetMedicalRecordDTO>()
            // Збираємо ПІБ пацієнта
            .ForMember(dest => dest.PatientFullName,
                opt => opt.MapFrom(src => $"{src.Patient.FirstName} {src.Patient.LastName}"))

            // Збираємо ПІБ лікаря (використовуємо поля з самої сутності Doctor, як ми визначили раніше)
            .ForMember(dest => dest.DoctorFullName,
                opt => opt.MapFrom(src => $"{src.Doctor.FirstName} {src.Doctor.LastName}"))

            // Назва послуги. AutoMapper безпечно обробить ситуацію, якщо src.Service == null
            .ForMember(dest => dest.ServiceName,
                opt => opt.MapFrom(src => src.Service != null ? src.Service.Name : null));

        // 3. Формування медичної карти (Patient -> MedicalCardDTO)
        this.CreateMap<Patient, MedicalCardDTO>()
            // Формуємо ПІБ для шапки карти
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))

            // Мапимо поле з різними назвами (BirthDate -> DateOfBirth)
            .ForMember(dest => dest.DateOfBirth,
                opt => opt.MapFrom(src => src.BirthDate))

            // Мапимо колекцію записів
            // AutoMapper автоматично застосує CreateMap<MedicalRecord, GetMedicalRecordDTO> для кожного елемента!
            .ForMember(dest => dest.Records,
                opt => opt.MapFrom(src => src.MedicalRecords));
    }
}
