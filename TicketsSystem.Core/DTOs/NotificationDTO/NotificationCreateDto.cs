using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.DTOs.TicketsCommentsDTO;

namespace TicketsSystem.Core.DTOs.NotificationDTO
{
    public class NotificationCreateDto
    {
        public Guid UserId { get; set; }
        public Guid? ContentId { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public bool IsRead { get; set; }
        public TicketsReadDto? Ticket { get; set; }
        public TicketsReadComment? TicketsReadComment { get; set; }
    }
}
