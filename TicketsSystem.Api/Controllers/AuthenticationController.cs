using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Api.Controllers
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
        public async Task<IActionResult> CreateNewUser([FromBody] UserCreateDto userCreateDto)
            => ProcessResult(await _userService.CreateNewUserAsync(userCreateDto));

        [HttpPost("updateuser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserInformation([FromBody] UserUpdateDto userUpdateDto, string userId)
            => ProcessResult(await _userService.UpdateUserInformationAsync(userUpdateDto, userId));

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
