using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events; 
using Yuviron.Domain.Exceptions;
using Yuviron.Application.Extensions; 

namespace Yuviron.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IIdentityContext _identityContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserHandler(
        IIdentityContext identityContext, ISystemContext systemContext, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser) 
    {
        _identityContext = identityContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var currentAdminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var normalizedEmail = EmailNormalizer.Normalize(request.Email);

        var emailTaken = await _identityContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != request.UserId && u.Email == normalizedEmail, cancellationToken);

        if (emailTaken)
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }

        var user = await _identityContext.Users
            .Include(u => u.Profile)
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }
        
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        string? finalAvatarUrl = user.Profile!.AvatarUrl;
        if (request.AvatarFileId.HasValue)
        {
            var avatarClaim = await _systemContext.ClaimFileAsync(
                request.AvatarFileId.Value, currentAdminId, "image/", "avatars", cancellationToken);

            user.RegisterFileSwapEvents(avatarClaim, user.Profile.AvatarUrl);
            finalAvatarUrl = avatarClaim.FinalPath;
        }

        string? finalBannerUrl = user.Profile!.BannerUrl;
        if (request.BannerFileId.HasValue)
        {
            var bannerClaim = await _systemContext.ClaimFileAsync(
                request.BannerFileId.Value, currentAdminId, "image/", "banners", cancellationToken);

            user.RegisterFileSwapEvents(bannerClaim, user.Profile.BannerUrl);
            finalBannerUrl = bannerClaim.FinalPath;
        }

        user.UpdateAdminDetails(normalizedEmail, request.AcceptMarketing, request.AccountState, utcNow);

        user.Profile!.UpdateDetails(
            request.FirstName.Trim(),
            finalAvatarUrl, 
            finalBannerUrl,
            user.Profile.Country,
            user.Profile.City,
            user.Profile.Bio,
            request.DateOfBirth,
            request.Gender,
            utcNow);

        if (request.RoleIds is not null)
        {
            var requestedRoleIds = request.RoleIds.Where(id => id != Guid.Empty).Distinct().ToHashSet();

            var existingRoleIds = await _identityContext.Roles
                .AsNoTracking()
                .Where(r => requestedRoleIds.Contains(r.Id))
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            if (existingRoleIds.Count != requestedRoleIds.Count)
            {
                throw new NotFoundException(nameof(Role), "One or more role IDs");
            }

            user.SyncRoles(existingRoleIds);
        }

        try
        {
            user.AddDomainEvent(new UserPermissionsChangedEvent(user.Id)); 
            await _identityContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsDuplicateEmailViolation(ex))
        {
            throw new UserAlreadyExistsException(normalizedEmail);
        }

        return Unit.Value;
    }

    private static bool IsDuplicateEmailViolation(DbUpdateException exception)
    {
        if (exception.InnerException is MySqlConnector.MySqlException mySqlEx)
        {
            return mySqlEx.Number == 1062 && 
                   mySqlEx.Message.Contains("email", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}