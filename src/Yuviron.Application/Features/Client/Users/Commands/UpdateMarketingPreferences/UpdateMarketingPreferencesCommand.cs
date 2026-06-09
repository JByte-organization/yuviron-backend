using MediatR;
namespace Yuviron.Application.Features.Client.Users.Commands.UpdateMarketingPreferences;
public record UpdateMarketingPreferencesCommand(bool AcceptMarketing) : IRequest<Unit>;
