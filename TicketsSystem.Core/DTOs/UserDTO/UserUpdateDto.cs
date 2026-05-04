using Microsoft.AspNetCore.Http;

namespace TicketsSystem.Core.DTOs.UserDTO
{
    public class UserUpdateDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string Role { get; set; } = null!;
        public IFormFile? ProfilePic { get; set; }
        public bool IsActive { get; set; }
    }
}
