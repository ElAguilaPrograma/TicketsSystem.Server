using FluentValidation;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Domain.Enums;

namespace TicketsSystem.Core.Validations.TicketsValidations
{
    public class TicketsFilterValidation : AbstractValidator<GetAllTicketsFilterDto>
    {
        public TicketsFilterValidation()
        {
            string validStatuses = string.Join(", ", Enum.GetNames<TicketsStatusFilterValues>());
            string validPriorities = string.Join(", ", Enum.GetNames<TicketsPriorityFilterValues>());

            RuleFor(t => t.Status).IsEnumName(typeof(TicketsStatusFilterValues), caseSensitive: false)
                .WithMessage($"Invalid status value. Valid statuses are: {validStatuses}");
            RuleFor(t => t.Priority).IsEnumName(typeof(TicketsPriorityFilterValues), caseSensitive: false)
                .WithMessage($"Invalid priority value. Valid priorities are: {validPriorities}");
            RuleFor(t => t.Month).InclusiveBetween(1, 12).WithMessage("Invalid month value");
        }
    }
}
