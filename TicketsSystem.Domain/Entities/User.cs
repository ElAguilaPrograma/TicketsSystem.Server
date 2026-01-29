namespace TicketsSystem.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Notification> Notifications { get; set; } = [];
    public virtual ICollection<Ticket> TicketAssignedToUsers { get; set; } = [];
    public virtual ICollection<TicketComment> TicketComments { get; set; } = [];
    public virtual ICollection<Ticket> TicketCreatedByUsers { get; set; } = [];
    public virtual ICollection<TicketHistory> TicketHistories { get; set; } = [];
}
