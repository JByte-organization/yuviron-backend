using MediatR;

namespace Yuviron.Application.MockData;

public record GenerateMockDataCommand(
    int MonthsToGenerate,
    bool GenerateUsers = true,
    bool GenerateSocial = true,
    bool GenerateAds = true,
    bool GenerateAnalytics = true
) : IRequest<Unit>;
