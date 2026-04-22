using AutoMapper;
using BLL.Models.Review;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class ReviewProfile : Profile
{
    public ReviewProfile()
    {
        // 1. Створення відгуку (DTO -> Entity)
        this.CreateMap<AddReviewDTO, Review>()
            .ForMember(static dest => dest.CreatedAt, static opt => opt.MapFrom(static _ => DateTime.UtcNow));

        // 2. Оновлення відгуку (DTO -> Entity)
        this.CreateMap<UpdateReviewDTO, Review>();

        // 3. Отримання відгуку (Entity -> DTO)
        this.CreateMap<Review, GetReviewDTO>()
            // Ім'я користувача, який залишив відгук
            .ForMember(static dest => dest.UserName,
                static opt => opt.MapFrom(static src => src.User != null ? src.User.UserName : "Анонім"))

            // Назва послуги
            .ForMember(static dest => dest.ServiceName,
                static opt => opt.MapFrom(static src => src.Service != null ? src.Service.Name : null))

            // Повне ім'я лікаря (якщо DoctorId не null)
            .ForMember(static dest => dest.DoctorName,
                static opt => opt.MapFrom(static src => src.Doctor != null
                    ? (string.IsNullOrWhiteSpace(src.Doctor.MiddleName)
                        ? $"{src.Doctor.LastName} {src.Doctor.FirstName}"
                        : $"{src.Doctor.LastName} {src.Doctor.FirstName} {src.Doctor.MiddleName}")
                    : null));
    }
}
