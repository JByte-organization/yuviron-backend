using Yuviron.Domain.Enums.Monetization;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetPayoutSettings;

public record PayoutSettingsDto(PayoutMethod? Method, string? AccountDetails);