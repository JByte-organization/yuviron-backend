using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Artists.Commands.AddTeamMember;

public sealed class AddTeamMemberHandler : IRequestHandler<AddTeamMemberCommand, Unit>
{
    private readonly ICatalogContext _catalogContext;
    private readonly TimeProvider _timeProvider;
    private readonly IIdentityManager _identityManager;

    public AddTeamMemberHandler(
        ICatalogContext catalogContext, 
        TimeProvider timeProvider, 
        IIdentityManager identityManager)
    {
        _catalogContext = catalogContext;
        _timeProvider = timeProvider;
        _identityManager = identityManager;
    }

    public async Task<Unit> Handle(AddTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var artist = await _catalogContext.Artists
                         .Include(a => a.TeamMembers)
                         .FirstOrDefaultAsync(a => a.Id == request.ArtistId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Artist), request.ArtistId);

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        artist.AddTeamMember(request.UserId, request.Role, utcNow);

        await _identityManager.EnsureManagementRoleAsync(request.UserId, cancellationToken);

        await _catalogContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}