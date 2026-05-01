using MediatR;

namespace Yuviron.Application.Features.Files.Queries.GetTempPreview;

public sealed record GetTempPreviewQuery(
    string FileName
) : IRequest<GetTempPreviewResponse>;