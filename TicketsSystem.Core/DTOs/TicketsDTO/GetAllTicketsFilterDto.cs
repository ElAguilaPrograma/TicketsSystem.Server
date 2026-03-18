using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.TicketsDTO
{
    public class GetAllTicketsFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string? Status { get; set; } = "All";
        public string? Priority { get; set; } = "All";
        public int? Month { get; set; } = null;
        public int? Year { get; set; } = null;
        public string? QuerySearch { get; set; } = "";
    }
}
