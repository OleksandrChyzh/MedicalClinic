using AutoMapper;
using BLL.Models.ServiceType;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class ServiceTypeProfile : Profile
{
    public ServiceTypeProfile()
    {
        // 1. Отримання (Entity -> DTO)
        this.CreateMap<ServiceType, GetServiceTypeDTO>();

        // 2. Створення (DTO -> Entity)
        this.CreateMap<AddServiceTypeDTO, ServiceType>();

        // 3. Оновлення (DTO -> Entity)
        this.CreateMap<UpdateServiceTypeDTO, ServiceType>();
    }
}
