using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Yuviron.Api.Controllers.StudioArtist;

[ApiExplorerSettings(GroupName = "artist")]
[Authorize(Roles = "ManagementUser")]
public abstract class StudioArtistApiControllerBase : ApiControllerBase 
{
}