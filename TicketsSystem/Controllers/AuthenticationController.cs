using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TicketsSystem.Core.Services;
using TicketsSystem.Data.DTOs;

namespace TicketsSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserService _userService;
        public AuthenticationController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("getallusers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsersAsync();

            if (result.IsFailed)
                return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

            return Ok(result.Value);
        }

        [HttpPost("createuser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNewUser([FromBody] UserDTO userDTO)
        {
            var result = await _userService.CreateNewUserAsync(userDTO);

            if (result.IsFailed)
                return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _userService.LoginAsync(request);
            
            if (result.IsFailed)
                return Unauthorized(new { message = result.Errors.First().Message });

            return Ok(result.Value);
        }
    }
}
