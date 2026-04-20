using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Service;
using DAL.Entities;
using DAL.Interfaces;

namespace BLL.Services;
public class ServiceManagementService(IUnitOfWork unitOfWork, IMapper mapper) : IServiceManagementService
{
    public async Task<IEnumerable<GetServiceDTO>> GetServicesAsync(int? directionId, int? typeId)
    {
        // Використовуємо твій готовий метод з репозиторію.
        // Фільтр перевіряє: якщо параметр null, то ігноруємо умову, інакше шукаємо збіг.
        var services = await unitOfWork.ServiceRepository.GetAllAsync(
            filter: s => (!directionId.HasValue || s.DirectionId == directionId.Value) &&
                         (!typeId.HasValue || s.TypeId == typeId.Value),
            includes: s => s.Reviews // Підтягуємо відгуки для мапера
        );

        return mapper.Map<IEnumerable<GetServiceDTO>>(services);
    }

    public async Task<GetServiceDTO> GetServiceByIdAsync(int id)
    {
        // Використовуємо GetFirstOrDefaultAsync, щоб відразу підтягнути Reviews
        var service = await unitOfWork.ServiceRepository.GetFirstOrDefaultAsync(
            filter: s => s.Id == id,
            includes: s => s.Reviews
        );

        if (service == null)
        {
            throw new KeyNotFoundException($"Послугу з ID {id} не знайдено.");
        }

        return mapper.Map<GetServiceDTO>(service);
    }

    public async Task<GetServiceDTO> CreateServiceAsync(AddServiceDTO dto)
    {
        // Перевіряємо, чи існують зовнішні ключі через відповідні репозиторії
        var direction = await unitOfWork.DirectionRepository.GetByIdAsync(dto.DirectionId);
        if (direction == null)
        {
            throw new ArgumentException("Вказаний напрямок не існує.");
        }

        var serviceType = await unitOfWork.ServiceTypeRepository.GetByIdAsync(dto.TypeId);
        if (serviceType == null)
        {
            throw new ArgumentException("Вказаний тип послуги не існує.");
        }

        var newService = mapper.Map<Service>(dto);

        // У твоєму BaseRepository метод AddAsync вже викликає SaveChangesAsync,
        // тому додатково викликати unitOfWork.SaveAsync() тут не обов'язково.
        await unitOfWork.ServiceRepository.AddAsync(newService);

        return mapper.Map<GetServiceDTO>(newService);
    }

    public async Task UpdateServiceAsync(int id, UpdateServiceDTO dto)
    {
        if (id != dto.Id)
        {
            throw new ArgumentException("ID у шляху запиту не збігається з ID у тілі.");
        }

        // Отримуємо сутність без відгуків (вони нам для апдейту базових полів не потрібні)
        var service = await unitOfWork.ServiceRepository.GetByIdAsync(id);

        if (service == null)
        {
            throw new KeyNotFoundException($"Послугу з ID {id} не знайдено.");
        }

        // Накладаємо зміни з DTO
        mapper.Map(dto, service);

        // У BaseRepository метод UpdateAsync також викликає SaveChangesAsync
        await unitOfWork.ServiceRepository.UpdateAsync(service);
    }
}
