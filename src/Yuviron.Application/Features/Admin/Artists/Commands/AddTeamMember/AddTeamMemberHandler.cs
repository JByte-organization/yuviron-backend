using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;

public sealed class AddTeamMemberHandler : IRequestHandler<AddTeamMemberCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public AddTeamMemberHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == request.UserId, cancellationToken);
            
        if (!userExists) throw new NotFoundException(nameof(User), request.UserId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        artist.AddTeamMember(request.UserId, request.Role, utcNow);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}