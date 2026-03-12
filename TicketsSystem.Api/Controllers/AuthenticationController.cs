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
        private readonly IWebHostEnvironment _env;

        public AuthenticationController(IUserService userService, IWebHostEnvironment env)
        {
            _userService = userService;
            _env = env;
        }

        [HttpGet("getallusers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
            => ProcessResult(await _userService.GetAllUsersAsync(page, pageSize));

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
        {
            var result = await _userService.LoginAsync(request);
            if (result.IsSuccess)
            {
                var loginData = result.Value;
                var isDev = _env.IsDevelopment();
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    // SameSite=None con Secure=true permite enviar la cookie en requests XHR cross-origin
                    // (frontend en :4200 -> backend en :7121). En prod usar Strict.
                    Secure = true,
                    SameSite = isDev ? SameSiteMode.None : SameSiteMode.Strict,
                    Expires = loginData.Expiration
                };

                Response.Cookies.Append("AuthToken", loginData.Token, cookieOptions);

                return Ok(new { message = "Login Successful" });
            }

            return ProcessResult(result);
        }

        [HttpGet("getcurrentuser")]
        [Authorize]
        public IActionResult GetCurrentUser()
            => ProcessResult(_userService.GetCurrentUser());

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            var isDev = _env.IsDevelopment();
            Response.Cookies.Delete("AuthToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = isDev ? SameSiteMode.None : SameSiteMode.Strict
            });
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("deactivateauser/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAUser(string userId)
            => ProcessResult(await _userService.DeactivateOrActivateAUserAsync(userId));
    }
}
