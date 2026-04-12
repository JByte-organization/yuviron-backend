using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;

public sealed class AddTeamMemberHandler : IRequestHandler<AddTeamMemberCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IIdentityManager _identityManager;

    public AddTeamMemberHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        IIdentityManager identityManager)
    {
        _context = context;
        _timeProvider = timeProvider;
        _identityManager = identityManager;
    }

    public async Task<Unit> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var artist = await _context.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        artist.AddTeamMember(request.UserId, request.Role, utcNow);

        await _identityManager.EnsureManagementRoleAsync(request.UserId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}