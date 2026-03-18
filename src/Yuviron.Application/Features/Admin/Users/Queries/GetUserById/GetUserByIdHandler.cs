using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.Admin.Users.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Users.Queries.GetUserById;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDetailsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider; // <-- Добавили для премиума

    public GetUserByIdHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<UserDetailsDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var user = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserDetailsDto(
                u.Id,
                u.Email,
                u.AccountState,
                u.AcceptMarketing,
                u.AcceptTerms,
                u.Profile != null ? u.Profile.DisplayName : null,
                u.Profile != null ? u.Profile.AvatarUrl : null,
                u.Profile != null ? u.Profile.Country : null,
                u.Profile != null ? u.Profile.Bio : null,
                u.Profile != null ? u.Profile.DateOfBirth : default,
                u.Profile != null ? u.Profile.Gender : default,
                u.Subscriptions.Any(s => s.Status == SubscriptionStatus.Active && s.EndAt > utcNow), 
                u.CreatedAt,
                u.UpdatedAt,
                u.LastLoginAt,
                u.UserRoles.Select(ur => new RoleSimpleDto(ur.RoleId, ur.Role.Name)).ToList() 
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        return user;
    }
}