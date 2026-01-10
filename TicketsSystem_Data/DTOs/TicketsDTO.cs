using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Data.DTOs
{
    public class TicketsDTO
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int StatusId { get; set; }
        public string? StatusName { get; set; } 
        public int PriorityId { get; set; }
        public string? PriorityName { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }

    }
}
