using BLL.Models.ServiceType;

namespace BLL.Interfaces;
public interface IServiceTypeService
{
    Task<IEnumerable<GetServiceTypeDTO>> GetAllServiceTypesAsync();
    Task<GetServiceTypeDTO> GetServiceTypeByIdAsync(int id);
    Task<GetServiceTypeDTO> CreateServiceTypeAsync(AddServiceTypeDTO dto);
    Task UpdateServiceTypeAsync(int id, UpdateServiceTypeDTO dto);
    Task DeleteServiceTypeAsync(int id);
}
