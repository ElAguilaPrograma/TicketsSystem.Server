using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Core.DTOs.TicketsDTO;

namespace TicketsSystem.Core.DTOs.NotificationDTO
{
    public class NotificationReadDto
    {
        public Guid NotificationId { get; set; }
        public Guid UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
