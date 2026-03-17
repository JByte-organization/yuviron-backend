using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services; // <-- Добавили
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;

public sealed class UpdateMoodHandler : IRequestHandler<UpdateMoodCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IFileStorageService _fileStorageService; // <-- Добавили

    public UpdateMoodHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _timeProvider = timeProvider;
        _fileStorageService = fileStorageService;
    }

    public async Task<Unit> Handle(UpdateMoodCommand request, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods
                       .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Mood), request.Id);

        if (!mood.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _context.Moods.AnyAsync(m => m.Name == request.Name, cancellationToken))
                throw new InvalidOperationException($"Mood with name '{request.Name}' already exists.");
        }

        var oldCoverUrl = mood.CoverUrl;
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        mood.Update(request.Name, request.CoverUrl, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        if (!string.Equals(oldCoverUrl, request.CoverUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldCoverUrl))
        {
            await _fileStorageService.DeleteAsync(oldCoverUrl, cancellationToken);
        }

        return Unit.Value;
    }
}