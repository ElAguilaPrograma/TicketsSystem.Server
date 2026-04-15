using FluentAssertions;
using TicketsSystem.Core.DTOs.TicketsDTO;
using TicketsSystem.Core.Validations.TicketsValidations;

namespace TicketsSystem.Tests.Validations;

public class TicketsCreateValidatorTests
{
	private readonly TicketsCreateValidator _validator = new();

	[Fact]
	public void Validate_ReturnsSuccess_WhenDtoIsValid()
	{
		var dto = new TicketsCreateDto
		{
			Title = "VPN not connecting",
			Description = "Remote access fails after login",
			PriorityId = 2
		};

		var result = _validator.Validate(dto);

		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_ReturnsError_WhenTitleIsEmpty()
	{
		var dto = new TicketsCreateDto
		{
			Title = string.Empty,
			Description = "Issue description",
			PriorityId = 2
		};

		var result = _validator.Validate(dto);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == nameof(TicketsCreateDto.Title));
	}

	[Fact]
	public void Validate_ReturnsError_WhenPriorityIsOutOfRange()
	{
		var dto = new TicketsCreateDto
		{
			Title = "Cannot open app",
			Description = "The desktop app crashes on start",
			PriorityId = 99
		};

		var result = _validator.Validate(dto);

		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(e => e.PropertyName == nameof(TicketsCreateDto.PriorityId));
	}
}