using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.UserDTO
{
    public class FilterUsersDto
    {
        public string Role { get; set; } = "All Roles";
        public string IsActive { get; set; } = "All";
        public int TimezoneOffsetMinutes { get; set; } = 0;
    }
}
