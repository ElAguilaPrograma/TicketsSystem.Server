using FluentValidation;
using TicketsSystem.Core.DTOs;

namespace TicketsSystem.Core.Validations
{
    public class UserDTOValidator : AbstractValidator<UserDTO>
    {
        private readonly List<string> _validRoles = ["Admin", "Agent", "User"];
        public UserDTOValidator()
        {
            RuleFor(u => u.FullName).NotEmpty().MinimumLength(5);
            RuleFor(u => u.Email).NotEmpty().EmailAddress();
            RuleFor(u => u.Password).NotEmpty().MinimumLength(5)
                .Matches("[A-Z]").WithMessage("Most contain a mayus");
            RuleFor(u => u.IsActive).NotEmpty();
            RuleFor(u => u.Role).NotEmpty().Must(role => _validRoles.Contains(role))
                .WithMessage("Admin, Agent and User are the available roles");
        }
    }
}
