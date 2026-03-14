using JobMarket.Application.Features.Auth.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponse>;
