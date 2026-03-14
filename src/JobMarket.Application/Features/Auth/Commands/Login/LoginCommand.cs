using JobMarket.Application.Features.Auth.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
