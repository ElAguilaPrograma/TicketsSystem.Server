using ClosedXML.Excel;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TicketsSystem.Core.DTOs.PaginationDTO;
using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Validations.UserValidations;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Core.Services
{

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserPasswordValidator _userPasswordValidator;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public UserService(
            IUserRepository userRepository,
            UserPasswordValidator passwordValidator,
            IPasswordHasher<User> passwordHasher,
            IConfiguration configuration,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _userRepository = userRepository;
            _userPasswordValidator = passwordValidator;
            _passwordHasher = passwordHasher;
            _config = configuration;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<PagedResult<UserReadDto>>> GetAllUsersWithFilterAsync(GetAllUsersFilterDto fiilterDto)
        {
            var validRoles = new[] { "All Roles", "User", "Admin", "Agent" };
            var validActiveFilters = new[] { "All", "Active", "Inactive" };

            if (!validRoles.Contains(fiilterDto.Role))
                return Result.Fail(new BadRequestError($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}"));
            if (!validActiveFilters.Contains(fiilterDto.IsActive))
                return Result.Fail(new BadRequestError($"Invalid active filter. Valid values are: {string.Join(", ", validActiveFilters)}"));

            bool? isActive = fiilterDto.IsActive switch
            {
                "All" => null,
                "Active" => true,
                "Inactive" => false,
                _ => null
            };

            var (users, totalCount) = await _userRepository.GetAllPaginatedWithFilters(
                fiilterDto.Page,
                fiilterDto.PageSize,
                fiilterDto.Role == "All Roles" ? null : fiilterDto.Role,
                isActive,
                fiilterDto.QuerySearch);

            var usersDTOs = users.Select(u => new UserReadDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            });

            var result = new PagedResult<UserReadDto>
            {
                Data = usersDTOs,
                TotalCount = totalCount,
                Page = fiilterDto.Page,
                PageSize = fiilterDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / fiilterDto.PageSize)
            };

            return Result.Ok(result).WithSuccess(new OkSuccess("Users retrieved successfully."));
        }

        public async Task<Result<UserReadDto>> GetUserById(string userIdStr)
        {
            Guid userId = Guid.Parse(userIdStr);
            var user = await _userRepository.GetById(userId);

            if (user == null)
                return Result.Fail(new NotFoundError("The user does not exist"));

            UserReadDto userReadDto = new UserReadDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Result.Ok(userReadDto).WithSuccess(new OkSuccess("User retrieved successfully."));
        }

        public async Task<Result> CreateNewUserAsync(UserCreateDto userCreateDto)
        {

            if (await _userRepository.EmailExist(userCreateDto.Email))
                return Result.Fail(new BadRequestError("The user already exist"));

            var newUser = new User
            {
                FullName = userCreateDto.FullName,
                Email = userCreateDto.Email,
                PasswordHash = userCreateDto.ConfirmPassword,
                Role = userCreateDto.Role,
                IsActive = userCreateDto.IsActive
            };

            newUser.PasswordHash = _passwordHasher.HashPassword(newUser, userCreateDto.Password);

            await _userRepository.Create(newUser);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new CreatedSuccess("User created successfully."));
        }

        public async Task<Result<LoginSuccessDto>> LoginAsync(LoginRequest request)
        {

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
            }).WithSuccess(new OkSuccess("Login successful."));
        }

        public async Task<Result> UpdateUserInformationAsync(UserUpdateDto userUpdateDto, string userIdStr)
        {
            Guid userId = Guid.Parse(userIdStr);

            var user = await _userRepository.GetById(userId);
            if (user == null)
                return Result.Fail(new NotFoundError("The user does not exist"));

            if (!string.IsNullOrWhiteSpace(userUpdateDto.Password) && !string.IsNullOrWhiteSpace(userUpdateDto.ConfirmPassword))
            {
                var validationPasswordResult = await _userPasswordValidator.ValidateAsync(userUpdateDto);
                if (!validationPasswordResult.IsValid)
                {
                    var errorMessages = validationPasswordResult.Errors.Select(e => new BadRequestError(e.ErrorMessage));
                    return Result.Fail(errorMessages);
                }
                else
                {
                    var verification = _passwordHasher.VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        userUpdateDto.Password
                    );

                    if (verification == PasswordVerificationResult.Success)
                        return Result.Fail(new BadRequestError("The new password cannot be the same as the current password."));
                    user.PasswordHash = _passwordHasher.HashPassword(user, userUpdateDto.Password);
                }
            }

            user.FullName = userUpdateDto.FullName;
            user.Email = userUpdateDto.Email;
            user.Role = userUpdateDto.Role;
            user.IsActive = userUpdateDto.IsActive;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("User information updated successfully."));
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

        public async Task<Result> DeactivateOrActivateAUserAsync(string userIdStr)
        {
            if (string.IsNullOrWhiteSpace(userIdStr))
                return Result.Fail(new BadRequestError("User ID is required"));

            Guid userId = Guid.Parse(userIdStr);

            var user = await _userRepository.GetById(userId);

            if (user == null)
                return Result.Fail(new NotFoundError("The user does not exist"));

            if (user.UserId == _currentUserService.GetCurrentUserId())
                return Result.Fail(new BadRequestError("An administrator cannot deactivate themselves."));

            user.IsActive = !user.IsActive;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return Result.Ok().WithSuccess(new OkSuccess("User status toggled successfully."));
        }

        public async Task<Result<CurrentUserDto>> GetCurrentUser()
        {
            var userId = _currentUserService.GetCurrentUserId();
            var email = _currentUserService.GetCurrentUserEmail();
            var role = _currentUserService.GetCurrentUserRole();
            var user = await _userRepository.GetById(userId);
            var fullName = user!.FullName;

            var currentUserClaimData = new CurrentUserDto()
            {
                UserId = userId,
                Email = email,
                Role = role,
                FullName = fullName
            };

            return Result.Ok(currentUserClaimData).WithSuccess(new OkSuccess("Current user retrieved successfully."));
        }

        public async Task<Result<UserCountDto>> GetUsersCount()
        {
            var users = await _userRepository.GetAll();

            var usersCount = new UserCountDto
            {
                TotalUsers = users.Count(),
                Users = users.Where(u => u.Role == "User").Count(),
                Admins = users.Where(u =>  u.Role == "Admin").Count(),
                Agents = users.Where(u => u.Role == "Agent").Count()
            };

            return Result.Ok(usersCount).WithSuccess(new OkSuccess("User count retrieved successfully."));
        }

        public async Task<Result<byte[]>> ExportUsersAsync(FilterUsersDto filterUsersDto)
        {
            var validRoles = new[] { "All Roles", "User", "Admin", "Agent" };
            var validActiveFilters = new[] { "All", "Active", "Inactive" };

            if (!validRoles.Contains(filterUsersDto.Role))
                return Result.Fail(new BadRequestError($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}"));
            if (!validActiveFilters.Contains(filterUsersDto.IsActive))
                return Result.Fail(new BadRequestError($"Invalid active filter. Valid values are: {string.Join(", ", validActiveFilters)}"));

            bool? isActive = filterUsersDto.IsActive switch
            {
                "All" => null,
                "Active" => true,
                "Inactive" => false,
                _ => null
            };

            var filterUsers = await _userRepository.ExportUsersWithFilters(filterUsersDto.Role, isActive);
            var users = filterUsers.Select(u => new
            {
                UserId = u.UserId.ToString(),
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive ? "True" : "False",
                CreatedAt = u.CreatedAt.AddMinutes(-filterUsersDto.TimezoneOffsetMinutes)
            });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Users");

                worksheet.Cell(1, 1).Value = "UserId";
                worksheet.Cell(1, 2).Value = "Full Name";
                worksheet.Cell(1, 3).Value = "Email";
                worksheet.Cell(1, 4).Value = "Role";
                worksheet.Cell(1, 5).Value = "Is Active";
                worksheet.Cell(1, 6).Value = "Created At";

                worksheet.Cell(2, 1).InsertData(users);

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                return Result.Ok(stream.ToArray()).WithSuccess(new OkSuccess("Users exported successfully."));
            }
        }
    }
}
