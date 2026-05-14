using AutoMapper;
using BLL.Models.Doctor;
using BLL.Models.User;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class DoctorProfile : Profile
{
    public DoctorProfile()
    {
        // 1. Створення лікаря (DTO -> Entity)
        this.CreateMap<AddDoctorDTO, Doctor>();

        // 2. Оновлення лікаря (DTO -> Entity)
        this.CreateMap<UpdateDoctorDTO, Doctor>();

        // 3. Отримання даних лікаря (Entity -> DTO)
        this.CreateMap<Doctor, GetDoctorDto>()
            // Формуємо повне ім'я. Перевіряємо, чи є по батькові (MiddleName)
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.MiddleName)
                    ? $"{src.LastName} {src.FirstName}"
                    : $"{src.LastName} {src.FirstName} {src.MiddleName}"))

            // Назва напрямку з пов'язаної сутності Direction
            .ForMember(dest => dest.DirectionName,
                opt => opt.MapFrom(src => src.Direction != null ? src.Direction.Name : string.Empty))

            // Кількість відгуків
            .ForMember(dest => dest.ReviewsCount,
                opt => opt.MapFrom(src => src.Reviews != null ? src.Reviews.Count : 0))

            // Середній рейтинг: перевіряємо, чи є відгуки, щоб не отримати помилку ділення на нуль
            // Припускаємо, що у сутності Review є поле Rating (або Value) типу int/double
            .ForMember(dest => dest.AverageRating,
                opt => opt.MapFrom(src => src.Reviews != null && src.Reviews.Any()
                    ? Math.Round(src.Reviews.Average(r => r.Rating), 1)
                    : 0.0))

            // Список ID відгуків
            .ForMember(dest => dest.ReviewIds,
                opt => opt.MapFrom(src => src.Reviews != null
                    ? src.Reviews.Select(r => r.Id).ToList()
                    : new List<int>()));

        // 4. Профіль «лікар + користувач» для автентифікованого лікаря
        // 4. Профіль «лікар + користувач»
        this.CreateMap<Doctor, DoctorProfileDto>()
            // Кажемо: для властивості Doctor візьми весь об'єкт Doctor (src) 
            // і замапь його за правилами GetDoctorDto (які описані вище)
            .ForMember(dest => dest.Doctor, opt => opt.MapFrom(src => src))

            // Кажемо: для властивості User візьми об'єкт User із сутності Doctor
            // і замапь його за правилами GetUser
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));
    }
}

// AutoMapper створює конвертер через reflection; тип має бути public, інакше не вдасться створити екз
