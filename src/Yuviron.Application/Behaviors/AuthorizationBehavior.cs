using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Authentication;

namespace Yuviron.Application.Behaviors;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IUserContext _userContext;

    public AuthorizationBehavior(IUserContext userContext)
    {
        _userContext = userContext;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is ISecuredRequest securedRequest)
        {
            if (!_userContext.IsAuthenticated)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            if (!_userContext.HasPermission(securedRequest.RequiredPermission))
            {
                throw new UnauthorizedAccessException($"Access denied. Required permission: {securedRequest.RequiredPermission}");
            }
        }

        return await next();
    }
}