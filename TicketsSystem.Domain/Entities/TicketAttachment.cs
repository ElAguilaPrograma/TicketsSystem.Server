using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace TicketsSystem.Domain.Entities
{
    public class TicketAttachment
    {
        public Guid TicketAttachmentId { get; set; }
        public Guid TicketId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        [JsonIgnore]
        public virtual Ticket Ticket { get; set; } = null!;
        public TicketAttachment()
        {
            TicketAttachmentId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}
