using FluentValidation;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Domain.Enums;

namespace TicketsSystem.Core.Validations.TicketsValidations
{
    public class TicketsCreateValidator : AbstractValidator<TicketsCreateDto>
    {
        public TicketsCreateValidator()
        {

            RuleFor(t => t.Title).NotEmpty().MaximumLength(50);
            RuleFor(t => t.Description).NotEmpty().MaximumLength(500);
            RuleFor(t => t.PriorityId).Must(t => Enum.IsDefined(typeof(TicketsPriorityValue), t))
                .WithMessage("'{PropertyValue}' is not a recognized priority level.");
        }
        private static bool CorrectStatusValue(int statusValue)
        {
            if (statusValue <= 0 || statusValue > 5)
                return false;

            return true;
        }

        private static bool CorrectPriorityValue(int priorityValue)
        {
            if (priorityValue <= 0 || priorityValue > 4)
                return false;

            return true;
        }
    }
}
