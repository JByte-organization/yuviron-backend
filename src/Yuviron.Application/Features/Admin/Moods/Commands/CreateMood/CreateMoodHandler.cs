using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; // <-- Добавили
using Yuviron.Domain.Entities; 

namespace Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;

public sealed class CreateMoodHandler : IRequestHandler<CreateMoodCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; // <-- Добавили

    public CreateMoodHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService) // <-- Добавили
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Guid> Handle(CreateMoodCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Moods.AnyAsync(m => m.Name == request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Mood with name '{request.Name}' already exists.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalCoverUrl = await _fileStorageService.MoveIfTempAsync(request.CoverUrl, "covers", cancellationToken);

        var mood = Mood.Create(
            request.Name,
            finalCoverUrl, 
            utcNow
        );

        _context.Moods.Add(mood);
        await _context.SaveChangesAsync(cancellationToken);

        return mood.Id;
    }
}