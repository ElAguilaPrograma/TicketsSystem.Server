using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs.TicketsCommentsDTO
{
    public class TicketsCreateComment
    {
        public string Content { get; set; } = null!;
        public bool IsInternal { get; set; }
    }
}
