using MediatR;
namespace Yuviron.Application.Features.Client.Settings.Commands.UpdatePrivacyToggles;
public record UpdatePrivacyTogglesCommand(bool MakePlaylistsPublicByDefault, bool ShowFollowers) : IRequest<Unit>;
