using JobMarket.Application.Common.Interfaces;
using JobMarket.Application.Features.Auth.DTOs;
using JobMarket.Domain.Exceptions;
using MediatR;

namespace JobMarket.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RefreshTokenHandler(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users
            .FindAsync(u => u.RefreshToken == request.RefreshToken, cancellationToken);

        var user = users.FirstOrDefault()
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!user.IsRefreshTokenValid(request.RefreshToken))
            throw new UnauthorizedException("Refresh token has expired.");

        string accessToken = _tokenService.GenerateAccessToken(user);
        string newRefreshToken = _tokenService.GenerateRefreshToken();
        DateTime refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken(newRefreshToken, refreshTokenExpiry);
        await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, newRefreshToken, refreshTokenExpiry);
    }
}
