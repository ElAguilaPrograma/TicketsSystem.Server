using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.TicketsHistoryDTO
{
    public class TicketHistoryGroupDto
    {
        public Guid ChangeGroupId { get; set; }
        public DateTime ChangedAt { get; set; }
        public Guid ChangedByUserId { get; set; }
        public IEnumerable<TicketHistoryReadDto> Changes { get; set; } = [];
    }
}
