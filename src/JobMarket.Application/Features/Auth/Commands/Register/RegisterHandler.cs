using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Auth.DTOs;
using JobMarket.Domain.Entities;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        bool emailExists = await _unitOfWork.Users
            .ExistsAsync(u => u.Email == request.Email, cancellationToken);

        if (emailExists)
            throw new DomainException("A user with this email already exists.");

        string hashedPassword = _passwordHasher.Hash(request.Password);
        User user = User.Create(request.Email, hashedPassword);

        string accessToken = _tokenService.GenerateAccessToken(user);
        (string refreshToken, DateTime refreshTokenExpiry) = _tokenService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, refreshTokenExpiry);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken, refreshTokenExpiry);
    }
}
