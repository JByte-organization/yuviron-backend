using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Yuviron.Infrastructure.SignalR;

[Authorize]
public sealed class AppHub : Hub<IYuvironClient>
{
}