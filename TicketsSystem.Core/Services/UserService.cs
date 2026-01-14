using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using TicketsSystem.Core.Validations;
using TicketsSystem.Data.DTOs;
using TicketsSystem_Data;
using TicketsSystem_Data.Repositories;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using TicketsSystem.Core.Models;
using Microsoft.AspNetCore.Http;
using FluentResults;
using System.Threading.Tasks;
using TicketsSystem.Core.Errors;
using TicketsSystem.Data.Repositories;


namespace TicketsSystem.Core.Services
{
    public interface IUserService
    {
        Task<Result> CreateNewUserAsync(UserDTO userDTO);
        Task<Result> DeactivateOrActivateAUserAsync(string userIdStr);
        Task<Result<IEnumerable<UserDTO>>> GetAllUsersAsync();
        Task<Result<LoginSuccessDto>> LoginAsync(LoginRequest request);
        Task<Result<IEnumerable<UserDTO>>> SearchUserAsync(string query);
        Task<Result> UpdateUserInformationAsync(UserDTO userDTO, string userIdStr);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserDTOValidator _userValidation;
        private readonly LoginRequestValidation _loginRequestValidation;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;
        private readonly ITicketsRepository _ticketRepository;
        public UserService(
            IUserRepository userRepository,
            UserDTOValidator validationRules,
            IPasswordHasher<User> passwordHasher,
            LoginRequestValidation loginValidationsRules,
            IConfiguration configuration,
            ITicketsRepository ticketsRepository)
        {
            _userRepository = userRepository;
            _userValidation = validationRules;
            _passwordHasher = passwordHasher;
            _loginRequestValidation = loginValidationsRules;
            _config = configuration;
            _ticketRepository = ticketsRepository;
        }

        public async Task<Result<IEnumerable<UserDTO>>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsers();
            IEnumerable<UserDTO> userDTOs = users.Select(u => new UserDTO
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Password = " ",
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });

            return Result.Ok(userDTOs);
        }

        public async Task<Result> CreateNewUserAsync(UserDTO userDTO)
        {
            if (userDTO == null)
                return Result.Fail(new BadRequestError("userDto is required"));

            var validationResult = await _userValidation.ValidateAsync(userDTO);
            if (!validationResult.IsValid)
            {
                // throw new ValidationException("One or more fields do not meet the requirements." + validationResult.Errors);
                var errorMessages = validationResult.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessages);
            }

            if (await _userRepository.EmailExist(userDTO.Email))
                return Result.Fail(new BadRequestError("The user already exist"));

            var newUser = new User
            {
                FullName = userDTO.FullName,
                Email = userDTO.Email,
                PasswordHash = userDTO.Password,
                Role = userDTO.Role,
                IsActive = userDTO.IsActive
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, userDTO.Password);

            await _userRepository.CreateNewUser(newUser);

            return Result.Ok(); //.WithSuccess("User created");
        }

        public async Task<Result<LoginSuccessDto>> LoginAsync(LoginRequest request)
        {
            if (request == null)
                return Result.Fail(new BadRequestError("Email or password are required"));

            var validationResult = await _loginRequestValidation.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errorMessage = validationResult.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessage);
            }

            var user = await _userRepository.Login(request.Email);

            if (user == null)
                return Result.Fail(new UnauthorizedError("Incorrect credentials"));

            if (!user.IsActive)
                return Result.Fail(new ForbiddenError("User is deactivated"));

            var verificationPassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verificationPassword != PasswordVerificationResult.Success)
                return Result.Fail(new UnauthorizedError("Incorrect credentials"));

            var tokenExpiration = DateTime.UtcNow.AddDays(7);
            var token = GenereteJwtToken(user, tokenExpiration);

            return Result.Ok(new LoginSuccessDto
            {
                Token = token,
                Expiration = tokenExpiration
            });
        }

        public async Task<Result> UpdateUserInformationAsync(UserDTO userDTO, string userIdStr)
        {
            Guid userId = Guid.Parse(userIdStr);

            if (userDTO == null)
                return Result.Fail(new BadRequestError("UserDTO is required"));

            var validationResult = await _userValidation.ValidateAsync(userDTO);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessages);
            }

            var user = await _userRepository.GetUserById(userId);
            if (user == null)
                return Result.Fail(new NotFoundError("The user does not exist"));

            user.FullName = userDTO.FullName;
            user.Email = userDTO.Email;
            user.Role = userDTO.Role;
            user.IsActive = userDTO.IsActive;
            user.PasswordHash = _passwordHasher.HashPassword(user, userDTO.Password);

            await _userRepository.UpdateUserInfo(user);

            return Result.Ok();
        }

        private string GenereteJwtToken(User user, DateTime expiration)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                // Unique id token
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Roles
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                // Expiration time
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result<IEnumerable<UserDTO>>> SearchUserAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Result.Fail(new BadRequestError("Query format is not valid"));

            query = query.ToLower();

            var user = await _userRepository.SearchUsers(query);

            IEnumerable<UserDTO> userDTO = user.Select(u => new UserDTO
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Password = " ",
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });
            
            return Result.Ok(userDTO);
        }

        public async Task<Result> DeactivateOrActivateAUserAsync(string userIdStr)
        {
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Result.Fail(new BadRequestError("User ID is required"));

            Guid userId = Guid.Parse(userIdStr);

            var user = await _userRepository.GetUserById(userId);

            if (user == null)
                return Result.Fail(new NotFoundError("The user does not exist"));

            user.IsActive = !user.IsActive;

            await _userRepository.UpdateUserInfo(user);

            return Result.Ok();
        }
    }
}
