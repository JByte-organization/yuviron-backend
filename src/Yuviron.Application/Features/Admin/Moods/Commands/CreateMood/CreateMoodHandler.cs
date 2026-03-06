using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities; // Добавлено, чтобы исправить "Cannot resolve symbol 'Mood'"

namespace Yuviron.Application.Features.Admin.Moods.Commands.CreateMood;

public sealed class CreateMoodHandler : IRequestHandler<CreateMoodCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateMoodHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateMoodCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Moods.AnyAsync(m => m.Name == request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Mood with name '{request.Name}' already exists.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var mood = Mood.Create(
            request.Name,
            request.CoverUrl,
            utcNow
        );

        _context.Moods.Add(mood);
        await _context.SaveChangesAsync(cancellationToken);

        return mood.Id;
    }
}