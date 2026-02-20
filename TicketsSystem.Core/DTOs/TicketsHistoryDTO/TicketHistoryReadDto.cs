using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.TicketsHistoryDTO
{
    public class TicketHistoryReadDto
    {
        public Guid TicketId { get; set; }
        public Guid ChangedByUserId { get; set; }
        public Guid ChangeGroupId { get; set; }
        public string FieldName { get; set; } = null!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
