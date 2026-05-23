using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;

public sealed class CreateGenreHandler : IRequestHandler<CreateGenreCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreateGenreHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateGenreCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        if (await _context.Genres.AnyAsync(g => g.Name == request.Name, cancellationToken))
            throw new InvalidOperationException($"Genre '{request.Name}' already exists.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        ClaimedFileResult? coverClaim = null;
        if (request.CoverFileId.HasValue)
        {
            coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
        }

        var genre = Genre.Create(request.Name, coverClaim?.FinalPath, utcNow); 

        if (coverClaim != null) genre.RegisterFileSwapEvents(coverClaim);

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync(cancellationToken);

        return genre.Id;
    }
}