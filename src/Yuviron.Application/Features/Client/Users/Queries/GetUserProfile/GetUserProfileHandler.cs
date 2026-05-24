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
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserProfileHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var profile = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId )
            .Select(u => new UserProfileDto(
                u.Id,
                u.Profile != null ? u.Profile.FirstName : "Unknown User",
                u.Profile != null ? u.Profile.AvatarUrl : null,
                u.Profile != null ? u.Profile.BannerUrl : null, 
                u.Profile != null ? u.Profile.Country : null,
                u.Profile != null ? u.Profile.City : null,
                u.Profile != null ? u.Profile.Bio : null,
                
                _context.UserFollowUsers.Count(ufu => ufu.FolloweeId == u.Id), 
                
                _context.UserFollowUsers.Count(ufu => ufu.FollowerId == u.Id) + 
                _context.UserFollowArtists.Count(ufa => ufa.UserId == u.Id),   
                
                _context.Playlists.Count(p => p.UserId == u.Id && p.Visibility == PlaylistVisibility.Public ), 
                
                currentUserId != null && _context.UserFollowUsers.Any(ufu => ufu.FollowerId == currentUserId && ufu.FolloweeId == u.Id)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (profile == null)
            throw new NotFoundException(nameof(User), request.UserId);

        return profile;
    }
}