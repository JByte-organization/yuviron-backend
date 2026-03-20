using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;

public sealed class CreateGenreHandler : IRequestHandler<CreateGenreCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateGenreHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider) 
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateGenreCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Genres.AnyAsync(g => g.Name == request.Name, cancellationToken))
            throw new InvalidOperationException($"Genre '{request.Name}' already exists.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalCoverUrl = FileStorageExtensions.PredictDestinationPath(request.CoverUrl, "covers");

        var genre = Genre.Create(request.Name, finalCoverUrl, utcNow); 

        if (!string.IsNullOrWhiteSpace(request.CoverUrl) && request.CoverUrl.StartsWith("temp/"))
        {
            genre.AddDomainEvent(new TempFileNeedsMovingEvent(request.CoverUrl, "covers"));
        }

        _context.Genres.Add(genre);
        
        await _context.SaveChangesAsync(cancellationToken);

        return genre.Id;
    }
}