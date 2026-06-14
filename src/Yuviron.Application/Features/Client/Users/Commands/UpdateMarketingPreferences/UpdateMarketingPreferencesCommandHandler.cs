using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateMarketingPreferences;

public class UpdateMarketingPreferencesCommandHandler : IRequestHandler<UpdateMarketingPreferencesCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICurrentUserService _currentUser;

    public UpdateMarketingPreferencesCommandHandler(IIdentityContext identityContext, ICurrentUserService currentUser)
    {
        _identityContext = identityContext;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateMarketingPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId!.Value;
        var user = await _identityContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null) throw new NotFoundException(nameof(User), userId);

        user.UpdateMarketingPreferences(request.AcceptMarketing, DateTime.UtcNow);
        await _identityContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
