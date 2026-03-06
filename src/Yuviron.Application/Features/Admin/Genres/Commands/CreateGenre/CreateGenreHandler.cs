using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;

public sealed class CreateGenreHandler : IRequestHandler<CreateGenreCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateGenreHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateGenreCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Genres.AnyAsync(g => g.Name == request.Name, cancellationToken))
            throw new InvalidOperationException($"Genre '{request.Name}' already exists.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var genre = Genre.Create(request.Name, request.CoverUrl, utcNow);

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync(cancellationToken);

        return genre.Id;
    }
}