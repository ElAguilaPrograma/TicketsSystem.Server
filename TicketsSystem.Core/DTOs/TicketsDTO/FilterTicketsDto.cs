using System;

namespace TicketsSystem.Core.DTOs.TicketsDTO
{
    public class FilterTicketsDto
    {
        public string? UserId { get; set; } = null;
        public bool CurrentUserOnly { get; set; }
        public bool AssignedToMeOnly { get; set; }
        public string? Status { get; set; } = "All";
        public string? Priority { get; set; } = "All";
        public int? Month { get; set; } = null;
        public int? Year { get; set; } = null;
        public string? QuerySearch { get; set; } = "";
        public bool? HasAssignment { get; set; } = null;
        public int TimezoneOffsetMinutes { get; set; } = 0;
    }
}
