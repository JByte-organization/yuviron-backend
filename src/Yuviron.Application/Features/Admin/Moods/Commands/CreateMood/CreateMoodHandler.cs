using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities; 

namespace Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;

public sealed class CreateMoodHandler : IRequestHandler<CreateMoodCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreateMoodHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateMoodCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (await _context.Moods.AnyAsync(m => m.Name == request.Name , cancellationToken))
        {
            throw new InvalidOperationException($"Mood with name '{request.Name}' already exists.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
        }

        var mood = Mood.Create(
            request.Name,
            coverClaim?.FinalPath, 
            utcNow
        );

        if (coverClaim != null)
        {
            mood.RegisterFileSwapEvents(coverClaim);
        }

        _context.Moods.Add(mood);
        await _context.SaveChangesAsync(cancellationToken);

        return mood.Id;
    }
}