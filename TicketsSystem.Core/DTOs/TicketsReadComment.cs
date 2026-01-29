using System;
using System.Collections.Generic;
using System.Text;

namespace TicketsSystem.Core.DTOs
{
    public class TicketsReadComment
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsInternal { get; set; }
    }
}
