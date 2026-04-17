using AutoMapper;
using BLL.Models.User;
using DAL.Entities;

namespace BLL.MappingProfiles;
public class UserProfile : Profile
{
    public UserProfile()
    {
        // 1. Реєстрація: Register DTO -> User
        // Пароль сюди не мапиться, бо UserManager хешує його окремо
        this.CreateMap<Register, User>();

        // 2. Видача на фронт: User -> GetUser DTO
        this.CreateMap<User, GetUser>();

        // 3. Оновлення профілю: UpdateUser DTO -> User
        // Фішка: ігноруємо null, щоб не затерти старі дані, якщо юзер прислав тільки нове ім'я
        this.CreateMap<UpdateUser, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}
