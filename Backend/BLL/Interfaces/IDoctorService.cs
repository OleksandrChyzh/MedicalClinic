using BLL.Models.Doctor;

namespace BLL.Interfaces;
public interface IDoctorService
{
    Task<IEnumerable<GetDoctorDto>> GetAllDoctorsAsync(int? directionId);
    Task<GetDoctorDto> GetDoctorByIdAsync(int id);
    Task<GetDoctorDto> CreateDoctorAsync(AddDoctorDTO dto);
    Task UpdateDoctorAsync(int id, UpdateDoctorDTO dto);
    Task DeleteDoctorAsync(int id);
}
