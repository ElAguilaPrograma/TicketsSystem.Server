using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.UserDTO
{
    public class GetAllUsersFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string Role { get; set; } = "All Roles";
        public string IsActive { get; set; } = "All";
        public string QuerySearch { get; set; } = "";
    }
}
