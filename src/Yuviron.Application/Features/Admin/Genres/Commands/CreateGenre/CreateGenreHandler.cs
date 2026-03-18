using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; // <-- ДОБАВИЛИ
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Admin.Genres.Commands.CreateGenre;

public sealed class CreateGenreHandler : IRequestHandler<CreateGenreCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService;

    public CreateGenreHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) 
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> Handle(CreateGenreCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Genres.AnyAsync(g => g.Name == request.Name, cancellationToken))
            throw new InvalidOperationException($"Genre '{request.Name}' already exists.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalCoverUrl = await _fileStorageService.MoveIfTempAsync(request.CoverUrl, "covers", cancellationToken);

        var genre = Genre.Create(request.Name, finalCoverUrl, utcNow); 

        _context.Genres.Add(genre);
        await _context.SaveChangesAsync(cancellationToken);

        return genre.Id;
    }
}