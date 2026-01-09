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


namespace TicketsSystem.Core.Services
{
    public interface IUserService
    {
        Task<UserResponse> CreateNewUserAsync(UserDTO userDTO);
        Task<UserResponse> GetAllUsersAsync();
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserDTOValidator _userValidation;
        private readonly LoginRequestValidation _loginRequestValidation;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;
        public UserService(
            IUserRepository userRepository,
            UserDTOValidator validationRules,
            IPasswordHasher<User> passwordHasher,
            LoginRequestValidation loginValidationsRules,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userValidation = validationRules;
            _passwordHasher = passwordHasher;
            _loginRequestValidation = loginValidationsRules;
            _config = configuration;
        }

        public async Task<UserResponse> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllUsers();
                IEnumerable<UserDTO> userDTOs = users.Select(u => new UserDTO
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    Password = " ",
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                });

                return new UserResponse
                {
                    Success = true,
                    Message = "Users successfully recovered.",
                    Users = userDTOs
                };
            }
            catch (Exception)
            {
                return new UserResponse
                {
                    Success = false,
                    Message = "There was a problem recovering the users."
                };
            }

        }

        public async Task<UserResponse> CreateNewUserAsync(UserDTO userDTO)
        {
            if (userDTO == null)
            {
                throw new ArgumentNullException("userDto is null");
            }
            
            var validationResult = await _userValidation.ValidateAsync(userDTO);
            if (!validationResult.IsValid)
            {
                // throw new ValidationException("One or more fields do not meet the requirements." + validationResult.Errors);
                return new UserResponse
                {
                    Success = false,
                    Message = "The email or password format is invalid."
                };
            }

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

            return new UserResponse
            {
                Success = true,
                Message = "User created successfully."
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("Email or password are null");
            }

            var validationResult = await _loginRequestValidation.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "The email or password format is invalid."
                };
            }

            var user = await _userRepository.Login(request.Email);

            if (user == null)
            {
                return new LoginResponse { Success = false, Message = "Incorrect credentials" };
            }

            var verificationPassword = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verificationPassword != PasswordVerificationResult.Success)
            {
                return new LoginResponse { Success = false, Message = "Incorrect credentials" };
            }

            var tokenExpiration = DateTime.UtcNow.AddDays(7);
            var token = GenereteJwtToken(user, tokenExpiration);

            return new LoginResponse
            {
                Success = true,
                Token = token,
                Message = "Login success",
                Expiration = tokenExpiration
            };
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

    }
}
