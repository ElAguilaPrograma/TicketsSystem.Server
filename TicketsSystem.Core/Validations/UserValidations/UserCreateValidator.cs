using FluentValidation;
using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Domain.Enums;

namespace TicketsSystem.Core.Validations.UserValidations
{
    public class UserCreateValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateValidator()
        {
            RuleFor(u => u.FullName).NotEmpty().MinimumLength(5);
            RuleFor(u => u.Email).NotEmpty().EmailAddress();
            RuleFor(u => u.Password).NotEmpty().MinimumLength(5)
                .Matches("[A-Z]").WithMessage("Most contain a mayus");
            RuleFor(u => u.ConfirmPassword).NotEmpty().MinimumLength(5)
                .Matches("[A-Z]").WithMessage("Most contain a mayus")
                .Equal(p => p.Password).WithMessage("The passwords do not match.");
            RuleFor(u => u.IsActive).NotEmpty();
            RuleFor(u => u.Role).NotEmpty()
                .Must(role => Enum.TryParse<UserRole>(role, ignoreCase: true, out _))
                .WithMessage($"Invalid role. Accepted roles: {string.Join(", ", Enum.GetNames<UserRole>())}");
        }
    }
}
