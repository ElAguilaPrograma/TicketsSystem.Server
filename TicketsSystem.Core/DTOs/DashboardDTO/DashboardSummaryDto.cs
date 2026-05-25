namespace TicketsSystem.Core.DTOs.DashboardDTO;

public class DashboardSummaryDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int ResolvedToday { get; set; }
    public double AvgResolutionHours { get; set; }
    public Dictionary<string, int> TicketsByPriority { get; set; } = new();
    public Dictionary<string, int> TicketsByStatus { get; set; } = new();
    public IEnumerable<DashboardRecentTicketDto> RecentTickets { get; set; } = [];
}

public class DashboardRecentTicketDto
{
    public Guid TicketId { get; set; }
    public string Title { get; set; } = null!;
    public string CreatedByUser { get; set; } = null!;
    public string? PriorityName { get; set; }
    public string? StatusName { get; set; }
    public DateTime CreatedAt { get; set; }
}
