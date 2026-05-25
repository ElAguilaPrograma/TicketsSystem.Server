namespace TicketsSystem.Core.DTOs.DashboardDTO;

public class DashboardFilterDto
{
    public bool CurrentUserOnly { get; set; } = true;
    public bool AssignedToMeOnly { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int RecentTicketsTake { get; set; } = 5;
}
