using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text;
using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        public CurrentUserService(IHttpContextAccessor httpContext, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContext;
        }

        public Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier) 
                ?? user?.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("Unauthenticated user.");
        }

        public string? GetCurrentUserEmail()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Email)?.Value ??
                user?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
        }

        public string? GetCurrentUserRole()
        {
            var user = _httpContextAccessor?.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Role)?.Value;
        }

        public async Task<string> GetCurrentUserName()
        {
            var user = await _userRepository.GetById(this.GetCurrentUserId());
            return user!.FullName;
        }
    }
}
