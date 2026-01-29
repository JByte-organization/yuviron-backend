using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Yuviron.Api.Controllers;

[ApiController]
[Route("api/[controller]")] 
public abstract class ApiControllerBase : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected Guid UserId
    {
        get
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdString, out var id) ? id : Guid.Empty;
        }
    }
}