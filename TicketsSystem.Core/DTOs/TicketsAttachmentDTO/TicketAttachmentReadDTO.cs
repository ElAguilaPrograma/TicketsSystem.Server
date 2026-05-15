namespace TicketsSystem.Core.DTOs.TicketsAttachmentDTO
{
    public class TicketAttachmentReadDTO
    {
        public Guid TicketAttachmentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}