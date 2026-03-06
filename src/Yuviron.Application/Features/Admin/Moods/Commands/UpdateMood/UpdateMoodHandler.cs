using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities; // Добавлено
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Moods.Commands.UpdateMood;

public sealed class UpdateMoodHandler : IRequestHandler<UpdateMoodCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateMoodHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateMoodCommand request, CancellationToken cancellationToken)
    {
        var mood = await _context.Moods
                       .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
                   ?? throw new NotFoundException(nameof(Mood), request.Id);

        if (mood.Name != request.Name && 
            await _context.Moods.AnyAsync(m => m.Name == request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Mood with name '{request.Name}' already exists.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        mood.Update(request.Name, request.CoverUrl, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}