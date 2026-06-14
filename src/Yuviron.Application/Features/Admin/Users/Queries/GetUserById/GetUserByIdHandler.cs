using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
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
    private readonly IIdentityContext _identityContext;
    private readonly TimeProvider _timeProvider; 

    public GetUserByIdHandler(IIdentityContext identityContext, TimeProvider timeProvider)
    {
        _identityContext = identityContext;
        _timeProvider = timeProvider;
    }

    public async Task<UserDetailsDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var user = await _identityContext.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId )
            .Select(u => new UserDetailsDto(
                u.Id,
                u.Email,
                u.AccountState,
                u.AcceptMarketing,
                u.AcceptTerms,
                u.Profile.FirstName,
                u.Profile.AvatarUrl,  
                u.Profile.Country,   
                u.Profile.Bio,        
                u.Profile.DateOfBirth, 
                u.Profile.Gender,    
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