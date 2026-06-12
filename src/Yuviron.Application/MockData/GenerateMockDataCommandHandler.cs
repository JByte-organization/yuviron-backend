using MediatR;
using Yuviron.Application.Abstractions.MockData;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.MockData;

public class GenerateMockDataCommandHandler : IRequestHandler<GenerateMockDataCommand, Unit>
{
    private readonly IMockDataService _mockDataService;

    public GenerateMockDataCommandHandler(IMockDataService mockDataService)
    {
        _mockDataService = mockDataService;
    }

    public async Task<Unit> Handle(GenerateMockDataCommand request, CancellationToken cancellationToken)
    {
        await _mockDataService.GenerateAsync(request.MonthsToGenerate, cancellationToken);
        return Unit.Value;
    }
}
