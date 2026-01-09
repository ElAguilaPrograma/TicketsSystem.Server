using System;
using System.Collections.Generic;
using System.Text;
using TicketsSystem.Data.DTOs;

namespace TicketsSystem.Core.Models
{
    public class UserResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<UserDTO>? Users { get; set; }
    }
}
