using FluentAssertions;
using Moq;
using TicketsSystem.Core.Services;
using TicketsSystem.Domain.Entities;
using TicketsSystem.Domain.Interfaces;

namespace TicketsSystem.Tests.Services;

public class GetUserRoleServiceTests
{
	private readonly Mock<IUserRepository> _userRepository = new();

	[Fact]
	public async Task UserIsAdmin_ReturnsTrue_WhenUserRoleIsAdmin()
	{
		var userId = Guid.NewGuid();
		_userRepository
			.Setup(x => x.GetById(userId))
			.ReturnsAsync(new User { UserId = userId, Role = "Admin" });

		var sut = new GetUserRoleService(_userRepository.Object);

		var result = await sut.UserIsAdmin(userId);

		result.Should().BeTrue();
	}

	[Fact]
	public async Task UserIsAgent_ReturnsFalse_WhenUserDoesNotExist()
	{
		var userId = Guid.NewGuid();
		_userRepository.Setup(x => x.GetById(userId)).ReturnsAsync((User?)null);

		var sut = new GetUserRoleService(_userRepository.Object);

		var result = await sut.UserIsAgent(userId);

		result.Should().BeFalse();
	}

	[Fact]
	public async Task UserIsUser_ReturnsFalse_WhenRoleIsDifferent()
	{
		var userId = Guid.NewGuid();
		_userRepository
			.Setup(x => x.GetById(userId))
			.ReturnsAsync(new User { UserId = userId, Role = "Agent" });

		var sut = new GetUserRoleService(_userRepository.Object);

		var result = await sut.UserIsUser(userId);

		result.Should().BeFalse();
	}
}