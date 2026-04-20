using AutoMapper;
using BLL.Interfaces;
using BLL.Models.ServiceType;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class ServiceTypeService(IUnitOfWork unitOfWork, IMapper mapper) : IServiceTypeService
{
    public async Task<IEnumerable<GetServiceTypeDTO>> GetAllServiceTypesAsync()
    {
        var types = await unitOfWork.ServiceTypeRepository.GetAllAsync();
        return mapper.Map<IEnumerable<GetServiceTypeDTO>>(types);
    }

    public async Task<GetServiceTypeDTO> GetServiceTypeByIdAsync(int id)
    {
        var serviceType = await unitOfWork.ServiceTypeRepository.GetByIdAsync(id);
        if (serviceType == null)
        {
            throw new KeyNotFoundException($"Тип послуги з ID {id} не знайдено.");
        }
        return mapper.Map<GetServiceTypeDTO>(serviceType);
    }

    public async Task<GetServiceTypeDTO> CreateServiceTypeAsync(AddServiceTypeDTO dto)
    {
        var serviceType = mapper.Map<ServiceType>(dto);
        await unitOfWork.ServiceTypeRepository.AddAsync(serviceType);
        return mapper.Map<GetServiceTypeDTO>(serviceType);
    }

    public async Task UpdateServiceTypeAsync(int id, UpdateServiceTypeDTO dto)
    {
        if (id != dto.Id)
        {
            throw new ArgumentException("ID у маршруті та у тілі запиту не збігаються.");
        }

        var serviceType = await unitOfWork.ServiceTypeRepository.GetByIdAsync(id);
        if (serviceType == null)
        {
            throw new KeyNotFoundException($"Тип послуги з ID {id} не знайдено.");
        }

        mapper.Map(dto, serviceType);
        await unitOfWork.ServiceTypeRepository.UpdateAsync(serviceType);
    }

    public async Task DeleteServiceTypeAsync(int id)
    {
        var serviceType = await unitOfWork.ServiceTypeRepository.GetByIdAsync(id);
        if (serviceType == null)
        {
            throw new KeyNotFoundException($"Тип послуги з ID {id} не знайдено.");
        }

        await unitOfWork.ServiceTypeRepository.DeleteAsync(serviceType);
    }
}
