using MediatR;

namespace Yuviron.Application.MockData;

public record GenerateMockDataCommand(int MonthsToGenerate) : IRequest<Unit>;
