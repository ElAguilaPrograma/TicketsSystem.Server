using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace TicketsSystem.Core.Services
{
    public interface ICurrentUserService
    {
        string? GetCurrentUserEmail();
        Guid GetCurrentUserId();
        string? GetCurrentUserRole();
    }
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserService(IHttpContextAccessor httpContext)
        {
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
    }
}
