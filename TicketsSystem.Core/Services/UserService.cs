using Microsoft.AspNetCore.Identity;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using FluentResults;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Validations.UserValidations;
using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Core.Interfaces;

namespace TicketsSystem.Core.Services
{

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserCreateValidator _userCreateValidator;
        private readonly UserUpdateValidator _userUpdateValidator;
        private readonly LoginRequestValidation _loginRequestValidation;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;
        public UserService(
            IUserRepository userRepository,
            UserCreateValidator validationCreateRules,
            UserUpdateValidator updateUserUpdateRules,
            IPasswordHasher<User> passwordHasher,
            LoginRequestValidation loginValidationsRules,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _userCreateValidator = validationCreateRules;
            _userUpdateValidator = updateUserUpdateRules;
            _passwordHasher = passwordHasher;
            _loginRequestValidation = loginValidationsRules;
            _config = configuration;
        }

        public async Task<Result<IEnumerable<UserReadDto>>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAll();
            IEnumerable<UserReadDto> userDTOs = users.Select(u => new UserReadDto
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

        public async Task<Result> CreateNewUserAsync(UserCreateDto userCreateDto)
        {
            if (userCreateDto == null)
                return Result.Fail(new BadRequestError("userDto is required"));

            var validationResult = await _userCreateValidator.ValidateAsync(userCreateDto);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessages);
            }

            if (await _userRepository.EmailExist(userCreateDto.Email))
                return Result.Fail(new BadRequestError("The user already exist"));

            var newUser = new User
            {
                FullName = userCreateDto.FullName,
                Email = userCreateDto.Email,
                PasswordHash = userCreateDto.Password,
                Role = userCreateDto.Role,
                IsActive = userCreateDto.IsActive
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, userCreateDto.Password);

            await _userRepository.Create(newUser);

            return Result.Ok();
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

        public async Task<Result> UpdateUserInformationAsync(UserUpdateDto userUpdateDto, string userIdStr)
        {
            Guid userId = Guid.Parse(userIdStr);

            if (userUpdateDto == null)
                return Result.Fail(new BadRequestError("UserDTO is required"));

            var validationResult = await _userUpdateValidator.ValidateAsync(userUpdateDto);
            if (!validationResult.IsValid)
            {
                var errorMessages = validationResult.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                return Result.Fail(errorMessages);
            }

            var user = await _userRepository.GetById(userId);
            if (user == null)
                return Result.Fail(new NotFoundError("The user does not exist"));

            user.FullName = userUpdateDto.FullName;
            user.Email = userUpdateDto.Email;
            user.Role = userUpdateDto.Role;
            user.IsActive = userUpdateDto.IsActive;
            user.PasswordHash = _passwordHasher.HashPassword(user, userUpdateDto.Password);

            await _userRepository.Update(user);

            return Result.Ok();
        }

        private string GenereteJwtToken(User user, DateTime expiration)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<Result<IEnumerable<UserReadDto>>> SearchUserAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Result.Fail(new BadRequestError("Query format is not valid"));

            query = query.ToLower();

            var user = await _userRepository.SearchUsers(query);

            IEnumerable<UserReadDto> userDTO = user.Select(u => new UserReadDto
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

            var user = await _userRepository.GetById(userId);

            if (user == null)
                return Result.Fail(new NotFoundError("The user does not exist"));

            user.IsActive = !user.IsActive;

            await _userRepository.Update(user);

            return Result.Ok();
        }
    }
}
