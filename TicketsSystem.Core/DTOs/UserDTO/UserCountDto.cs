using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.UserDTO
{
    public class UserCountDto
    {
        public int TotalUsers { get; set; }
        public int Users { get; set; }
        public int Admins { get; set; }
        public int Agents { get; set; }
    }
}
