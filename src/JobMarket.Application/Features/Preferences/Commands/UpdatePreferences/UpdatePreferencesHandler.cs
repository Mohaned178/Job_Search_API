using JobMarket.Application.Common.Interfaces;
using JobMarket.Domain.Entities;
using MediatR;

namespace JobMarket.Application.Features.Preferences.Commands.UpdatePreferences;

public class UpdatePreferencesHandler : IRequestHandler<UpdatePreferencesCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePreferencesHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        var existingPrefs = await _unitOfWork.UserPreferences
            .FindAsync(p => p.UserId == request.UserId, cancellationToken);

        UserPreferences preferences = existingPrefs.FirstOrDefault()
            ?? UserPreferences.Create(request.UserId);

        preferences.Update(
            request.Categories,
            request.PreferredLocations,
            request.PreferredJobType,
            request.PreferredSeniority);

        if (existingPrefs.Count == 0)
            await _unitOfWork.UserPreferences.AddAsync(preferences, cancellationToken);
        else
            await _unitOfWork.UserPreferences.UpdateAsync(preferences, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
