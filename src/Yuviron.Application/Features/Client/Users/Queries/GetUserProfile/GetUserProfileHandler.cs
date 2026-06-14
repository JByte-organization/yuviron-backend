using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums; // Перевір, де лежить PlaylistVisibility
using Yuviron.Domain.Exceptions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserProfile;

public sealed class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IIdentityContext _identityContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUserService;

    public GetUserProfileHandler(IIdentityContext identityContext, ILibraryContext libraryContext, ICurrentUserService currentUserService)
    {
        _identityContext = identityContext;
        _libraryContext = libraryContext;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var profile = await _identityContext.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId )
            .Select(u => new UserProfileDto(
                u.Id,
                u.Profile != null ? u.Profile.FirstName : "Unknown User",
                u.Profile != null ? u.Profile.AvatarUrl : null,
                u.Profile != null ? u.Profile.BannerUrl : null, 
                u.Profile != null ? u.Profile.Bio : null,
                
                _libraryContext.UserFollowUsers.Count(ufu => ufu.FolloweeId == u.Id), 
                
                _libraryContext.UserFollowUsers.Count(ufu => ufu.FollowerId == u.Id) + 
                _libraryContext.UserFollowArtists.Count(ufa => ufa.UserId == u.Id),   
                
                _libraryContext.Playlists.Count(p => p.UserId == u.Id && p.Visibility == PlaylistVisibility.Public ), 
                
                currentUserId != null && _libraryContext.UserFollowUsers.Any(ufu => ufu.FollowerId == currentUserId && ufu.FolloweeId == u.Id)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
            throw new NotFoundException(nameof(User), request.UserId);

        return profile;
    }
}