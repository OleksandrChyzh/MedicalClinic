using AutoMapper;
using BLL.Models.Schedule;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class ScheduleProfile : Profile
{
    public ScheduleProfile()
    {
        // 1. Мапінг для читання (З БД на фронтенд)
        // Перетворюємо TimeOnly у зручний рядок "HH:mm"
        this.CreateMap<Schedule, GetScheduleDTO>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ToString("HH:mm")))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ToString("HH:mm")));

        // 2. Мапінг для створення (З фронтенду в БД)
        // Парсимо рядок "HH:mm" у тип TimeOnly
        this.CreateMap<AddScheduleDTO, Schedule>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.Parse(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.Parse(src.EndTime)));

        // 3. Мапінг для оновлення (З фронтенду в БД)
        // Аналогічно парсимо рядок "HH:mm" у тип TimeOnly
        this.CreateMap<UpdateScheduleDTO, Schedule>()
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => TimeOnly.Parse(src.StartTime)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => TimeOnly.Parse(src.EndTime)));
    }
}
