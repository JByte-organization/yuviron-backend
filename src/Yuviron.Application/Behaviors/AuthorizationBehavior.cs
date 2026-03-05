using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Application.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUserContext _userContext;
    private readonly IPermissionService _permissionService;

    public AuthorizationBehavior(IUserContext userContext, IPermissionService permissionService)
    {
        _userContext = userContext;
        _permissionService = permissionService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is ISecuredRequest securedRequest)
        {
            if (!_userContext.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var userPermissions = await _permissionService.GetPermissionsAsync(_userContext.UserId, cancellationToken);

            var requiredPermString = securedRequest.RequiredPermission.ToString();

            if (!userPermissions.Contains(requiredPermString))
            {
                throw new UnauthorizedAccessException($"Access denied. Required: {requiredPermString}");
            }
        }

        return await next();
    }
}