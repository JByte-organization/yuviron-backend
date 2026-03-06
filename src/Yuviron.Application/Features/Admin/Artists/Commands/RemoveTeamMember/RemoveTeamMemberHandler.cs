using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.RemoveTeamMember;

public sealed class RemoveTeamMemberHandler : IRequestHandler<RemoveTeamMemberCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public RemoveTeamMemberHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(RemoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        artist.RemoveTeamMember(request.UserId, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}