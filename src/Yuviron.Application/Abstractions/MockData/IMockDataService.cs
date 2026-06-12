using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Abstractions.MockData;

public interface IMockDataService
{
    Task GenerateAsync(int monthsToGenerate, CancellationToken cancellationToken);
}
