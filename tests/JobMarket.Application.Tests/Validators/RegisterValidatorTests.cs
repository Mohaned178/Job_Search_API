using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using JobMarket.Application.Features.Auth.Commands.Register;

namespace JobMarket.Application.Tests.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Theory]
    [InlineData("user@example.com", "Password123!", true)]
    [InlineData("", "Password123!", false)]
    [InlineData("not-an-email", "Password123!", false)]
    [InlineData("user@example.com", "", false)]
    [InlineData("user@example.com", "short", false)]
    public void Validate_ReturnsExpectedResult(string email, string password, bool shouldBeValid)
    {
        var command = new RegisterCommand(email, password);

        ValidationResult result = _validator.Validate(command);

        result.IsValid.Should().Be(shouldBeValid);
    }

    [Fact]
    public void Validate_WithPasswordTooShort_HasPasswordError()
    {
        var command = new RegisterCommand("user@example.com", "abc123");

        ValidationResult result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WithInvalidEmail_HasEmailError()
    {
        var command = new RegisterCommand("bad-email", "Password123!");

        ValidationResult result = _validator.Validate(command);

        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }
}
