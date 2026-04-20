using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Direction;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class DirectionService(IUnitOfWork unitOfWork, IMapper mapper) : IDirectionService
{
    public async Task<IEnumerable<GetDirectionDTO>> GetAllDirectionsAsync()
    {
        // Використовуємо базовий метод GetAllAsync з репозиторію
        var directions = await unitOfWork.DirectionRepository.GetAllAsync();

        return mapper.Map<IEnumerable<GetDirectionDTO>>(directions);
    }

    public async Task<GetDirectionDTO> GetDirectionByIdAsync(int id)
    {
        var direction = await unitOfWork.DirectionRepository.GetByIdAsync(id);

        if (direction == null)
        {
            throw new KeyNotFoundException($"Напрямок з ID {id} не знайдено.");
        }

        return mapper.Map<GetDirectionDTO>(direction);
    }

    public async Task<GetDirectionDTO> CreateDirectionAsync(AddDirectionDTO dto)
    {
        // Мапимо DTO в ентіті
        var direction = mapper.Map<Direction>(dto);

        // Твій BaseRepository відразу викликає SaveChangesAsync всередині AddAsync
        await unitOfWork.DirectionRepository.AddAsync(direction);

        return mapper.Map<GetDirectionDTO>(direction);
    }

    public async Task UpdateDirectionAsync(int id, AddDirectionDTO dto)
    {
        var direction = await unitOfWork.DirectionRepository.GetByIdAsync(id);

        if (direction == null)
        {
            throw new KeyNotFoundException($"Напрямок з ID {id} не знайдено для оновлення.");
        }

        // Накладаємо зміни з DTO на існуючу сутність
        mapper.Map(dto, direction);

        // Викликаємо оновлення через репозиторій
        await unitOfWork.DirectionRepository.UpdateAsync(direction);
    }

    public async Task DeleteDirectionAsync(int id)
    {
        var direction = await unitOfWork.DirectionRepository.GetByIdAsync(id);

        if (direction == null)
        {
            throw new KeyNotFoundException($"Напрямок з ID {id} не знайдено для видалення.");
        }

        // Твій BaseRepository має метод DeleteAsync, який відразу зберігає зміни
        await unitOfWork.DirectionRepository.DeleteAsync(direction);
    }
}
