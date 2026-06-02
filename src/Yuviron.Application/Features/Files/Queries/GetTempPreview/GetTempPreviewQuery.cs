using MediatR;
using System;

namespace Yuviron.Application.Features.Files.Queries.GetTempPreview;

public sealed record GetTempPreviewQuery(
    string FileName,
    Guid UserId 
) : IRequest<GetTempPreviewResponse>;