using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.UserDTO
{
    public class LoginSuccessDto
    {
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
    }
}
