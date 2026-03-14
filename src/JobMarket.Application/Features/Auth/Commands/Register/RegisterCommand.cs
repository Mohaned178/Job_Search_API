using JobMarket.Application.Features.Auth.DTOs;
using MediatR;

namespace JobMarket.Application.Features.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<AuthResponse>;
