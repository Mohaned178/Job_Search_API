using FluentAssertions;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Auth.Commands.Login;
using JobMarket.Application.Features.Auth.DTOs;
using JobMarket.Domain.Entities;
using JobMarket.Domain.Exceptions;
using Moq;

namespace JobMarket.Application.Tests.Handlers;

public class LoginHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRepository<User>> _userRepoMock = new();

    private LoginHandler CreateHandler()
    {
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        return new LoginHandler(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsAuthResponse()
    {
        var user = User.Create("test@example.com", "hashed");

        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(new List<User> { user });
        _passwordHasherMock.Setup(h => h.Verify("Password123!", "hashed")).Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("access_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh_token");
        _userRepoMock.Setup(r => r.UpdateAsync(user, default)).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(new LoginCommand("test@example.com", "Password123!"), default);

        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsDomainException()
    {
        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(new List<User>());

        var handler = CreateHandler();
        Func<Task> act = () => handler.Handle(new LoginCommand("unknown@example.com", "pass"), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Invalid*");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsDomainException()
    {
        var user = User.Create("test@example.com", "hashed");

        _userRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(new List<User> { user });
        _passwordHasherMock.Setup(h => h.Verify("wrong", "hashed")).Returns(false);

        var handler = CreateHandler();
        Func<Task> act = () => handler.Handle(new LoginCommand("test@example.com", "wrong"), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Invalid*");
    }
}
