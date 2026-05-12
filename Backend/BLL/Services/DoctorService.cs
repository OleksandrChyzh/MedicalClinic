using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BLL.Interfaces;
using BLL.Models.Doctor;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BLL.Services;
public class DoctorService(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    UserManager<User> userManager) : IDoctorService
{
    public async Task<IEnumerable<GetDoctorDto>> GetAllDoctorsAsync(int? directionId)
    {
        // Використовуємо filter для перевірки DirectionId
        var doctors = await unitOfWork.DoctorRepository.GetAllAsync(
            filter: d => !directionId.HasValue || d.DirectionId == directionId.Value,
            includes: [d => d.Direction, d => d.User, d => d.Reviews]
        );

        return mapper.Map<IEnumerable<GetDoctorDto>>(doctors);
    }

    public async Task<GetDoctorDto> GetDoctorByIdAsync(int id)
    {
        var doctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(
            filter: d => d.Id == id,
            includes: [d => d.Direction, d => d.User, d => d.Reviews]
        );
        if (doctor == null)
        {
            throw new KeyNotFoundException("Лікаря не знайдено.");
        }

        return mapper.Map<GetDoctorDto>(doctor);
    }

    public async Task<GetDoctorDto> CreateDoctorAsync(AddDoctorDTO dto)
    {
        // 1. Створюємо Identity User, використовуючи дані з dto.Account
        var user = new User
        {
            UserName = dto.Account.UserName, // Беремо UserName з твого record
            Email = dto.Account.Email,
            PhoneNumber = dto.Account.PhoneNumber,
        };

        var result = await userManager.CreateAsync(user, dto.Account.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Помилка створення акаунта: {errors}");
        }

        // 2. Додаємо роль
        await userManager.AddToRoleAsync(user, "Doctor");

        // 3. Створюємо сутність Doctor
        var doctor = mapper.Map<Doctor>(dto); // Мапер ігноруватиме Account, бо в Doctor немає такого поля
        doctor.UserId = user.Id;

        await unitOfWork.DoctorRepository.AddAsync(doctor);

        return await GetDoctorByIdAsync(doctor.Id);
    }

    public async Task UpdateDoctorAsync(int id, UpdateDoctorDTO dto)
    {
        if (id != dto.Id)
        {
            throw new ArgumentException("ID не збігаються.");
        }

        var doctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(
            filter: d => d.Id == id,
            includes: [d => d.User]
        );

        if (doctor == null)
        {
            throw new KeyNotFoundException("Лікаря не знайдено.");
        }

        // Мапимо оновлені дані безпосередньо в сутність лікаря (AutoMapper проігнорує Account)
        mapper.Map(dto, doctor);

        // Оновлюємо пов'язаного User
        if (doctor.User != null)
        {
            // Якщо прийшли дані для оновлення акаунта
            if (dto.Account != null)
            {
                if (!string.IsNullOrWhiteSpace(dto.Account.UserName))
                {
                    doctor.User.UserName = dto.Account.UserName;
                }

                if (!string.IsNullOrWhiteSpace(dto.Account.PhoneNumber))
                {
                    doctor.User.PhoneNumber = dto.Account.PhoneNumber;
                }

                // Логіка оновлення пароля (якщо Адмін передав новий пароль)
                if (!string.IsNullOrWhiteSpace(dto.Account.Password))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(doctor.User);
                    var passResult = await userManager.ResetPasswordAsync(doctor.User, token, dto.Account.Password);

                    if (!passResult.Succeeded)
                    {
                        var errors = string.Join(", ", passResult.Errors.Select(e => e.Description));
                        throw new Exception($"Помилка оновлення пароля: {errors}");
                    }
                }
            }

            var updateResult = await userManager.UpdateAsync(doctor.User);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new Exception($"Помилка оновлення акаунта користувача: {errors}");
            }
        }

        // Зберігаємо зміни сутності лікаря
        await unitOfWork.DoctorRepository.UpdateAsync(doctor);
    }

    public async Task DeleteDoctorAsync(int id)
    {
        var doctor = await unitOfWork.DoctorRepository.GetFirstOrDefaultAsync(
            filter: d => d.Id == id,
            includes: [d => d.User] // Підтягуємо юзера, щоб видалити його
        );

        if (doctor == null)
        {
            throw new KeyNotFoundException("Лікаря не знайдено.");
        }

        var user = doctor.User;

        // 1. Спочатку видаляємо сутність Doctor
        await unitOfWork.DoctorRepository.DeleteAsync(doctor);

        // 2. Потім видаляємо акаунт User
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }
    }
}
