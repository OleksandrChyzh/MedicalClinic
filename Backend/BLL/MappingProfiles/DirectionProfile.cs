using AutoMapper;
using BLL.Models.Direction;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class DirectionProfile : Profile
{
    public DirectionProfile()
    {
        // Мапінг для створення (DTO -> Entity)
        this.CreateMap<AddDirectionDTO, Direction>();

        // Мапінг для отримання (Entity -> DTO)
        // Тепер він працює автоматично для Id, Name та Description
        this.CreateMap<Direction, GetDirectionDTO>();
    }
}
