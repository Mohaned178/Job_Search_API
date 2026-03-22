using FluentAssertions;
using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Auth.Commands.Register;
using JobMarket.Application.Features.Auth.DTOs;
using JobMarket.Domain.Entities;
using Moq;

namespace JobMarket.Application.Tests.Handlers;

public class RegisterHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRepository<User>> _userRepoMock = new();

    private RegisterHandler CreateHandler()
    {
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        return new RegisterHandler(
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithNewEmail_ReturnsAuthResponse()
    {
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.Hash("Password123!")).Returns("hashed");
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("access_token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(("refresh_token", refreshExpiry));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = CreateHandler();
        var result = await handler.Handle(new RegisterCommand("new@example.com", "Password123!"), default);

        result.Should().BeOfType<AuthResponse>();
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ThrowsDomainException()
    {
        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(true);

        var handler = CreateHandler();
        Func<Task> act = () => handler.Handle(new RegisterCommand("existing@example.com", "Password123!"), default);

        await act.Should().ThrowAsync<JobMarket.Domain.Exceptions.DomainException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_WithNewEmail_CallsPasswordHasher()
    {
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        _userRepoMock.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>(), default))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>())).Returns("token");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns(("refresh", refreshExpiry));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = CreateHandler();
        await handler.Handle(new RegisterCommand("test@example.com", "password"), default);

        _passwordHasherMock.Verify(h => h.Hash("password"), Times.Once);
    }
}
