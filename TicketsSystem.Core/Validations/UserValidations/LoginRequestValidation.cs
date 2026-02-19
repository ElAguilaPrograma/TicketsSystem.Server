using FluentValidation;
using TicketsSystem.Core.DTOs.UserDTO;

namespace TicketsSystem.Core.Validations.UserValidations
{
    public class LoginRequestValidation : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidation()
        {
            RuleFor(l => l.Email).NotEmpty().EmailAddress();
            RuleFor(l => l.Password).NotEmpty();
        }
    }
}
