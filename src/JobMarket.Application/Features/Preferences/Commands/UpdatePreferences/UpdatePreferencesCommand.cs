using JobMarket.Domain.Enums;
using MediatR;

namespace JobMarket.Application.Features.Preferences.Commands.UpdatePreferences;

public record UpdatePreferencesCommand(
    Guid UserId,
    List<string> Categories,
    List<string> PreferredLocations,
    JobType? PreferredJobType,
    SeniorityLevel? PreferredSeniority) : IRequest<Unit>;
