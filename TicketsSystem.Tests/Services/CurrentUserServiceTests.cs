using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using TicketsSystem.Core.Services;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Tests.Services;

public class CurrentUserServiceTests
{
	private readonly Mock<IUserRepository> _userRepository = new();

	private static IHttpContextAccessor BuildContextAccessor(params Claim[] claims)
	{
		var context = new DefaultHttpContext
		{
			User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
		};

		return new HttpContextAccessor { HttpContext = context };
	}

	[Fact]
	public void GetCurrentUserId_ReturnsNameIdentifier_WhenPresent()
	{
		var userId = Guid.NewGuid();
		var accessor = BuildContextAccessor(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
		var sut = new CurrentUserService(accessor, _userRepository.Object);

		var result = sut.GetCurrentUserId();

		result.Should().Be(userId);
	}

	[Fact]
	public void GetCurrentUserId_ReturnsSubClaim_WhenNameIdentifierIsMissing()
	{
		var userId = Guid.NewGuid();
		var accessor = BuildContextAccessor(new Claim("sub", userId.ToString()));
		var sut = new CurrentUserService(accessor, _userRepository.Object);

		var result = sut.GetCurrentUserId();

		result.Should().Be(userId);
	}

	[Fact]
	public void GetCurrentUserId_ThrowsUnauthorized_WhenNoValidClaimExists()
	{
		var accessor = BuildContextAccessor(new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));
		var sut = new CurrentUserService(accessor, _userRepository.Object);

		var action = () => sut.GetCurrentUserId();

		action.Should().Throw<UnauthorizedAccessException>();
	}

	[Fact]
	public void GetCurrentUserEmail_ReturnsEmailClaim()
	{
		var accessor = BuildContextAccessor(new Claim(ClaimTypes.Email, "user@test.com"));
		var sut = new CurrentUserService(accessor, _userRepository.Object);

		var result = sut.GetCurrentUserEmail();

		result.Should().Be("user@test.com");
	}

	[Fact]
	public void GetCurrentUserRole_ReturnsRoleClaim()
	{
		var accessor = BuildContextAccessor(new Claim(ClaimTypes.Role, "Admin"));
		var sut = new CurrentUserService(accessor, _userRepository.Object);

		var result = sut.GetCurrentUserRole();

		result.Should().Be("Admin");
	}

	[Fact]
	public async Task GetCurrentUserName_ReturnsUserFullName_FromRepository()
	{
		var userId = Guid.NewGuid();
		var accessor = BuildContextAccessor(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
		_userRepository
			.Setup(x => x.GetById(userId))
			.ReturnsAsync(new User
			{
				UserId = userId,
				FullName = "Jane Doe",
				Email = "jane@test.com",
				PasswordHash = "hash",
				Role = "User"
			});

		var sut = new CurrentUserService(accessor, _userRepository.Object);

		var result = await sut.GetCurrentUserName();

		result.Should().Be("Jane Doe");
	}
}