using AutoMapper;
using BLL.Models.Service;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class ServiceProfile : Profile
{
    public ServiceProfile()
    {
        // 1. Створення послуги (DTO -> Entity)
        this.CreateMap<AddServiceDTO, Service>();

        // 2. Оновлення послуги (DTO -> Entity)
        this.CreateMap<UpdateServiceDTO, Service>();

        // 3. Отримання послуги (Entity -> DTO) з кастомною логікою
        this.CreateMap<Service, GetServiceDTO>()
            // Рахуємо кількість відгуків
            .ForMember(dest => dest.ReviewsCount,
                       opt => opt.MapFrom(src => src.Reviews.Count))

            // Рахуємо середній рейтинг (з перевіркою, щоб не було ділення на нуль)
            .ForMember(dest => dest.AverageRating,
                       opt => opt.MapFrom(src => src.Reviews.Any()
                                                 ? src.Reviews.Average(r => r.Rating)
                                                 : 0.0));
    }
}
