using FluentValidation;
using TicketsSystem.Core.DTOs.UserDTO;

namespace TicketsSystem.Core.Validations.UserValidations
{
    public class UserPasswordValidator : AbstractValidator<UserUpdateDto>
    {
        public UserPasswordValidator()
        {
            RuleFor(u => u.Password).NotEmpty().MinimumLength(5)
                .Matches("[A-Z]").WithMessage("Most contain a mayus");
            RuleFor(u => u.ConfirmPassword).NotEmpty().MinimumLength(5)
                .Matches("[A-Z]").WithMessage("Most contain a mayus")
                .Equal(p => p.Password).WithMessage("The passwords do not match.");
        }
    }
}
