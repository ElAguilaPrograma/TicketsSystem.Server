using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Data.DTOs;

namespace TicketsSystem.Core.Validations
{
    public class TicketsDTOValidator : AbstractValidator<TicketsDTO>
    {
        public TicketsDTOValidator()
        {
            RuleFor(t => t.Title).NotEmpty().MaximumLength(50);
            RuleFor(t => t.Description).NotEmpty().MaximumLength(200);
        }
    }
}
