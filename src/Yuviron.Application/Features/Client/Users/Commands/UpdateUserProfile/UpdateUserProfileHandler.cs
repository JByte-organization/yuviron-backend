using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Application.Policies;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Users.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly TimeProvider _timeProvider;
    private readonly IPermissionService _permissionService;
    private readonly UserSettingsPolicy _userSettingsPolicy; 

    public UpdateUserProfileHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser,
        TimeProvider timeProvider,
        IPermissionService permissionService,
        UserSettingsPolicy userSettingsPolicy)
    {
        _context = context;
        _currentUser = currentUser;
        _timeProvider = timeProvider;
        _permissionService = permissionService;
        _userSettingsPolicy = userSettingsPolicy;
    }

    public async Task Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user?.Profile is null) throw new NotFoundException(nameof(User), userId);

        bool hasAnimatedMediaPermission = await _permissionService.HasPermissionAsync(
            userId, AppPermission.AnimatedMedia, cancellationToken);

        string? finalAvatarUrl = user.Profile.AvatarUrl;
        if (request.AvatarFileId.HasValue)
        {
            await _context.ValidateAnimatedMediaPolicyAsync(
                request.AvatarFileId.Value, 
                hasAnimatedMediaPermission, 
                _userSettingsPolicy, 
                cancellationToken);
            
            var claim = await _context.ClaimFileAsync(request.AvatarFileId.Value, userId, "image/", "avatars", cancellationToken);
            user.Profile.RegisterFileSwapEvents(claim, user.Profile.AvatarUrl);
            finalAvatarUrl = claim.FinalPath;
        }

        string? finalBannerUrl = user.Profile.BannerUrl;
        if (request.BannerFileId.HasValue)
        {
            await _context.ValidateAnimatedMediaPolicyAsync(
                request.BannerFileId.Value, 
                hasAnimatedMediaPermission, 
                _userSettingsPolicy, 
                cancellationToken);
            
            var claim = await _context.ClaimFileAsync(request.BannerFileId.Value, userId, "image/", "banners", cancellationToken);
            user.Profile.RegisterFileSwapEvents(claim, user.Profile.BannerUrl);
            finalBannerUrl = claim.FinalPath;
        }

        user.Profile.UpdateDetails(
            request.Name, finalAvatarUrl, finalBannerUrl, 
            user.Profile.Country, user.Profile.City, request.Bio, 
            user.Profile.DateOfBirth, user.Profile.Gender, utcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
    
}