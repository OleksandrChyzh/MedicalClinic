using BLL.Models.Service;

namespace BLL.Interfaces;
public interface IServiceManagementService
{
    Task<IEnumerable<GetServiceDTO>> GetServicesAsync(int? directionId, int? typeId);
    Task<GetServiceDTO> GetServiceByIdAsync(int id);
    Task<GetServiceDTO> CreateServiceAsync(AddServiceDTO dto);
    Task UpdateServiceAsync(int id, UpdateServiceDTO dto);
}
