using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Api.Controllers;
using TicketsSystem.Core.Services;
using TicketsSystem.Data.DTOs;

namespace TicketsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ApiBaseController
    {
        private readonly IUserService _userService;
        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getallusers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
            => ProcessResult(await _userService.GetAllUsersAsync());

        [HttpPost("createuser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewUser([FromBody] UserDTO userDTO)
            => ProcessResult(await _userService.CreateNewUserAsync(userDTO));

        [HttpPost("updateuser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserInformation([FromBody] UserDTO userDTO, string userId)
            => ProcessResult(await _userService.UpdateUserInformationAsync(userDTO, userId));

        [HttpGet("searchuser/{query}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchUser(string query)
            => ProcessResult(await _userService.SearchUserAsync(query));

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
            => ProcessResult(await _userService.LoginAsync(request));

        [HttpPost("deactivateauser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAUser(string userId)
            => ProcessResult(await _userService.DeactivateOrActivateAUserAsync(userId));
    }
}
