using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Domain.Entities;

namespace TicketsSystem.Core.Helpers.Mappers;

public static class UserMappers
{
    public static User ToEntity(this UserCreateDto dto) => new()
    {
        FullName = dto.FullName,
        Email = dto.Email,
        PasswordHash = dto.ConfirmPassword,
        Role = dto.Role,
        IsActive = dto.IsActive
    };

    public static UserReadDto ToReadDto(this User user) => new()
    {
        UserId = user.UserId,
        FullName = user.FullName,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };

    public static CurrentUserDto ToCurrentUserDto(this User user, string? email, string? role) => new()
    {
        UserId = user.UserId,
        Email = email,
        Role = role,
        FullName = user.FullName
    };
}
