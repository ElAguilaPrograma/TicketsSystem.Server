using FluentResults;
using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Core.DTOs.TicketsHistoryDTO;

namespace TicketsSystem.Core.Interfaces
{
    public interface ITicketHistoryService
    {
        Task<Result<IEnumerable<TicketHistoryGroupDto>>> GetTicketHistoryAsync(string ticketIdStr);
    }
}
