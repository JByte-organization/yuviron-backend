using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.MockData;

public record MockDataGenerationOptions(
    int MonthsToGenerate,
    bool GenerateUsers = true,
    bool GenerateSocial = true,
    bool GenerateAds = true,
    bool GenerateAnalytics = true);

public interface IMockDataService
{
    Task GenerateAsync(
        MockDataGenerationOptions options,
        CancellationToken cancellationToken);
}
