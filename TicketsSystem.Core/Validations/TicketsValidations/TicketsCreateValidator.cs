using FluentValidation;
using TicketsSystem.Core.DTOs;

namespace TicketsSystem.Core.Validations.TicketsValidations
{
    public class TicketsCreateValidator : AbstractValidator<TicketsCreateDto>
    {
        private readonly ITicketsCustomValidations _ticketsCustomValidations;
        public TicketsCreateValidator(ITicketsCustomValidations ticketsCustomValidations)
        {
            _ticketsCustomValidations = ticketsCustomValidations;

            RuleFor(t => t.Title).NotEmpty().MaximumLength(50);
            RuleFor(t => t.Description).NotEmpty().MaximumLength(500);
            RuleFor(t => t.StatusId).Must(t => _ticketsCustomValidations.CorrectStatusValue(t))
                .WithMessage("'{PropertyValue}' is not a recognized status level.");
            RuleFor(t => t.PriorityId).Must(t => _ticketsCustomValidations.CorrectPriorityValue(t))
                .WithMessage("'{PropertyValue}' is not a recognized priority level.");
        }

    }
}
