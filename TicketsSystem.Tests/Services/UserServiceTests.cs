using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using TicketsSystem.Core.DTOs.UserDTO;
using TicketsSystem.Core.Errors;
using TicketsSystem.Core.Interfaces;
using TicketsSystem.Core.Services;
using TicketsSystem.Core.Validations.UserValidations;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher<User>> _passwordHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();

    private static IConfiguration BuildConfig()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = "ThisIsATestKeyWithEnoughLength123456",
            ["Jwt:Issuer"] = "TicketsSystem.Tests",
            ["Jwt:Audience"] = "TicketsSystem.Client"
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    private UserService BuildSut()
    {
        return new UserService(
            _userRepository.Object,
            new UserPasswordValidator(),
            _passwordHasher.Object,
            BuildConfig(),
            _unitOfWork.Object,
            _currentUserService.Object);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenUserDoesNotExist()
    {
        var request = new LoginRequest { Email = "missing@test.com", Password = "Password123" };
        _userRepository.Setup(x => x.Login(request.Email)).ReturnsAsync((User?)null);

        var sut = BuildSut();

        var result = await sut.LoginAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);
    }

    [Fact]
    public async Task LoginAsync_ReturnsForbidden_WhenUserIsDeactivated()
    {
        var request = new LoginRequest { Email = "inactive@test.com", Password = "Password123" };
        var user = BuildUser(isActive: false);
        _userRepository.Setup(x => x.Login(request.Email)).ReturnsAsync(user);

        var sut = BuildSut();

        var result = await sut.LoginAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ForbiddenError);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        var request = new LoginRequest { Email = "user@test.com", Password = "WrongPassword" };
        var user = BuildUser();

        _userRepository.Setup(x => x.Login(request.Email)).ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            .Returns(PasswordVerificationResult.Failed);

        var sut = BuildSut();

        var result = await sut.LoginAsync(request);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);
    }

    [Fact]
    public async Task LoginAsync_ReturnsJwtTokenWithClaims_WhenCredentialsAreValid()
    {
        var request = new LoginRequest { Email = "agent@test.com", Password = "StrongPass123" };
        var userId = Guid.NewGuid();
        var user = BuildUser(userId: userId, email: request.Email, role: "Agent", isActive: true);

        _userRepository.Setup(x => x.Login(request.Email)).ReturnsAsync(user);
        _passwordHasher
            .Setup(x => x.VerifyHashedPassword(user, user.PasswordHash, request.Password))
            .Returns(PasswordVerificationResult.Success);

        var sut = BuildSut();

        var result = await sut.LoginAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotBeNullOrWhiteSpace();
        result.Value.Expiration.Should().NotBeNull();

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Value.Token!);
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == request.Email);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Agent");
    }

    [Fact]
    public async Task DeactivateOrActivateAUserAsync_ReturnsBadRequest_WhenUserIdIsEmpty()
    {
        var sut = BuildSut();

        var result = await sut.DeactivateOrActivateAUserAsync(string.Empty);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
    }

    [Fact]
    public async Task DeactivateOrActivateAUserAsync_ReturnsBadRequest_WhenTryingToToggleSelf()
    {
        var userId = Guid.NewGuid();
        var user = BuildUser(userId: userId, role: "Admin", isActive: true);

        _userRepository.Setup(x => x.GetById(userId)).ReturnsAsync(user);
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(userId);

        var sut = BuildSut();

        var result = await sut.DeactivateOrActivateAUserAsync(userId.ToString());

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is BadRequestError);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task DeactivateOrActivateAUserAsync_TogglesStatusAndSaves_WhenValidUser()
    {
        var currentAdminId = Guid.NewGuid();
        var targetUserId = Guid.NewGuid();
        var user = BuildUser(userId: targetUserId, role: "User", isActive: true);

        _userRepository.Setup(x => x.GetById(targetUserId)).ReturnsAsync(user);
        _currentUserService.Setup(x => x.GetCurrentUserId()).Returns(currentAdminId);

        var sut = BuildSut();

        var result = await sut.DeactivateOrActivateAUserAsync(targetUserId.ToString());

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
        _userRepository.Verify(x => x.Update(user), Times.Once);
        _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateOrActivateAUserAsync_ThrowsFormatException_WhenUserIdIsInvalidGuid()
    {
        var sut = BuildSut();

        var action = async () => await sut.DeactivateOrActivateAUserAsync("bad-guid");

        await action.Should().ThrowAsync<FormatException>();
    }

    private static User BuildUser(Guid? userId = null, string email = "user@test.com", string role = "User", bool isActive = true)
    {
        return new User
        {
            UserId = userId ?? Guid.NewGuid(),
            FullName = "Test User",
            Email = email,
            PasswordHash = "hashed-password",
            Role = role,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };
    }
}
