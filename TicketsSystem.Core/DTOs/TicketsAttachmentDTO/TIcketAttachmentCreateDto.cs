namespace TicketsSystem.Core.DTOs.TicketsAttachmentDTO
{
    public class TicketsAttachmentCreateDto
    {
        public Guid TicketAttachmentId { get; set; }
        public Guid TicketId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string Path {  get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }    
}
