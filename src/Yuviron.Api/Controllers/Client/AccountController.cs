using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yuviron.Application.Features.Auth.Commands.DeleteAccount;
using Yuviron.Application.Features.Client.Users.Commands.UpdateAccountDetails;
using Yuviron.Application.Features.Client.Users.Commands.UpdateMarketingPreferences;
using Yuviron.Application.Features.Client.Users.Commands.UpdateUserProfile;

namespace Yuviron.Api.Controllers.Client;

[Authorize]
[Route("api/me/account")]
[ApiExplorerSettings(GroupName = "client")]
public class AccountController : ApiControllerBase
{
    [HttpPut("profile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request, CancellationToken ct)
    {
        var command = new UpdateUserProfileCommand(request.Name, request.Bio, request.AvatarFileId, request.BannerFileId);
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("details")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateAccountDetails([FromBody] UpdateAccountDetailsCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpPut("marketing")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateMarketingPreferences([FromBody] UpdateMarketingPreferencesCommand command, CancellationToken ct)
    {
        await Mediator.Send(command, ct);
        return NoContent();
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAccount(CancellationToken ct)
    {
        await Mediator.Send(new DeleteAccountCommand(), ct);
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None
        });
        return NoContent();
    }
}
