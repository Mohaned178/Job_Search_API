using FluentAssertions;
using JobMarket.Domain.Entities;
using JobMarket.Domain.Enums;
using JobMarket.Domain.Exceptions;

namespace JobMarket.Application.Tests.Domain;

public class UserEntityTests
{
    [Fact]
    public void Create_WithValidData_ReturnsUser()
    {
        var user = User.Create("john@example.com", "hashed_password");

        user.Email.Should().Be("john@example.com");
        user.Role.Should().Be(UserRole.JobSeeker);
    }

    [Fact]
    public void Create_WithEmailAndHash_StoresPasswordHash()
    {
        var user = User.Create("john@example.com", "bcrypt_hash");

        user.PasswordHash.Should().Be("bcrypt_hash");
        user.RefreshToken.Should().BeNull();
    }

    [Fact]
    public void SetRefreshToken_SetsToken()
    {
        var user = User.Create("john@example.com", "hash");
        var expiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken("new_token", expiry);

        user.RefreshToken.Should().Be("new_token");
        user.RefreshTokenExpiry.Should().BeCloseTo(expiry, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("correct_token", true)]
    [InlineData("wrong_token", false)]
    public void IsRefreshTokenValid_ReturnsExpectedResult(string tokenToCheck, bool expected)
    {
        var user = User.Create("john@example.com", "hash");
        user.SetRefreshToken("correct_token", DateTime.UtcNow.AddDays(7));

        bool result = user.IsRefreshTokenValid(tokenToCheck);

        result.Should().Be(expected);
    }
}

