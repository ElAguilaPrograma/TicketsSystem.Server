using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.TicketsDTO
{
    public class GetCurrentUserTicketsCount
    {
        public int TotalTickets { get; set; }
        public int TicketsOpen { get; set; }
        public int TicketsReopen { get; set; }
        public int TicketsClosed { get; set; }
    }
}
