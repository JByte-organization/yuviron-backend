using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Genres.Commands.UpdateGenre;

public sealed class UpdateGenreHandler : IRequestHandler<UpdateGenreCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateGenreHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateGenreCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var genre = await _context.Genres
            .FirstOrDefaultAsync(g => g.Id == request.GenreId, cancellationToken);

        if (genre == null)
        {
            throw new NotFoundException(nameof(Genre), request.GenreId);
        }

        if (!genre.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            var isDuplicate = await _context.Genres
                .AnyAsync(g => g.Name == request.Name , cancellationToken);

            if (isDuplicate)
            {
                throw new InvalidOperationException($"Genre '{request.Name}' already exists.");
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        string? finalCoverUrl = genre.CoverUrl; 
        
        if (request.CoverFileId.HasValue)
        {
            var coverClaim = await _context.ClaimFileAsync(
                request.CoverFileId.Value, adminId, "image/", "covers", cancellationToken);
            
            genre.RegisterFileSwapEvents(coverClaim, genre.CoverUrl);
            finalCoverUrl = coverClaim.FinalPath;
        }
        
        genre.Update(request.Name, finalCoverUrl, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}