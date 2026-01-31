using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Users.Queries.GetUserProfile;

public class GetUserProfileHandler : IRequestHandler<GetUserProfileQuery, UserProfileDTO>
{
    private readonly IApplicationDbContext _context;

    public GetUserProfileHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDTO> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Profile) 
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        return new UserProfileDTO(
            Id: user.Id,
            Email: user.Email,
            DisplayName: user.Profile?.DisplayName ?? "Unknown User",
            AvatarUrl: user.Profile?.AvatarUrl,
            Bio: user.Profile?.Bio,
            JoinedAt: user.CreatedAt
        );
    }
}