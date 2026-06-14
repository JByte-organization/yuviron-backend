using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;

public sealed class UpdateMoodHandler : IRequestHandler<UpdateMoodCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateMoodHandler(
        ICatalogContext catalogContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateMoodCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var mood = await _catalogContext.Moods
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (mood == null)
        {
            throw new NotFoundException(nameof(Mood), request.Id);
        }

        if (!mood.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var isDuplicate = await _catalogContext.Moods
                .AnyAsync(m => m.Name == request.Name , cancellationToken);

            if (isDuplicate)
            {
                throw new InvalidOperationException($"Mood with name '{request.Name}' already exists.");
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        string? finalCoverUrl = mood.CoverUrl;

        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _systemContext.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
                
            mood.RegisterFileSwapEvents(coverClaim, mood.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }

        mood.Update(request.Name, finalCoverUrl, utcNow); 

        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}