using TicketsSystem.Domain.Enums;

namespace TicketsSystem.Domain.Entities;

public class User
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    private string _role = null!;
    public string Role { get => _role; set
        {
            if (!Enum.TryParse<UserRole>(value, ignoreCase: true, out _))
                throw new ArgumentException($"Invalid role: '{value}'. Accepted roles: " +
                    $"{string.Join(", ", Enum.GetNames<UserRole>())}");
            _role = value;
        } 
    }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Notification> Notifications { get; set; } = [];
    public virtual ICollection<Ticket> TicketAssignedToUsers { get; set; } = [];
    public virtual ICollection<TicketComment> TicketComments { get; set; } = [];
    public virtual ICollection<Ticket> TicketCreatedByUsers { get; set; } = [];
    public virtual ICollection<TicketHistory> TicketHistories { get; set; } = [];
}
