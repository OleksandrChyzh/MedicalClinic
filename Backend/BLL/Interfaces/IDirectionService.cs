using BLL.Models.Direction;

namespace BLL.Interfaces;
public interface IDirectionService
{
    Task<IEnumerable<GetDirectionDTO>> GetAllDirectionsAsync();
    Task<GetDirectionDTO> GetDirectionByIdAsync(int id);
    Task<GetDirectionDTO> CreateDirectionAsync(AddDirectionDTO dto);
    Task UpdateDirectionAsync(int id, AddDirectionDTO dto);
    Task DeleteDirectionAsync(int id);
}
