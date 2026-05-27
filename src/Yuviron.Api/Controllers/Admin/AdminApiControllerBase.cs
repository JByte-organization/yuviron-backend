using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Yuviron.Api.Controllers.Admin;

[ApiExplorerSettings(GroupName = "admin")]
[Authorize( /*Policy = "RequireAdminSession", */Roles = "Admin")]
public abstract class AdminApiControllerBase : ApiControllerBase 
{
  
}