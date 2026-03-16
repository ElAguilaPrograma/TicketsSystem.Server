using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.UserDTO
{
    public class CurrentUserDto
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; } = string.Empty;
        public string? Role { get; set; } = string.Empty;
        public string? FullName { get; set; } = string.Empty;
    }
}
